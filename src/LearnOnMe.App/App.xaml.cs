using System.Windows;
using LearnOnMe.App.ViewModels;
using LearnOnMe.Data;

namespace LearnOnMe.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var db = new LearnOnMeDatabase(LearnOnMeDatabase.DefaultPath());
        db.Initialize();

        var main = new MainViewModel(new SubjectRepository(db));
        main.Load();

        MainWindow = new MainWindow { DataContext = main };
        MainWindow.Show();
    }
}
