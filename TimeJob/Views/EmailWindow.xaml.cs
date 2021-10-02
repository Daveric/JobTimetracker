using JobTimeTracker.Data;
using JobTimeTracker.ViewModel;

namespace JobTimeTracker.Views
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
