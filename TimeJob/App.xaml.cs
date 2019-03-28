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
    private System.Windows.Forms.NotifyIcon _notifyIcon;
    private bool _isExit;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      MainWindow = new MainWindow();
      MainWindow.Closing += MainWindow_Closing;
      _notifyIcon = new System.Windows.Forms.NotifyIcon();
      _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
      _notifyIcon.Icon = new Icon("../../Images/clock.ico");
      _notifyIcon.Visible = true;

      CreateContextMenu();
    }

    private void CreateContextMenu()
    {
      _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
      _notifyIcon.ContextMenuStrip.Items.Add("Restore").Click += (s, e) => ShowMainWindow();
      _notifyIcon.ContextMenuStrip.Items.Add("-");
      _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
    }

    private void ExitApplication()
    {
      _isExit = true;
      MainWindow?.Close();
      _notifyIcon.Dispose();
      _notifyIcon = null;
    }

    private void ShowMainWindow()
    {
      if (MainWindow != null && MainWindow.IsVisible)
      {
        if (MainWindow.WindowState == WindowState.Minimized)
        {
          MainWindow.WindowState = WindowState.Normal;
        }
        MainWindow.Activate();
      }
      else
      {
        MainWindow?.Show();
      }
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
      if (_isExit) return;
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