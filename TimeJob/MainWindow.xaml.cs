using TimeJob.ViewModel;

namespace TimeJob
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      var vm = new MainViewModel();
      DataContext = vm;
    }
  }
}