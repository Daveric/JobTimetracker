using System;
using System.Windows;
using System.Windows.Threading;

namespace JobTimeTracker.Views
{
  public partial class PopUpWindow
  {
    public PopUpWindow()
    {
      InitializeComponent();

        Dispatcher?.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
          var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
          var compositionTarget = PresentationSource.FromVisual(this)?.CompositionTarget;
          if (compositionTarget == null) return;
          var transform = compositionTarget.TransformFromDevice;
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
