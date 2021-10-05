using System;
using System.Windows;
using JobTimeTracker.ViewModel;

namespace JobTimeTracker.Views
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      var vm = new MainViewModel();
      DataContext = vm;
      if (vm.MinimizeOnStartUp)
      {
        Hide();
      }
    }

    private void CmdMinimizeToTray(object sender, RoutedEventArgs e)
    {
      Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
      if (!(DataContext is MainViewModel vm)) return;
      vm.ExitApplication();
    }
  }
}