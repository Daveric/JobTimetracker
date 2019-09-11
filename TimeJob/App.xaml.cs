using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace TimeJob
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : ISingleInstanceApp
  {
    private const string Unique = "Job Time Tracker";

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      MainWindow = new MainWindow();
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
      if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
      {
        var application = new App();
        application.InitializeComponent();
        application.Run();

        // Allow single instance code to perform cleanup operations
        SingleInstance<App>.Cleanup();
      }
    }

    #region ISingleInstanceApp Members

    public bool SignalExternalCommandLineArgs(IList<string> args)
    {
      // Bring window to foreground
      var mainWindow = MainWindow;
      if (mainWindow != null && mainWindow.WindowState == WindowState.Minimized)
      {
        if (MainWindow != null) MainWindow.WindowState = WindowState.Normal;
      }

      MainWindow?.Activate();

      return true;
    }

    #endregion ISingleInstanceApp Members
  }
}