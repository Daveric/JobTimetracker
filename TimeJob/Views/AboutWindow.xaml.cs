using System.Windows.Navigation;
using JobTimeTracker.ViewModel;

namespace JobTimeTracker.Views
{
  public partial class AboutWindow
  {
    public AboutWindow()
    {
      InitializeComponent();
      var vm = new AboutInfoViewModel();
      DataContext = vm;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      System.Diagnostics.Process.Start(e.Uri.ToString());
    }
  }
}
