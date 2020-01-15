using System;
using System.Windows;
using TimeJobTracker.ViewModel;

namespace TimeJobTracker
{
  public partial class MainWindow
  {
    MainViewModel _vm = new MainViewModel();

    public MainWindow()
    {
      InitializeComponent();
      DataContext = _vm;
    }

    protected void CmdMinimizeToTray(object sender, RoutedEventArgs e)
    {
      Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
      _vm.ExitApplication();
    }
  }
}