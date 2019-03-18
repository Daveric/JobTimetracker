using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using System.Reflection;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Linq;
using System.Net;
using DevExpress.Mvvm.UI;
using DevExpress.Utils.CommonDialogs;
using Microsoft.Win32;
using TimeJob.Data;
using System.DirectoryServices.AccountManagement;

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
      StartTimer();
      InitGeneralSettings();

      CmdCloseWindow = new RelayCommand<Window>(CmdCloseWindowExecute);
      CmdSaveSettings = new RelayCommand(CmdSaveSettingsExecute);

      DataAccess.LoadConfiguration(this);
    }

    #endregion Constructor

    #region Properties

    private DateTime _starTime;
    public DateTime StarTime
    {
      get => _starTime;
      set { _starTime = value; RaisePropertyChanged(); }
    }

    private string _timeNow;

    public string TimeNow
    {
      get => _timeNow;
      set { _timeNow = value; RaisePropertyChanged(); }
    }
    private int _workingHoursPerWeek;
    public int WorkingHoursPerWeek
    {
      get => _workingHoursPerWeek;
      set { _workingHoursPerWeek = value; RaisePropertyChanged(); }
    }

    private TimeSpan _timeAlert = TimeSpan.FromMinutes(60);

    public double MinutesAlert
    {
      get => _timeAlert.TotalMinutes;
      set { _timeAlert = TimeSpan.FromMinutes(value); RaisePropertyChanged("MinutesAlertText");}
    }

    public string MinutesAlertText
    {
      get => _timeAlert.ToString(@"hh\:mm");
    }

    private TimeSpan _timeLunchBreak = TimeSpan.FromMinutes(45);

    public double MinutesBreak
    {
      get => _timeLunchBreak.TotalMinutes;
      set { _timeLunchBreak = TimeSpan.FromMinutes(value); RaisePropertyChanged("MinutesBreakText");}
    }

    public string MinutesBreakText
    {
      get => _timeLunchBreak.ToString(@"hh\:mm");
    }

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
      var timer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(1)
      };
      timer.Tick += TickEvent;
      timer.Start();
    }

    private void TickEvent(object sender, EventArgs e)
    {
      TimeNow = DateTime.Now.ToString(@"hh:mm:ss tt");
    }

    public static DateTime? GetLastLoggingToMachine()
    {
      var c = new PrincipalContext(ContextType.Machine, Environment.MachineName);
      var uc = UserPrincipal.FindByIdentity(c, Environment.UserName);
      return uc.LastLogon;
    }
    private void InitGeneralSettings()
    {
      _starTime = GetLastLoggingToMachine().Value;
      CmdConfig = new RelayCommand(CmdConfigExecute);
      CmdHideConfig = new RelayCommand(CmdHideConfigExecute);
      CmdAddSounds = new RelayCommand(CmdAddSoundsExecute);
      CmdEditMail= new RelayCommand(CmdEditMailExecute);
      CmdOpenFileLocation = new RelayCommand(CmdOpenFileLocationExecute);
      SoundWarning = true;
      ChargeSoundFiles(); 
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
    #endregion Functions


    public DataTable Data;
    private void LoadCSVOnDataGridView(string fileName)
    {
      try
      {
        ReadCSV csv = new ReadCSV(fileName);

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

  public class ReadCSV
  {
    public DataTable readCSV;

    public ReadCSV(string fileName, bool firstRowContainsFieldNames = true)
    {
      readCSV = GenerateDataTable(fileName, firstRowContainsFieldNames);
    }

    private static DataTable GenerateDataTable(string fileName, bool firstRowContainsFieldNames = true)
    {
      DataTable result = new DataTable();

      if (fileName == "")
      {
        return result;
      }

      string delimiters = ",";
      string extension = Path.GetExtension(fileName);

      if (extension.ToLower() == "txt")
        delimiters = "\t";
      else if (extension.ToLower() == "csv")
        delimiters = ",";

      using (TextFieldParser tfp = new TextFieldParser(fileName))
      {
        tfp.SetDelimiters(delimiters);

        // Get The Column Names
        if (!tfp.EndOfData)
        {
          string[] fields = tfp.ReadFields();

          for (int i = 0; i < fields.Count(); i++)
          {
            if (firstRowContainsFieldNames)
              result.Columns.Add(fields[i]);
            else
              result.Columns.Add("Col" + i);
          }

          // If first line is data then add it
          if (!firstRowContainsFieldNames)
            result.Rows.Add(fields);
        }

        // Get Remaining Rows from the CSV
        while (!tfp.EndOfData)
          result.Rows.Add(tfp.ReadFields());
      }

      return result;
    }
  }

}
