using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using System.Reflection;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Data;
using System.Linq;
using Microsoft.Win32;
using TimeJobTracker.Data;
using System.Media;
using System.Text;
using System.Windows.Media;
using System.Drawing;
using System.DirectoryServices.AccountManagement;

namespace TimeJobTracker.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    #region Fields
   
    private static readonly string soundPath = @"..\..\SoundFiles\";

    private System.Windows.Forms.NotifyIcon _notifyIcon;

    private static string TimeLoggingFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
      @"TimeJobTracking\TimeLogging.csv");

    private bool _isExit = false;

    #endregion Fields

    #region Constructor

    public MainViewModel()
    {
      DataAccess.LoadConfiguration(this);

      _notifyIcon = new System.Windows.Forms.NotifyIcon();
      _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
      InitGeneralSettings();
      _notifyIcon.Visible = true;
      CreateContextMenu();

      CmdCloseWindow = new RelayCommand<Window>(CmdCloseWindowExecute);
      CmdSaveSettings = new RelayCommand(CmdSaveSettingsExecute);
    }

    #endregion Constructor

    #region Properties

    private DateTime _startTime;
    private DateTime _timeNow;
    private DateTime _regularEndTime;
    private DateTime _maximumEndTime;
    private TimeSpan _timeToGo;
    private TimeSpan _timeToGoMaximum;

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
      set { _timeNow = Convert.ToDateTime(value); RaisePropertyChanged(); }
    }
        
    public string TimeToGo
    {
      get => _timeToGo.ToString();
      set { _timeToGo = TimeSpan.Parse(value); RaisePropertyChanged();}
    }

    public string TimeToGoMaximum
    {
      get => _timeToGoMaximum.ToString();
      set { _timeToGoMaximum = TimeSpan.Parse(value); RaisePropertyChanged(); }
    }

    private int _workingHoursPerWeek;
    public int WorkingHoursPerWeek
    {
      get => _workingHoursPerWeek;
      set { _workingHoursPerWeek = value; RaisePropertyChanged(); }
    }

    private int _workingDaysPerWeek;
    public int WorkingDaysPerWeek
    {
      get => _workingDaysPerWeek;
      set {
            if (value == 0)
            {
              CmdResetExecute();
            }
            else
              _workingDaysPerWeek = value;

            RaisePropertyChanged();
      } 
    }

    private TimeSpan _timeAlert;

    public double MinutesAlert
    {
      get => _timeAlert.TotalMinutes;
      set { _timeAlert = TimeSpan.FromMinutes(value);
        UpdateTimers();
        RaisePropertyChanged("MinutesAlertText");}
    }

    public string MinutesAlertText => _timeAlert.ToString(@"hh\:mm");

    private TimeSpan _timeLunchBreak;

    public double MinutesBreak
    {
      get => _timeLunchBreak.TotalMinutes;
      set {
        _timeLunchBreak = TimeSpan.FromMinutes(value);
        UpdateTimers();
        RaisePropertyChanged("MinutesBreakText");
      }
    }

    public string MinutesBreakText => _timeLunchBreak.ToString(@"hh\:mm");

    public string RegularEndTime => _regularEndTime.ToString("HH:mm:ss");

    public string MaximumEndTime => _maximumEndTime.ToString("HH:mm:ss");

    public SolidColorBrush ColorTime { get; set; }

    public static string AssemblyDirectory
    {
      get
      {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }

    public string _timeLogFileLocation;
    private string _timeLogFileLocationName;
    public string TimeLogFileLocationName
    {
      get => _timeLogFileLocationName;
      set
      {
        _timeLogFileLocationName = value;
        if (value != string.Empty)
          _timeLogFileLocation = Path.GetFullPath(value);
        RaisePropertyChanged();
      }
    }

    public Dictionary<string,string> SoundsDict { get; set; }
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
          if (item.Key == _selectedAlertSound)
          {
            _alertSoundPath = item.Value;
          }
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
          if (item.Key == _selectedWarningSound)
          {
            _warningSoundPath = item.Value;
          }
        }
      }
    }

    private bool _timeLogging;

    public bool TimeLogging
    {
      get => _timeLogging;
      set { _timeLogging = value; RaisePropertyChanged(); }
    }

    private bool _emailCheckBox;

    public bool EmailCheckBox
    {
      get => _emailCheckBox;
      set { _emailCheckBox = value; RaisePropertyChanged("DisplayEmailButton"); }
    }

    public Visibility DisplayEmailButton
    {
      get => EmailCheckBox ? Visibility.Visible : Visibility.Hidden;
    }

    private bool _soundWarning;

    public bool SoundWarning
    {
      get => _soundWarning;
      set { _soundWarning = value; RaisePropertyChanged("IsEnabled"); }
    }

    public string Warning
    {
      get => Properties.Resources.Warning;
    }

    private bool _mininizeOnStartUp;
    public bool MinimizeOnStartUp
    {
      get => _mininizeOnStartUp;
      set { _mininizeOnStartUp = value; RaisePropertyChanged(); }
    }

    private bool _displayConfig;

    private bool DisplayConfig
    {
      get => _displayConfig;
      set
      {
        _displayConfig = value;
        RaisePropertyChanged("DisplayConfiguration");
        RaisePropertyChanged("DisplayDataCSV");
      }
    }

    public Visibility DisplayConfiguration
    {
      get => DisplayConfig ? Visibility.Visible : Visibility.Hidden;
    }
    public Visibility DisplayDataCSV
    {
      get => DisplayConfig ? Visibility.Hidden : Visibility.Visible;
    }

    #endregion Properties

    #region Commands

    public RelayCommand<Window> CmdCloseWindow { get; private set; }

    private void CmdCloseWindowExecute(Window window)
    {
      DataAccess.SaveConfiguration(this);
      window?.Close();
    }

    public RelayCommand CmdHideConfig { get; private set; }

    private void CmdHideConfigExecute()
    {
      DisplayConfig = false;
      SaveDataOnCSVFile(_timeLogFileLocation);
      LoadCSVOnDataGridView(_timeLogFileLocation);
      RaisePropertyChanged("DataCSV");
    }

    public RelayCommand CmdAddSounds { get; private set; }

    private void CmdAddSoundsExecute()
    {
      var openFile = new OpenFileDialog
      {
        Filter = "WAV files (*.wav)|*.wav",
        DefaultExt = ".wav"
      };
      if (openFile.ShowDialog() == true)
      {
        try
        {
          File.Copy(openFile.FileName, Path.Combine(AssemblyDirectory, soundPath + Path.GetFileName(openFile.FileName)));
          ChargeSoundFiles();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, Properties.Resources.Warning);
        }
      }
    }

    public RelayCommand CmdSaveSettings { get; set; }

    private void CmdSaveSettingsExecute()
    {
      DataAccess.SaveConfiguration(this);
      DisplayConfig = false;
      CmdTrackTimeExecute();
      SaveDataOnCSVFile(_timeLogFileLocation);
      LoadCSVOnDataGridView(_timeLogFileLocation);
      RaisePropertyChanged("DataCSV");
    }

    public RelayCommand CmdTrackTime { get; private set; }

    private void CmdTrackTimeExecute()
    {
      if (TimeLogging)
      {
        LoadCSVOnDataGridView(_timeLogFileLocation);
        RaisePropertyChanged("DataCSV");
      }
      else
      {
        MessageBox.Show("TimeLogging is deactivated", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
        DisplayConfig = true;
      }
    }

    public RelayCommand CmdConfig { get; private set; }

    private void CmdConfigExecute()
    {
      DisplayConfig = true;
    }

    public RelayCommand CmdReset { get; private set; }

    private void CmdResetExecute()
    {
      WorkingDaysPerWeek = 5;
      WorkingHoursPerWeek = 40;
      MinutesBreak = 30;
      MinutesAlert = 60;
      _timeLogFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),@"TimeJobTracking\TimeLogging.csv");
      ChargeSoundFiles();

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

    }

    public RelayCommand CmdEditMail { get; private set; }

    private void CmdEditMailExecute()
    {
      var win = new EmailWindow();
      win.Show();
    }

    public RelayCommand CmdOpenLoggingFileLocation { get; set; }

    private void CmdOpenLoggingFileLocationExecute()
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "All files (*.*)|*.*|CSV Files (*.csv)|*.csv"
      };
      if (openFileDialog.ShowDialog() == true)
      {
        TimeLogFileLocationName = Path.GetFullPath(openFileDialog.FileName);
      }
    }
    #endregion Commands

    #region Functions
    
    private void StartTimer()
    {
      var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s, e) => TimeNow = DateTime.Now.ToString(@"HH:mm:ss");
      timer.Start();
    }

    private void RemainingTimerToGo()
    {
      var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s,e) =>
      {
        var timeToGo = _regularEndTime - _timeNow;
        TimeToGo = timeToGo.ToString();

        var timeToGoMaximum = _maximumEndTime - _timeNow;
        TimeToGoMaximum = timeToGoMaximum.ToString();

        UpdateTimersColor(timeToGo);

        ShowPopUpDialog(timeToGo, timeToGoMaximum);
      }; 
      timer.Start();
    }

    private void TimerWarning()
    {
      var timer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s, e) =>
      {
        if (TimeSpan.Compare(TimeSpan.Zero, _regularEndTime - _timeNow) == 0 && SoundWarning)
        {
          //timer.Stop();
          try
          {
            ActivateAlarm(_warningSoundPath);
            MessageBox.Show("You raised the Warning time to work", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      };
      timer.Start();
    }

    private void TimerAlert()
    {
      var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s, e) =>
      {
        if (TimeSpan.Compare(TimeSpan.Zero, _maximumEndTime - _timeNow) == 0 && SoundWarning)
        {
          //timer.Stop();
          try
          {
            ActivateAlarm(_alertSoundPath);
            MessageBox.Show("You raised the Alert time to work, you must go home","Alert",  MessageBoxButton.OK, MessageBoxImage.Exclamation);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      };
      timer.Start();
    }

    public DateTime? GetFirstLoggingToMachine()
    {
      ChekDataCSV(DateTime.Today.ToString(@"d"), out DateTime csvLogon);
      PrincipalContext c = new PrincipalContext(ContextType.Machine, Environment.MachineName);
      UserPrincipal uc = UserPrincipal.FindByIdentity(c, System.Security.Principal.WindowsIdentity.GetCurrent().Name);
      var logon = uc.LastLogon;
      return logon > csvLogon ? csvLogon : logon;
    }

    private void UpdateTimers()
    {
      if ((_workingDaysPerWeek == 0) || (_workingHoursPerWeek == 0))
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
        _notifyIcon.Icon = new Icon("../../Images/warningclock.ico");
      }
      else if (TimeSpan.Compare(-_timeAlert, timeToGo) == 1 || TimeSpan.Compare(-_timeAlert, timeToGo) == 0 && !_isExit)
      {
        ColorTime = new SolidColorBrush(Colors.Red);
        _notifyIcon.Icon = new Icon("../../Images/alertclock.ico");
      }
      else if (!_isExit)
      {
        ColorTime = new SolidColorBrush(Colors.Black);
        _notifyIcon.Icon = new Icon("../../Images/clock.ico");
      }
      RaisePropertyChanged("ColorTime");
    }

    private void ShowPopUpDialog(TimeSpan timeToGo, TimeSpan timeToGoMaximum)
    {
      if ((timeToGo.TotalSeconds >= 0) && (timeToGo.TotalSeconds <= 15) || (timeToGoMaximum.TotalSeconds >= 0) && (timeToGoMaximum.TotalSeconds <= 15))
      {
        var win = new PopUpWindow
        {
          Name = "PopUpWindow",
          DataContext = this
        };
        var wnd = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name.Contains("PopUpWindow") && w.IsVisible);
        if (wnd == null)
        {
          win.Show();
        }
      }
    }

    private void CreateContextMenu()
    {
      _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
      _notifyIcon.ContextMenuStrip.Items.Add("Restore").Click += (s, e) => ShowMainWindow();
      _notifyIcon.ContextMenuStrip.Items.Add("-");
      _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
    }

    private void ExitApplication()
    {
      _isExit = true;
      SaveDataOnCSVFile(_timeLogFileLocation);
      Application.Current.Shutdown();
      _notifyIcon.Dispose();
      _notifyIcon = null;
    }

    private void ShowMainWindow()
    {
      if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
      {
        if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
          Application.Current.MainWindow.WindowState = WindowState.Normal;
        Application.Current.MainWindow.Activate();
      }
      else
        Application.Current.MainWindow?.Show();
    }

    private void InitGeneralSettings()
    {
      LoadCSVOnDataGridView(_timeLogFileLocation);
      StartTime = GetFirstLoggingToMachine().Value;
      StartTimer();
      RemainingTimerToGo();
      ChargeSoundFiles();
      TimerWarning();
      TimerAlert();
      CmdTrackTimeExecute();

      CmdOpenLoggingFileLocation = new RelayCommand(CmdOpenLoggingFileLocationExecute);

      CmdConfig = new RelayCommand(CmdConfigExecute);
      CmdHideConfig = new RelayCommand(CmdHideConfigExecute);
      CmdAddSounds = new RelayCommand(CmdAddSoundsExecute);
      CmdTrackTime = new RelayCommand(CmdTrackTimeExecute);
      CmdEditMail= new RelayCommand(CmdEditMailExecute);

      CmdReset = new RelayCommand(CmdResetExecute);
      CmdDeactivate = new RelayCommand(CmdDeactivateExecute);
      CmdLanguage = new RelayCommand(CmdLanguageExecute);
      CmdAbout = new RelayCommand(CmdAboutExecute);
    }

    private void ChargeSoundFiles()
    {
      SoundsDict = new Dictionary<string, string>();
      SoundsList = new List<string>();
      var path = Path.Combine(AssemblyDirectory, soundPath);
      string[] files = Directory.GetFiles(path, "*.wav");
      foreach (var sound in files)
      {
        SoundsDict.Add(Path.GetFileName(sound), Path.GetFullPath(sound));
        SoundsList.Add(Path.GetFileName(sound));
      }
      if (SoundsDict.Count >= 1)
      {
        SoundWarning = true;
        RaisePropertyChanged("SoundWarning");
      }

      SelectedAlertSound = SoundsDict.Keys.ElementAt(0);
      SelectedWarningSound = SoundsDict.Keys.ElementAt(1);
    }

    private void ActivateAlarm(string soundPath)
    {
      var soundPlayer = new SoundPlayer{SoundLocation = soundPath};
      soundPlayer.Play();
    }

    #endregion Functions

    #region DataCVS

    private DataTable _dataCSV;

    public DataTable DataCSV
    {
      get => _dataCSV;
      set { _dataCSV = value; RaisePropertyChanged(); }
    }

    private void ChekDataCSV(string todayDate, out DateTime value)
    {
      value = DateTime.Now;
      foreach (DataRow row in DataCSV.Rows)
      {
        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
        var fieldLine = string.Join(",", fields);
        if (fieldLine.Contains(todayDate) && DateTime.TryParse(fields.ElementAt(1), out DateTime dateValue))
        {
          value = dateValue;
          break;
        }
      }
    }

    private void SaveDataOnCSVFile(string fileName)
    {
      try
      {
        var csv = new StringBuilder();
        var Date = DateTime.Today.ToString(@"d");
        var End = _timeNow.ToString(@"h:mm:ss tt");
        var Remark = string.Empty;

        IEnumerable<string> columnNames = DataCSV.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        csv.AppendLine(string.Join(",", columnNames));

        DateTime dateValue;
        foreach (DataRow row in DataCSV.Rows)
        {
          IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
          var fieldLine = string.Join(",", fields);
          if ((!fieldLine.Contains(Date)) && DateTime.TryParse(fields.First(), out dateValue) && DateTime.TryParse(fields.ElementAt(1), out dateValue))
          {
            csv.AppendLine(fieldLine);
          }
        }

        var Start = _startTime.ToString(@"h:mm:ss tt");
        var newLine = string.Format("{0},{1},{2},{3}", Date, Start, End, Remark);
        csv.AppendLine(newLine);

        File.WriteAllText(fileName,csv.ToString());
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
    private void LoadCSVOnDataGridView(string fileName)
    {
      try
      {
        if (!File.Exists(fileName) || (fileName == null))
        {
          fileName = TimeLoggingFile;
          DataAccess.EnsureDirectory(fileName);
        }

        var csv = new ImportCsv(fileName);
        try
        {
          DataCSV = csv.ReadCsv;
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
