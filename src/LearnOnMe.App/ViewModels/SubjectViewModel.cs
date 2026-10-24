using System.Windows.Input;

namespace LearnOnMe.App.ViewModels;

/// <summary>Subject card. Pins and the "Nauka" tab arrive in the next stage.</summary>
public sealed class SubjectViewModel(SubjectTileViewModel tile, Action goHome)
{
    public SubjectTileViewModel Tile { get; } = tile;

    public ICommand BackCommand { get; } = new RelayCommand(goHome);
}
