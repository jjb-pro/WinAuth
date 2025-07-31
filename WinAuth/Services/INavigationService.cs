namespace WinAuth.Services;

public interface INavigationService
{
    bool CanGoBack { get; }

    void GoBack();

    void NavigateTo(string pageKey);
    void NavigateTo<TViewModel>();
}