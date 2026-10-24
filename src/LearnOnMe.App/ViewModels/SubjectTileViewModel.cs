using System.Windows.Media;
using LearnOnMe.Core.Models;

namespace LearnOnMe.App.ViewModels;

public sealed class SubjectTileViewModel
{
    public SubjectTileViewModel(Subject subject)
    {
        Subject = subject;
        AccentBrush = CreateBrush(subject.AccentColor);
    }

    public Subject Subject { get; }

    public string Code => Subject.Code;

    public string Name => Subject.Name;

    public Brush AccentBrush { get; }

    private static Brush CreateBrush(string hex)
    {
        Color color;
        try
        {
            color = (Color)ColorConverter.ConvertFromString(hex);
        }
        catch (FormatException)
        {
            color = Colors.SteelBlue;
        }
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
