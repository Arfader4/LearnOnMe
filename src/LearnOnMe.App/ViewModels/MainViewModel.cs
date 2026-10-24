using LearnOnMe.Core.Abstractions;

namespace LearnOnMe.App.ViewModels;

/// <summary>Root view model: holds the page currently shown in the window.</summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly HomeViewModel _home;
    private object _currentPage;

    public MainViewModel(ISubjectRepository subjects)
    {
        _home = new HomeViewModel(subjects, OpenSubject);
        _currentPage = _home;
    }

    public object CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public void Load() => _home.Load();

    private void OpenSubject(SubjectTileViewModel tile) =>
        CurrentPage = new SubjectViewModel(tile, GoHome);

    private void GoHome() => CurrentPage = _home;
}
