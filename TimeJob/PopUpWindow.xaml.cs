using System;
using System.Windows;
using System.Windows.Threading;

namespace TimeJobTracker
{
  public partial class PopUpWindow
  {
    public PopUpWindow()
    {
      InitializeComponent();

        Dispatcher?.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
          var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
          var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
          var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

          Left = corner.X - ActualWidth - 10;
          Top = corner.Y - ActualHeight;
        }));
      }
    
    protected void CmdClose(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
