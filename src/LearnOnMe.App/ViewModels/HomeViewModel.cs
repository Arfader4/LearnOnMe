using System.Collections.ObjectModel;
using System.Windows.Input;
using LearnOnMe.Core.Abstractions;

namespace LearnOnMe.App.ViewModels;

/// <summary>Home screen with one tile per subject.</summary>
public sealed class HomeViewModel(ISubjectRepository subjects, Action<SubjectTileViewModel> openSubject)
{
    public ObservableCollection<SubjectTileViewModel> Subjects { get; } = [];

    public ICommand OpenSubjectCommand { get; } =
        new RelayCommand(p => openSubject((SubjectTileViewModel)p!), p => p is SubjectTileViewModel);

    public void Load()
    {
        Subjects.Clear();
        foreach (var subject in subjects.GetAll())
        {
            Subjects.Add(new SubjectTileViewModel(subject));
        }
    }
}
