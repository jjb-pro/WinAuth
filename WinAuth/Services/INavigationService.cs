namespace WinAuth.Services;

public enum TransitionAnimation
{
    Default,
    Slide
}

public interface INavigationService
{
    bool CanGoBack { get; }

    void GoBack();

    void NavigateTo(string pageKey, TransitionAnimation transitionAnimation = TransitionAnimation.Default);
    void NavigateTo<TViewModel>(TransitionAnimation transitionAnimation = TransitionAnimation.Default);
}