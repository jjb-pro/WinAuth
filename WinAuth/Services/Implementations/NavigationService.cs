using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace WinAuth.Services.Implementations;

public class NavigationService(Frame contentFrame, Dictionary<string, Type> pageMappings) : INavigationService
{
    public void Configure(string key, Type pageType)
    {
        if (pageMappings.ContainsKey(key))
            throw new ArgumentException($"Key {key} is already configured.");

        pageMappings.Add(key, pageType);
    }

    public bool CanGoBack => contentFrame.CanGoBack;

    public void GoBack()
    {
        if (CanGoBack)
            contentFrame.GoBack();
    }

    public void NavigateTo(string pageKey, TransitionAnimation transitionAnimation = TransitionAnimation.Default)
    {
        if (!pageMappings.TryGetValue(pageKey, out var pageType))
            throw new ArgumentException($"No such page: {pageKey}");

        if (contentFrame.Content.GetType() == pageType) // only navigate if type is not same
            return;

        switch (transitionAnimation)
        {
            case TransitionAnimation.Default:
                contentFrame.Navigate(pageType);
                break;
            case TransitionAnimation.Slide:
                contentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                break;
        }
    }

    public void NavigateTo<TViewModel>(TransitionAnimation transitionAnimation = TransitionAnimation.Default)
    {
        var key = typeof(TViewModel).Name;
        if (!pageMappings.ContainsKey(key))
            throw new ArgumentException($"No page mapping for ViewModel: {key}");

        NavigateTo(key, transitionAnimation);
    }
}