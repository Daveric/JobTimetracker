using TimeJobTracker.ViewModel;

namespace TimeJobTracker
{
  public partial class EmailWindow
  {
    public EmailWindow()
    {
      InitializeComponent();
      var vm = new EmailViewModel();
      DataContext = vm;
    }

  }
}
