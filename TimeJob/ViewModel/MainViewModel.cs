using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JobTimeTracker.Common;
using JobTimeTracker.Data;
using JobTimeTracker.Properties;
using JobTimeTracker.Views;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace JobTimeTracker.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    #region Fields

    private readonly NotifyIcon _notifyIcon;
    private readonly DataAccess _dataAccess;
    private readonly Window _currentWindow = Application.Current.MainWindow;

    // flag for exit the application
    private bool _isExit;

    #endregion Fields

    #region Constructor

    public MainViewModel()
    {
      _dataAccess = new DataAccess(this);
      _notifyIcon = new NotifyIcon {Visible = true};
      _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

      RegisterApp();
      InitGeneralSettings();
      CreateContextMenu();

      CmdCloseWindow = new RelayCommand(CmdCloseWindowExecute);
    }

    #endregion Constructor

    #region Properties

    private DateTime _startTime;
    private DateTime _timeNow;
    private DateTime _regularEndTime;
    private DateTime _maximumEndTime;
    private TimeSpan _timeToGo;
    private TimeSpan _timeToGoMaximum;
    private TimeSpan _extraTimeWorked;

    private static string TimeLoggingFile => Constants.FileLocationName;

    public string ExtraTimeWorked
    {
      get =>
        $"{(_extraTimeWorked < TimeSpan.Zero ? "-" : "")}{Math.Abs(_extraTimeWorked.Hours):00}:{Math.Abs(_extraTimeWorked.Minutes):00}:{Math.Abs(_extraTimeWorked.Seconds):00}"
      ;
      set
      {
        _extraTimeWorked = TimeSpan.Parse(value);
        RaisePropertyChanged();
      }
    }

    public DateTime StartTime
    {
      get => _startTime;
      set
      {
        _startTime = value;
        UpdateTimers();
      }
    }

    public string TimeNow
    {
      get => _timeNow.ToString(@"HH:mm:ss");
      set
      {
        _timeNow = Convert.ToDateTime(value);
        RaisePropertyChanged();
      }
    }

    public string TimeToGo
    {
      get => _timeToGo.ToString();
      set
      {
        _timeToGo = TimeSpan.Parse(value);
        RaisePropertyChanged();
      }
    }

    public string TimeToGoMaximum
    {
      get => _timeToGoMaximum.ToString();
      set
      {
        _timeToGoMaximum = TimeSpan.Parse(value);
        RaisePropertyChanged();
      }
    }

    private int _workingHoursPerWeek;

    public int WorkingHoursPerWeek
    {
      get => _workingHoursPerWeek;
      set
      {
        _workingHoursPerWeek = value;
        RaisePropertyChanged();
      }
    }

    private int _workingDaysPerWeek;

    public int WorkingDaysPerWeek
    {
      get => _workingDaysPerWeek;
      set
      {
        _workingDaysPerWeek = value;
        RaisePropertyChanged();
      }
    }

    private TimeSpan _timeAlert;

    public double MinutesAlert
    {
      get => _timeAlert.TotalMinutes;
      set
      {
        _timeAlert = TimeSpan.FromMinutes(value);
        UpdateTimers();
        RaisePropertyChanged("MinutesAlertText");
      }
    }

    public string MinutesAlertText => _timeAlert.ToString(@"hh\:mm");

    private TimeSpan _timeLunchBreak;

    public double MinutesBreak
    {
      get => _timeLunchBreak.TotalMinutes;
      set
      {
        _timeLunchBreak = TimeSpan.FromMinutes(value);
        UpdateTimers();
        RaisePropertyChanged("MinutesBreakText");
      }
    }

    public string MinutesBreakText => _timeLunchBreak.ToString(@"hh\:mm");

    public string RegularEndTime => _regularEndTime.ToString("HH:mm:ss");

    public string MaximumEndTime => _maximumEndTime.ToString("HH:mm:ss");

    public SolidColorBrush ColorTime { get; set; }

    public string TimeLogFileLocation;
    private string _timeLogFileLocationName;

    public string TimeLogFileLocationName
    {
      get => _timeLogFileLocationName;
      set
      {
        _timeLogFileLocationName = value;
        if (value != string.Empty)
          TimeLogFileLocation = Path.GetFullPath(value);
        RaisePropertyChanged();
      }
    }

    public Dictionary<string, string> SoundsDict { get; set; }
    public List<string> SoundsList { get; set; }

    private string _alertSoundPath;
    private string _selectedAlertSound;

    public string SelectedAlertSound
    {
      get => _selectedAlertSound;
      set
      {
        _selectedAlertSound = value;
        foreach (var item in SoundsDict)
        {
          if (item.Key != _selectedAlertSound) continue;
          _alertSoundPath = item.Value;
        }
      }
    }

    private string _warningSoundPath;
    private string _selectedWarningSound;

    public string SelectedWarningSound
    {
      get => _selectedWarningSound;
      set
      {
        _selectedWarningSound = value;
        foreach (var item in SoundsDict)
        {
          if (item.Key != _selectedWarningSound) continue;
          _warningSoundPath = item.Value;
        }
      }
    }

    private bool _timeLogging;

    public bool TimeLogging
    {
      get => _timeLogging;
      set
      {
        _timeLogging = value;
        RaisePropertyChanged();
      }
    }

    private bool _emailCheckBox;

    public bool EmailCheckBox
    {
      get => _emailCheckBox;
      set
      {
        _emailCheckBox = value;
        RaisePropertyChanged("DisplayEmailButton");
      }
    }

    public Visibility DisplayEmailButton => EmailCheckBox ? Visibility.Visible : Visibility.Hidden;

    private bool _soundWarning;

    public bool SoundWarning
    {
      get => _soundWarning;
      set
      {
        _soundWarning = value;
        RaisePropertyChanged();
      }
    }

    public string Warning => Resources.Warning;

    private bool _minimizeOnStartUp;

    public bool MinimizeOnStartUp
    {
      get => _minimizeOnStartUp;
      set
      {
        _minimizeOnStartUp = value;
        RaisePropertyChanged();
      }
    }

    private bool _executeOnStartUp;

    public bool ExecuteOnStartUp
    {
      get => _executeOnStartUp;
      set
      {
        _executeOnStartUp = value;
        RaisePropertyChanged();
      }
    }

    #endregion Properties

    #region Commands

    public RelayCommand CmdCloseWindow { get; }

    private void CmdCloseWindowExecute()
    {
      ExitApplication();
    }

    public RelayCommand CmdAddSounds { get; private set; }

    private void CmdAddSoundsExecute()
    {
      var openFile = new OpenFileDialog
      {
        Filter = "All files (*.*)|*.*|MP3 files (*.mp3)|*.mp3|WAV files (*.wav)|*.wav"
      };
      if (openFile.ShowDialog() != true) return;
      try
      {
        File.Copy(openFile.FileName,
          Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Constants.SoundPath + Path.GetFileName(openFile.FileName)));
        ChargeSoundFiles();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, Resources.Warning);
      }
    }

    public RelayCommand CmdSaveSettings { get; set; }

    private void CmdSaveSettingsExecute()
    {
      _dataAccess.SaveConfigurationSettings(this);
      RegisterApp();
      SaveDataOnCSVFile(TimeLogFileLocation);
      LoadCSVOnDataGridView(TimeLogFileLocation);
      RaisePropertyChanged("DataCsv");
    }

    public RelayCommand CmdTrackTime { get; private set; }

    private void CmdTrackTimeExecute()
    {
      if (TimeLogging)
      {
        LoadCSVOnDataGridView(TimeLogFileLocation);
        RaisePropertyChanged("DataCsv");
      }
      else
      {
        MessageBox.Show("TimeLogging is deactivated", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }

    public RelayCommand CmdExtraTime { get; private set; }

    private void CmdExtraTimeExecute()
    {
      SaveDataOnCSVFile(TimeLogFileLocation);
      LoadCSVOnDataGridView(TimeLogFileLocation);
      RaisePropertyChanged("DataCsv");
      var extraWnd = new ExtraTimeWindow(this);
      extraWnd.ShowDialog();
    }

    public RelayCommand CmdReset { get; private set; }

    private void CmdResetExecute()
    {
      WorkingDaysPerWeek = 5;
      WorkingHoursPerWeek = 40;
      MinutesBreak = 30;
      MinutesAlert = 60;
      TimeLogFileLocation = TimeLoggingFile;
      ChargeSoundFiles();
      SoundWarning = true;

      RaisePropertyChanged("TimeLogFileLocationName");
      RaisePropertyChanged("MinutesAlertText");
      RaisePropertyChanged("MinutesBreakText");
    }

    public RelayCommand CmdDeactivate { get; private set; }

    private void CmdDeactivateExecute()
    {
      SoundWarning = false;
      RaisePropertyChanged("SoundWarning");
    }

    public RelayCommand CmdLanguage { get; private set; }

    private void CmdLanguageExecute()
    {
    }

    public RelayCommand CmdAbout { get; private set; }

    private void CmdAboutExecute()
    {
      var win = new AboutWindow();
      win.ShowDialog();
    }

    public RelayCommand CmdEditMail { get; private set; }

    private void CmdEditMailExecute()
    {
      var win = new EmailWindow(_dataAccess);
      win.Show();
    }

    public RelayCommand CmdOpenLoggingFileLocation { get; set; }

    private void CmdOpenLoggingFileLocationExecute()
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "All files (*.*)|*.*|CSV Files (*.csv)|*.csv"
      };
      if (openFileDialog.ShowDialog() == true) TimeLogFileLocationName = Path.GetFullPath(openFileDialog.FileName);
    }

    #endregion Commands

    #region Functions

    private void InitGeneralSettings()
    {
      LoadCSVOnDataGridView(TimeLogFileLocation);
      var startTime = GetFirstLoginToMachine();
      StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute,
        startTime.Second);

      //Start Timer
      var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
      timer.Tick += (s, e) => TimeNow = DateTime.Now.ToString(@"HH:mm:ss");
      timer.Start();

      RemainingTimerToGo();
      ChargeSoundFiles();
      TimerWarning();
      TimerAlert();
      CmdTrackTimeExecute();

      CmdOpenLoggingFileLocation = new RelayCommand(CmdOpenLoggingFileLocationExecute);
      CmdExtraTime = new RelayCommand(CmdExtraTimeExecute);
      CmdAddSounds = new RelayCommand(CmdAddSoundsExecute);
      CmdTrackTime = new RelayCommand(CmdTrackTimeExecute);
      CmdEditMail = new RelayCommand(CmdEditMailExecute);
      CmdReset = new RelayCommand(CmdResetExecute);
      CmdDeactivate = new RelayCommand(CmdDeactivateExecute);
      CmdLanguage = new RelayCommand(CmdLanguageExecute);
      CmdAbout = new RelayCommand(CmdAboutExecute);
      CmdSaveSettings = new RelayCommand(CmdSaveSettingsExecute);
    }

    private void RegisterApp()
    {
      try
      {
        var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        var curAssembly = Assembly.GetExecutingAssembly();
        var currentAssemblyName = curAssembly.GetName().Name;
        var valueExists = key?.GetSubKeyNames().Contains(currentAssemblyName);
        if (valueExists == false && ExecuteOnStartUp)
        {
          key.SetValue(currentAssemblyName, curAssembly.Location);
        }
        else if (valueExists == true)
        {
          key.DeleteValue(currentAssemblyName);
        }
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, Resources.Warning, MessageBoxButton.OKCancel);
      }
    }

    private void RemainingTimerToGo()
    {
      var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
      timer.Tick += (s, e) =>
      {
        var timeToGo = _regularEndTime - _timeNow;
        var signToGo = "";
        if (timeToGo < TimeSpan.Zero)
          signToGo = "-";

        TimeToGo =
          $"{signToGo}{Math.Abs(timeToGo.Hours):00}:{Math.Abs(timeToGo.Minutes):00}:{Math.Abs(timeToGo.Seconds):00}";

        var timeToGoMaximum = _maximumEndTime - _timeNow;
        var signToGoMaximum = "";
        if (timeToGoMaximum < TimeSpan.Zero)
          signToGoMaximum = "-";

        TimeToGoMaximum =
          $"{signToGoMaximum}{Math.Abs(timeToGoMaximum.Hours):00}:{Math.Abs(timeToGoMaximum.Minutes):00}:{Math.Abs(timeToGoMaximum.Seconds):00}";

        UpdateTimersColor(timeToGo);
        ShowPopUpDialog(timeToGo, timeToGoMaximum);
      };
      timer.Start();
    }

    private void TimerWarning()
    {
      var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
      timer.Tick += (s, e) =>
      {
        var time = _regularEndTime - _timeNow;
        var timeToCompare = new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        if (TimeSpan.Compare(TimeSpan.Zero, timeToCompare) != 0 || !SoundWarning) return;
        timer.Stop();
        try
        {
          ActivateAlarm(_warningSoundPath);
          MessageBox.Show("You raised the Warning time to work", "Warning", MessageBoxButton.OK,
            MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      };
      timer.Start();
    }

    private void TimerAlert()
    {
      var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
      timer.Tick += (s, e) =>
      {
        var time = _maximumEndTime - _timeNow;
        var timeToCompare = new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        if (TimeSpan.Compare(TimeSpan.Zero, timeToCompare) != 0 || !SoundWarning) return;
        timer.Stop();
        try
        {
          ActivateAlarm(_alertSoundPath);
          MessageBox.Show("You raised the Alert time to work, you must go home", "Alert", MessageBoxButton.OK,
            MessageBoxImage.Exclamation);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      };
      timer.Start();
    }

    private DateTime GetFirstLoginToMachine()
    {
      CheckDataCsv(DateTime.Today.ToString(@"d"), out var csvLogon);
      var loginTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
      if (!loginTime.Day.Equals(DateTime.Today.Day)) return csvLogon;
      if (csvLogon.Day.Equals(loginTime.Day)) return csvLogon < loginTime ? csvLogon : loginTime;
      return loginTime;
    }

    private void UpdateTimers()
    {
      if (_workingDaysPerWeek == 0 || _workingHoursPerWeek == 0)
        CmdResetExecute();
      _regularEndTime = _startTime + _timeLunchBreak + TimeSpan.FromHours(_workingHoursPerWeek / _workingDaysPerWeek);
      _maximumEndTime = _regularEndTime + _timeAlert;
      RaisePropertyChanged("RegularEndTime");
      RaisePropertyChanged("MaximumEndTime");
    }

    private void UpdateTimersColor(TimeSpan timeToGo)
    {
      if (TimeSpan.Compare(TimeSpan.Zero, timeToGo) == 1 && TimeSpan.Compare(timeToGo, -_timeAlert) == 1 && !_isExit)
      {
        ColorTime = new SolidColorBrush(Colors.Orange);
        _notifyIcon.Icon = Resources.warningclock;
      }
      else if (TimeSpan.Compare(-_timeAlert, timeToGo) == 1 || TimeSpan.Compare(-_timeAlert, timeToGo) == 0 && !_isExit)
      {
        ColorTime = new SolidColorBrush(Colors.Red);
        _notifyIcon.Icon = Resources.alertclock;
      }
      else if (!_isExit)
      {
        ColorTime = new SolidColorBrush(Colors.Black);
        _notifyIcon.Icon = Resources.clock;
      }

      RaisePropertyChanged("ColorTime");
    }

    private void ShowPopUpDialog(TimeSpan timeToGo, TimeSpan timeToGoMaximum)
    {
      if ((!(timeToGo.TotalSeconds > 13) || !(timeToGo.TotalSeconds < 16)) &&
          (!(timeToGoMaximum.TotalSeconds > 13) || !(timeToGoMaximum.TotalSeconds < 16))) return;
      var wnd = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name.Contains("PopUpWindow"));
      if (wnd != null) return;
      var popUpWindow = new PopUpWindow
      {
        Name = "PopUpWindow",
        DataContext = this
      };
      popUpWindow.Show();
      popUpWindow.BringIntoView();
    }

    private void CreateContextMenu()
    {
      _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
      var menuItem = new ToolStripMenuItem("Maximize", Resources.maximize, (s, e) => ShowMainWindow());
      _notifyIcon.ContextMenuStrip.Items.Add(menuItem);
      menuItem = new ToolStripMenuItem("Minimize", Resources.minimize, (s, e) => MinimizeWindow());
      _notifyIcon.ContextMenuStrip.Items.Add(menuItem);
      _notifyIcon.ContextMenuStrip.Items.Add("-");
      menuItem = new ToolStripMenuItem("Hide", Resources.hide, (s, e) => _currentWindow.Hide());
      _notifyIcon.ContextMenuStrip.Items.Add(menuItem);
      menuItem = new ToolStripMenuItem("Exit", Resources.close, (s, e) => ExitApplication());
      _notifyIcon.ContextMenuStrip.Items.Add(menuItem);
    }

    private void MinimizeWindow()
    {
      if (_currentWindow == null) return;
      _currentWindow.WindowState = WindowState.Minimized;
    }

    public void ExitApplication()
    {
      _dataAccess.SaveConfigurationSettings(this);
      SaveDataOnCSVFile(TimeLogFileLocation);
      _isExit = true;
      Application.Current.Shutdown();
      _notifyIcon?.Dispose();
    }

    private void ShowMainWindow()
    {
      if (_currentWindow == null) return;
      _currentWindow.Show();
      _currentWindow.WindowState = WindowState.Normal;
    }

    private void ChargeSoundFiles()
    {
      SoundsDict = new Dictionary<string, string>();
      SoundsList = new List<string>();
      var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SoundPath);
      if (!Directory.Exists(path)) Directory.CreateDirectory(path);

      using (Stream output = File.OpenWrite($"{path}\\Trumpet.wav"))
      {
        Resources.Trumpet.CopyTo(output);
      }

      using (Stream output = File.OpenWrite($"{path}\\KLAXXON.wav"))
      {
        Resources.KLAXXON.CopyTo(output);
      }

      using (Stream output = File.OpenWrite($"{path}\\RunForrest.wav"))
      {
        Resources.RunForrest.CopyTo(output);
      }

      var files = Directory.GetFiles(path, "*.wav");
      if (files.Length == 0) return;
      foreach (var sound in files)
      {
        SoundsDict.Add(Path.GetFileName(sound), Path.GetFullPath(sound));
        SoundsList.Add(Path.GetFileName(sound));
      }

      RaisePropertyChanged("SoundsList");
      SelectedAlertSound = SoundsDict.Keys.ElementAt(0);
      SelectedWarningSound = SoundsDict.Keys.ElementAt(1);
    }

    private static void ActivateAlarm(string soundPath)
    {
      var soundPlayer = new SoundPlayer {SoundLocation = soundPath};
      soundPlayer.Play();
    }

    #endregion Functions

    #region DataCVS

    private DataTable _dataCsv;

    public DataTable DataCsv
    {
      get => _dataCsv;
      set
      {
        _dataCsv = value;
        RaisePropertyChanged();
      }
    }

    private void CheckDataCsv(string todayDate, out DateTime value)
    {
      value = DateTime.Now;
      for (var i = DataCsv.Rows.Count - 1; i == 0; i--)
      {
        var row = DataCsv.Rows[i];
        var fields = row.ItemArray.Select(field => field.ToString());
        var enumerable = fields as string[] ?? fields.ToArray();
        var fieldLine = string.Join(",", enumerable);
        if (!fieldLine.Contains(todayDate) || !DateTime.TryParse(enumerable.ElementAt(1), out var dateValue) ||
            enumerable.Last().Contains(Constants.EndSession)) continue;
        value = dateValue;
        break;
      }
    }

    private void SaveDataOnCSVFile(string fileName, string remark = "")
    {
      try
      {
        var csv = new StringBuilder();
        var todayDate = DateTime.Today.ToString(@"d");
        var startTime = _startTime.ToString(@"HH:mm:ss tt");
        var endTime = _timeNow.ToString(@"HH:mm:ss tt");

        var extraTime = _timeNow - _regularEndTime;
        var sign = extraTime < TimeSpan.Zero ? "-" : "";
        var timeExtra =
          $"{sign}{Math.Abs(extraTime.Hours):00}:{Math.Abs(extraTime.Minutes):00}:{Math.Abs(extraTime.Seconds):00}";

        _extraTimeWorked = TimeSpan.Zero;

        var columnNames = DataCsv.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        csv.AppendLine(string.Join(",", columnNames));
        foreach (DataRow row in DataCsv.Rows)
        {
          var fields = row.ItemArray.Select(field => field.ToString());
          var enumerable = fields as string[] ?? fields.ToArray();
          var fieldLine = string.Join(",", enumerable);
          if (!fieldLine.Contains(todayDate) && DateTime.TryParse(enumerable.ElementAt(1), out var dateValue) ||
              fieldLine.Contains(todayDate) && DateTime.TryParse(enumerable.ElementAt(1), out dateValue) &&
              dateValue.Hour > 1 && _startTime - dateValue > _timeLunchBreak + _timeAlert)
            csv.AppendLine(fieldLine);
          else if (DateTime.TryParse(enumerable.First(), out dateValue) &&
                   DateTime.TryParse(enumerable.ElementAt(1), out dateValue) &&
                   string.IsNullOrEmpty(remark)) remark = enumerable.Last();

          _extraTimeWorked += TimeSpan.Parse(enumerable[3]);
        }

        RaisePropertyChanged("ExtraTimeWorked");

        var newLine = $"{todayDate},{startTime},{endTime},{timeExtra},{remark}";
        csv.AppendLine(newLine);

        File.WriteAllText(fileName, csv.ToString());
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private static void EnsureDirectory(string path)
    {
      var fileInfo = new FileInfo(path);
      if (Directory.Exists(fileInfo.DirectoryName) && File.Exists(fileInfo.ToString())) return;
      Directory.CreateDirectory(fileInfo.DirectoryName ?? throw new InvalidOperationException());
      File.Create(path).Dispose();

      var line = new StringBuilder();
      line.AppendLine("Date,Start,End,Extra time,Remark (!only one line!)");
      File.WriteAllText(path, line.ToString());
    }

    private void LoadCSVOnDataGridView(string fileName)
    {
      try
      {
        if (!File.Exists(fileName) || fileName == null)
        {
          fileName = TimeLoggingFile;
          EnsureDirectory(fileName);
          TimeLogFileLocationName = fileName;
          RaisePropertyChanged("TimeLogFileLocationName");
        }

        var csv = new ImportCsv(fileName);
        try
        {
          DataCsv = csv.ReadCsv;
        }
        catch (Exception ex)
        {
          throw new Exception(ex.Message);
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    #endregion DataCVS
  }
}