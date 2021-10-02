namespace JobTimeTracker.Models
{
  public class ConfigurationProperties
  {
    public string ExtraTimeWorked { get; set; }
    public int WorkingHoursPerWeek { get; set; }
    public int WorkingDaysPerWeek { get; set; }
    public double LunchBreakTime { get; set; }
    public double AlertTime { get; set; }
    public string TimeLogFileLocationName { get; set; }
    public bool TimeLogging { get; set; }
    public bool SoundWarning { get; set; }
    public bool EmailCheckBox { get; set; }
    public bool MinimizeOnStartUp { get; set; }
  }
}
