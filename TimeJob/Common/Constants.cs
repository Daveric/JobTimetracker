using System;
using System.IO;

namespace TimeJobRecord.Common
{
  public static class Constants
  {
    public const string Version = "1.0";
    public const string Url = "https://github.com/Daveric/JobTimetracker";
    public const string SoundPath = @"SoundFiles\";
    public const string EndSession = "End Session";
    public static readonly string FileLocationName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TimeJobTracking\TimeLogging.csv");
  }
}
  