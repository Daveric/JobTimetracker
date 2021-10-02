using TimeJobRecord.Data;
using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Views
{
  public partial class EmailWindow
  {
    public EmailWindow(DataAccess dataAccess)
    {
      InitializeComponent();
      var vm = new EmailViewModel(dataAccess, PasswordBox);
      DataContext = vm;
    }

  }
}
