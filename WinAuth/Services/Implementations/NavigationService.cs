using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WinAuth.Services.Implementations;

public class NavigationService : INavigationService
{
    private readonly Frame _contentFrame;
    private readonly Dictionary<string, Type> _pageMappings;

    public NavigationService(Frame contentFrame, Dictionary<string, Type> pageMappings)
    {
        _contentFrame = contentFrame;
        _pageMappings = pageMappings;

        SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
    }

    public void Configure(string key, Type pageType)
    {
        if (_pageMappings.ContainsKey(key))
            throw new ArgumentException($"Key {key} is already configured.");

        _pageMappings.Add(key, pageType);
    }

    public bool CanGoBack => _contentFrame.CanGoBack;

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        if (!CanGoBack)
            return;

        _contentFrame.GoBack();
        e.Handled = true;
    }

    public void GoBack()
    {
        if (CanGoBack)
            _contentFrame.GoBack();
    }

    public void NavigateTo(string pageKey)
    {
        if (!_pageMappings.TryGetValue(pageKey, out var pageType))
            throw new ArgumentException($"No such page: {pageKey}");

        if (_contentFrame.Content.GetType() != pageType) // only navigate if type is not same
            _contentFrame.Navigate(pageType);
    }

    public void NavigateTo<TViewModel>()
    {
        var key = typeof(TViewModel).Name;
        if (!_pageMappings.ContainsKey(key))
            throw new ArgumentException($"No page mapping for ViewModel: {key}");

        NavigateTo(key);
    }
}