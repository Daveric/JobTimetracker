using System;
using GalaSoft.MvvmLight.Command;
using JobTimeTracker.Common;

namespace JobTimeTracker.ViewModel
{
  public class AboutInfoViewModel
  {
    public string AppPath => AppDomain.CurrentDomain.BaseDirectory;

    public string UrlPath => Constants.Url;

    public AboutInfoViewModel()
    {
      CommandPath = new RelayCommand(CmdCommandPath);
    }

    public RelayCommand CommandPath { get; set; }

    private void CmdCommandPath()
    {
      System.Diagnostics.Process.Start("explorer.exe", $"{AppPath}");
    }
  }
}
