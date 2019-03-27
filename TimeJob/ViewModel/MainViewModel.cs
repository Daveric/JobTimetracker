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
using TimeJob.Data;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

namespace TimeJob.ViewModel
{
  public class MainViewModel : ViewModelBase
  {

    #region Fields

    [System.Runtime.InteropServices.DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr handle);

    [System.Runtime.InteropServices.DllImport("User32.dll")]
    private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("User32.dll")]
    private static extern bool IsIconic(IntPtr handle);

    private static readonly string soundPath = @"..\..\SoundFiles\";

    #endregion Fields

    #region Constructor

    public MainViewModel()
    {
      DataAccess.LoadConfiguration(this);
      InitGeneralSettings();

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
        _regularEndTime = _startTime + _timeLunchBreak + TimeSpan.FromHours(WorkingHoursPerWeek / WorkingDaysPerWeek);
        _maximumEndTime = _regularEndTime + TimeSpan.FromHours(2);
        RaisePropertyChanged("RegularEndTime");
        RaisePropertyChanged("MaximumEndTime");
      }
    }

    public string TimeNow
    {
      get => _timeNow.ToString("HH:mm:ss");
      set { _timeNow = Convert.ToDateTime(value); RaisePropertyChanged(); }
    }

    public string RegularEndTime => _regularEndTime.ToString("HH:mm:ss");
    
    public string MaximumEndTime => _maximumEndTime.ToString("HH:mm:ss");

    public string TimeToGo
    {
      get => _timeToGo.ToString(@"hh\:mm\:ss");
      set { _timeToGo = TimeSpan.Parse(value); RaisePropertyChanged();}
    }

    public string TimeToGoMaximum
    {
      get => _timeToGoMaximum.ToString(@"hh\:mm\:ss");
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
      set { _workingDaysPerWeek = value; RaisePropertyChanged(); }
    }

    private TimeSpan _timeAlert;

    public double MinutesAlert
    {
      get => _timeAlert.TotalMinutes;
      set { _timeAlert = TimeSpan.FromMinutes(value); RaisePropertyChanged("MinutesAlertText");}
    }

    public string MinutesAlertText => _timeAlert.ToString(@"hh\:mm");

    private TimeSpan _timeLunchBreak;

    public double MinutesBreak
    {
      get => _timeLunchBreak.TotalMinutes;
      set { _timeLunchBreak = TimeSpan.FromMinutes(value); RaisePropertyChanged("MinutesBreakText");
      }
    }

    public string MinutesBreakText => _timeLunchBreak.ToString(@"hh\:mm");

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

    public List<string> SoundsList { get; set; }

    private string _alertSoundPath;
    private string _selectedAlertSound;

    public string SelectedAlertSound
    {
      get => _selectedAlertSound;
      set
      {
        _alertSoundPath = Path.GetFullPath(value);
        _selectedAlertSound = value;
      }
    }

    private string _lunchSoundPath;
    private string _selectedLunchBreakSound;

    public string SelectedLunchBreakSound
    {
      get => _selectedLunchBreakSound;
      set
      {
        _lunchSoundPath = Path.GetFullPath(value);
        _selectedLunchBreakSound = value;
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

    private bool _isEnabled;

    public bool IsEnabled
    {
      get => _isEnabled && SoundWarning;
      set
      {
        _isEnabled = value;
        RaisePropertyChanged();
      }
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
      }
    }
    public Visibility DisplayConfiguration
    {
      get => DisplayConfig ? Visibility.Visible : Visibility.Hidden;
    }

    #endregion Properties

    #region Commands

    public RelayCommand<Window> CmdCloseWindow { get; private set; }

    private void CmdCloseWindowExecute(Window window)
    {
      window?.Close();
    }

    public RelayCommand CmdHideConfig { get; private set; }

    private void CmdHideConfigExecute()
    {
      DisplayConfig = false;
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
          File.Copy(openFile.FileName, Path.Combine(AssemblyDirectory, soundPath+Path.GetFileName(openFile.FileName)));
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
    }

    public RelayCommand CmdTrackTime { get; private set; }

    private void CmdTrackTimeExecute()
    {

    }

    public RelayCommand CmdConfig { get; private set; }

    private void CmdConfigExecute()
    {
      DisplayConfig = true;
      ChargeSoundFiles();
    }

    public RelayCommand CmdEditMail { get; private set; }

    private void CmdEditMailExecute()
    {
      var win = new EmailWindow();
      win.Show();
    }

    public RelayCommand CmdOpenFileLocation { get; set; }

    private void CmdOpenFileLocationExecute()
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "All files (*.*)|*.*|CSV Files (*.csv)|*.csv"
      };
      if (openFileDialog.ShowDialog() == true)
      {
        TimeLogFileLocationName = Path.GetFileName(openFileDialog.FileName);
      }
    }
    #endregion Commands

    #region Functions

    private void StartTimer()
    {
      var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s, e) => TimeNow = DateTime.Now.ToString("HH:mm:ss");
      timer.Start();
    }

    private void RemainingTimerToGo()
    {
      var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s,e) => TimeToGo = (_regularEndTime - _timeNow).ToString(@"hh\:mm\:ss"); ;
      timer.Start();
    }

    private void RemainingTimerToGoMaximum()
    {
      var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      timer.Tick += (s, e) => TimeToGoMaximum = (_maximumEndTime - _timeNow).ToString(@"hh\:mm\:ss"); ;
      timer.Start();
    }

    public DateTime? GetLastLoggingToMachine()
    {
      var c = new PrincipalContext(ContextType.Machine, Environment.MachineName);
      var uc = UserPrincipal.FindByIdentity(c, Environment.UserName);
      return uc.LastLogon;
    }

    private void InitGeneralSettings()
    {
      WorkingDaysPerWeek = 5;
      StartTime = GetLastLoggingToMachine().Value;
      StartTimer();
      RemainingTimerToGo();
      RemainingTimerToGoMaximum();

      SoundWarning = true;
      ChargeSoundFiles();

      CmdConfig = new RelayCommand(CmdConfigExecute);
      CmdHideConfig = new RelayCommand(CmdHideConfigExecute);
      CmdAddSounds = new RelayCommand(CmdAddSoundsExecute);
      CmdEditMail= new RelayCommand(CmdEditMailExecute);
      CmdOpenFileLocation = new RelayCommand(CmdOpenFileLocationExecute);
    }

    private void ChargeSoundFiles()
    {
      SoundsList = new List<string>();
      var path = Path.Combine(AssemblyDirectory, soundPath);
      string[] files = Directory.GetFiles(path, "*.wav");
      foreach (var sound in files)
      {
        SoundsList.Add(Path.GetFileName(sound));
      }
      if (SoundsList.Count >= 1)
      {
        IsEnabled = true;
      }

      SelectedAlertSound = SoundsList.ElementAt(0);
      SelectedLunchBreakSound = SoundsList.ElementAt(1);
    }

    private void ActivateAlarm()
    {

    }
    #endregion Functions


    public DataTable Data;
    private void LoadCSVOnDataGridView(string fileName)
    {
      try
      {
        ReadCsv csv = new ReadCsv(fileName);

        try
        {
          Data = csv.readCSV;
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
  }

}
