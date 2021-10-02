using System;
using System.Windows;
using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Views
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      var vm = new MainViewModel();
      DataContext = vm;
    }

    protected void CmdMinimizeToTray(object sender, RoutedEventArgs e)
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