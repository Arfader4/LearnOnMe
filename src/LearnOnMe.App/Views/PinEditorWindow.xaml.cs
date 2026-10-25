using System.IO;
using System.Windows;
using LearnOnMe.App.Services;
using LearnOnMe.App.ViewModels;
using LearnOnMe.Core.Models;
using LearnOnMe.Core.Pins;
using Microsoft.Win32;

namespace LearnOnMe.App.Views;

public partial class PinEditorWindow : Window
{
    public PinEditorWindow()
    {
        InitializeComponent();
        DarkTitleBar.Apply(this);
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PinEditorViewModel vm)
            {
                vm.CloseRequested += result => DialogResult = result;
            }
        };
        Loaded += (_, _) => TargetBox.Focus();
    }

    private PinEditorViewModel ViewModel => (PinEditorViewModel)DataContext;

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        var current = PinTargets.NormalizePath(vm.Target);
        var startDir = current.Length > 0 && Directory.Exists(current) ? current
            : current.Length > 0 && Directory.Exists(Path.GetDirectoryName(current)) ? Path.GetDirectoryName(current)
            : null;

        if (vm.Type == PinType.Folder)
        {
            var dialog = new OpenFolderDialog { Title = "Wybierz folder", InitialDirectory = startDir ?? "" };
            if (dialog.ShowDialog(this) == true)
            {
                vm.Target = dialog.FolderName;
            }
            return;
        }

        var isApp = vm.Type == PinType.App;
        var fileDialog = new OpenFileDialog
        {
            Title = isApp ? "Wybierz program albo skrót" : "Wybierz plik",
            Filter = isApp
                ? "Programy i skróty|*.exe;*.lnk;*.appref-ms|Wszystkie pliki|*.*"
                : "Wszystkie pliki|*.*",
            // Keep .lnk shortcuts as they are: they carry the right arguments and working folder.
            DereferenceLinks = !isApp,
            // Start-menu shortcuts are the easiest way to find SSMS, Packet Tracer or Visual Studio.
            InitialDirectory = startDir ?? (isApp
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)
                : ""),
        };
        if (fileDialog.ShowDialog(this) == true)
        {
            vm.Target = fileDialog.FileName;
        }
    }
}
