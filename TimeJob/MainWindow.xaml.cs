using System.ComponentModel;
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

    protected override void OnClosing(CancelEventArgs e)
    {
      _vm.ExitApplication();
    }
  }
}