using TimeJobRecord.ViewModel;

namespace TimeJobRecord
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
