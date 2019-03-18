using System;
using System.Collections.Generic;
using System.Windows;

namespace TimeJob
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ISingleInstanceApp
  {
    private const string Unique = "Job Time Tracker";

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
      if (this.MainWindow.WindowState == WindowState.Minimized)
      {
        MainWindow.WindowState = WindowState.Normal;
      }

      MainWindow.Activate();

      return true;
    }

    #endregion ISingleInstanceApp Members
  }
}