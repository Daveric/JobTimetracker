using System.Windows.Navigation;
using TimeJobTracker.ViewModel;

namespace TimeJobTracker
{
  public partial class AboutWindow
  {
    public AboutWindow()
    {
      InitializeComponent();
      var vm = new MainViewModel();
      DataContext = vm;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
    }
  }
}
