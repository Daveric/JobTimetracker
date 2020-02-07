using System;
using System.Windows;

namespace TimeJobRecord
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = ViewModelToUse.Vm;
    }

    protected void CmdMinimizeToTray(object sender, RoutedEventArgs e)
    {
      Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
      ViewModelToUse.Vm.ExitApplication();
    }
  }
}