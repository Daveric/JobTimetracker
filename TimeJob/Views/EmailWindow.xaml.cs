using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Views
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
