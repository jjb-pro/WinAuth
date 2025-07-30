using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using ZXing;
using static ZXing.RGBLuminanceSource;

namespace WinAuth.Controls;

public sealed partial class QRCodeScanner : UserControl
{
    public string DetectedCode
    {
        get => (string)GetValue(DetectedCodeProperty);
        set => SetValue(DetectedCodeProperty, value);
    }

    public static readonly DependencyProperty DetectedCodeProperty =
        DependencyProperty.Register(
            nameof(DetectedCode),
            typeof(string),
            typeof(QRCodeScanner),
            new PropertyMetadata(string.Empty));

    public event EventHandler<string> DetectedCodeChanged;
    public event EventHandler<Exception> Failed;

    private readonly MediaCapture _mediaCapture = new();

    private double _width = 640;
    private double _height = 480;

    private bool _isFocusing = false;
    private bool _processScan = false;

    private readonly SemaphoreSlim _readSemaphore = new(1);
    private readonly SemaphoreSlim _scanSemaphore = new(1);

    private readonly DispatcherTimer _focusTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly BarcodeReader _reader = new()
    {
        AutoRotate = true,
        Options = new ZXing.Common.DecodingOptions
        {
            TryInverted = true,
            TryHarder = true,
            PossibleFormats = [BarcodeFormat.QR_CODE]
        }
    };

    public QRCodeScanner()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e) => await Initialize();

    private async Task Initialize()
    {
        await InitializeCamera();
        await InitializePreview();
    }

    private async Task InitializeCamera()
    {
        var frontCamera = default(DeviceInformation);
        var rearCamera = default(DeviceInformation);

        var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        foreach (var device in devices)
        {
            switch (device.EnclosureLocation.Panel)
            {
                case Windows.Devices.Enumeration.Panel.Front:
                    frontCamera = device;
                    break;
                case Windows.Devices.Enumeration.Panel.Back:
                    rearCamera = device;
                    break;
            }
        }

        try
        {
            var settings = new MediaCaptureInitializationSettings
            {
                VideoDeviceId = rearCamera?.Id ?? frontCamera?.Id,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MediaCategory = MediaCategory.Other,
                AudioProcessing = AudioProcessing.Default
            };

            await _mediaCapture.InitializeAsync(settings);

            await SetResolution();

            if (_mediaCapture.VideoDeviceController.FlashControl.Supported)
                _mediaCapture.VideoDeviceController.FlashControl.Auto = false;
        }
        catch (Exception ex)
        {
            Failed?.Invoke(this, ex);
        }
    }

    private async Task SetResolution()
    {
        var properties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
        var maxResolution = 0u;
        var indexMaxResolution = 0;

        for (int i = 0; i < properties.Count; i++)
        {
            var encodingProperties = (VideoEncodingProperties)properties[i];
            if (encodingProperties.Width > maxResolution)
            {
                indexMaxResolution = i;
                maxResolution = encodingProperties.Width;

                _width = encodingProperties.Width;
                _height = encodingProperties.Height;
            }
        }

        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, properties[indexMaxResolution]);
    }

    private async Task InitializePreview()
    {
        if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
        {
            _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            _mediaCapture.SetPreviewMirroring(true);
        }

        var focusControl = _mediaCapture.VideoDeviceController.FocusControl;
        //if (!focusControl.FocusChangedSupported)
        {
            //if (focusControl.Supported)
            {
                _processScan = true;

                VideoCaptureElement.Source = _mediaCapture;
                VideoCaptureElement.Stretch = Stretch.UniformToFill;
                await _mediaCapture.StartPreviewAsync();
                await focusControl.UnlockAsync();

                focusControl.Configure(new FocusSettings { Mode = FocusMode.Auto });
                _focusTimer.Tick += OnFocusTimerTick;
                _focusTimer.Start();
            }
            //else
            {
                Failed?.Invoke(this, new Exception("Auto focus control is not supported on this device"));
            }
        }
        //else
        //{
        //    _processScan = true;

        //    _mediaCapture.FocusChanged += OnFocusChanged;
        //    VideoCaptureElement.Source = _mediaCapture;
        //    VideoCaptureElement.Stretch = Stretch.UniformToFill;
        //    await _mediaCapture.StartPreviewAsync();
        //    await focusControl.UnlockAsync();

        //    var settings = new FocusSettings { Mode = FocusMode.Continuous, AutoFocusRange = AutoFocusRange.FullRange };
        //    focusControl.Configure(settings);

        //    await focusControl.FocusAsync();
        //}
    }

    private async void OnFocusTimerTick(object sender, object e)
    {
        if (_isFocusing || !_processScan)
            return; // if camera is still focusing

        _isFocusing = true;

        await _mediaCapture.VideoDeviceController.FocusControl.FocusAsync();
        await CapturePhotoFromCameraAsync();

        _isFocusing = false;
    }

    private async void OnFocusChanged(MediaCapture sender, MediaCaptureFocusChangedEventArgs args)
    {
        if (_processScan)
            await CapturePhotoFromCameraAsync();
    }

    private async Task CapturePhotoFromCameraAsync()
    {
        if (!_processScan)
            return;

        if (await _readSemaphore.WaitAsync(0))
        {
            try
            {
                var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)_width, (int)_height);
                await _mediaCapture.GetPreviewFrameAsync(videoFrame);

                var bytes = await SaveSoftwareBitmapToBufferAsync(videoFrame.SoftwareBitmap);
                await ScanImageAsync(bytes);
            }
            finally
            {
                _readSemaphore.Release();
            }
        }
    }

    private async Task<byte[]> SaveSoftwareBitmapToBufferAsync(SoftwareBitmap softwareBitmap)
    {
        var bytes = default(byte[]);
        try
        {
            var stream = new InMemoryRandomAccessStream();
            {

                // create an encoder with the desired format
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.IsThumbnailGenerated = false;
                await encoder.FlushAsync();

                bytes = new byte[stream.Size];

                // returns IAsyncOperationWithProgess, so you can add additional progress handling
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
            }
        }

        catch (Exception ex)
        {
            Failed.Invoke(this, ex);
        }

        return bytes;
    }

    private async Task ScanImageAsync(byte[] pixelsArray)
    {
        await _scanSemaphore.WaitAsync();

        try
        {
            if (_processScan)
            {
                var result = _reader.Decode(pixelsArray, (int)_width, (int)_height, BitmapFormat.Unknown);
                
                if (null != result)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (result.Text != DetectedCode)
                        {
                            DetectedCode = result.Text;
                            DetectedCodeChanged?.Invoke(this, DetectedCode);
                        }
                    });
                }
            }
        }
        catch
        {
            // wasn't able to find a barcode
        }
        finally
        {
            _scanSemaphore.Release();
        }
    }

    private async void OnUnloaded(object sender, RoutedEventArgs e) => await Cleanup();

    public async Task Cleanup()
    {
        _processScan = false;
        _focusTimer.Stop();
        _focusTimer.Tick -= OnFocusTimerTick;

        await _mediaCapture.StopPreviewAsync();
        _mediaCapture.FocusChanged -= OnFocusChanged;
    }
}