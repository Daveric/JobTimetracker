using System.Windows;
using TimeJobTracker.ViewModel;

namespace TimeJobTracker
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
  }
}