using System.Windows;
using TimeJob.ViewModel;

namespace TimeJob
{
  public partial class EmailWindow : Window
  {
    public EmailWindow()
    {
      InitializeComponent();
      var vm = new EmailViewModel();
      DataContext = vm;
    }

  }
}
