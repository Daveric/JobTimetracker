using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Views
{
  public partial class ExtraTimeWindow
  {
    public ExtraTimeWindow(MainViewModel dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
    }
  }
}
