using JobTimeTracker.ViewModel;

namespace JobTimeTracker.Views
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
