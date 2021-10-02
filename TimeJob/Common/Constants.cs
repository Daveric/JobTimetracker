
using System;
using System.IO;

namespace TimeJobRecord.Common
{
  public static class Constants
  {
    public const string UserDetails = "UserDetails";
    public const string User = "User";
    public const string Password = "Password";

    public const string Contacts = "Contacts";
    public const string Contact = "Contact";
    public const string EmailDetails = "EmailDetails";
    public const string Subject = "Subject";
    public const string FileAttachment = "FileAttachment";
    public const string BodyText = "BodyText";

    public const string Configuration = "Configuration";
    public const string ExtraTimeWorked = "ExtraTimeWorked";
    public const string WorkingHoursPerWeek = "WorkingHoursPerWeek";
    public const string WorkingDaysPerWeek = "WorkingDaysPerWeek";
    public const string LunchBreakTime = "LunchBreakTime";
    public const string AlertTime = "AlertTime";
    public const string TimeLogFileLocationName = "TimeLogFileLocationName";
    public const string TimeLogging = "TimeLogging";
    public const string SoundWarning = "SoundWarning";
    public const string MinimizeOnStartUp = "MinimizeOnStartUp";
    public const string EmailCheckBox = "EmailCheckBox";

    public const string Version = "1.0";
    public const string Url = "https://github.com/Daveric/JobTimetracker";
    public const string SoundPath = @"SoundFiles\";
    public const string EndSession = "End Session";
    public static readonly string FileLocationName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TimeJobTracking\TimeLogging.csv");
  }
}
  