using System.Windows;
using LearnOnMe.App.Services;

namespace LearnOnMe.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DarkTitleBar.Apply(this);
    }
}
