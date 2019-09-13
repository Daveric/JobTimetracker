using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace TimeJobTracker
{
  /// <inheritdoc>
  ///   <cref></cref>
  /// </inheritdoc>
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    private const string AppGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      MainWindow  = new MainWindow();
      MainWindow.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = true;
      MainWindow?.Hide(); // A hidden window can be shown again, a closed one not
    }

    [STAThread]
    public static void Main()
    {
      using (var mutex = new Mutex(false, "Global\\" + AppGuid))
      {
        if (!mutex.WaitOne(0, false))
        {
          MessageBox.Show("Instance already running");
          return;
        }

        var application = new App();
        application.InitializeComponent();
        application.Run();
      }
    }
  }
}