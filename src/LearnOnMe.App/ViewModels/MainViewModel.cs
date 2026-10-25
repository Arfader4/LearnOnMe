using LearnOnMe.App.Services;
using LearnOnMe.Core.Abstractions;

namespace LearnOnMe.App.ViewModels;

/// <summary>Root view model: holds the page currently shown in the window.</summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly HomeViewModel _home;
    private readonly IPinRepository _pins;
    private readonly IDialogService _dialogs;
    private object _currentPage;

    public MainViewModel(ISubjectRepository subjects, IPinRepository pins, IDialogService dialogs)
    {
        _pins = pins;
        _dialogs = dialogs;
        _home = new HomeViewModel(subjects, OpenSubject);
        _currentPage = _home;
    }

    public object CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public void Load() => _home.Load();

    private void OpenSubject(SubjectTileViewModel tile)
    {
        var page = new SubjectViewModel(tile, _pins, _dialogs, GoHome);
        page.Load();
        CurrentPage = page;
    }

    private void GoHome() => CurrentPage = _home;
}
