using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TimeJobRecord.Common;
using TimeJobRecord.Models;
using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Data
{
  internal class DataAccess
  {
    private static readonly string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TimeJobTracking\Settings.json");

    private static CommonProperties _commonProperties;

    public DataAccess()
    {
      LoadFile();
    }

    private static void LoadFile()
    {
      var fileInfo = new FileInfo(SettingsFile);
      if (!File.Exists(SettingsFile) && !Directory.Exists(fileInfo.DirectoryName) && !File.Exists(fileInfo.ToString()))
      {
        SetDefaultProperties();
        SaveFile();
      }
      else
      {
        var jsonText = File.ReadAllText(SettingsFile);
        _commonProperties = JsonConvert.DeserializeObject<CommonProperties>(jsonText);

      }
    }

    #region Load and Save

    public static void LoadEmailSettings(EmailViewModel viewModel)
    {
      if (_commonProperties == null || _commonProperties.Version != Constants.Version) return;
      RestoreUserDetails(_commonProperties.UserProperties, viewModel);
    }

    public static void LoadConfigurationSettings(MainViewModel viewModel)
    {
      if (_commonProperties == null || _commonProperties.Version != Constants.Version) return;
      RestoreConfiguration(_commonProperties.ConfigurationProperties, viewModel);
    }

    public static void SaveEmailSettings(EmailViewModel viewModel)
    {
      if (_commonProperties == null || _commonProperties.Version != Constants.Version)
      {
        SetDefaultProperties();
      }

      var userProps = new UserProperties
      {
        Body = viewModel.BodyText,
        Subject = viewModel.Subject,
        ContactList = viewModel.EmailContacts.ToList(),
        UserName = viewModel.UserEmail,
        Password = viewModel.UserPassword
      };
      if (_commonProperties != null) _commonProperties.UserProperties = userProps;
      SaveFile();
    }

    public static void SaveConfigurationSettings(MainViewModel viewModel)
    {
      if (_commonProperties == null || _commonProperties.Version != Constants.Version)
      {
        SetDefaultProperties();
      }

      var configProps = new ConfigurationProperties
      {
        MinimizeOnStartUp = viewModel.MinimizeOnStartUp,
        AlertTime = viewModel.MinutesAlert,
        LunchBreakTime = viewModel.MinutesBreak,
        EmailCheckBox = viewModel.EmailCheckBox,
        ExtraTimeWorked = viewModel.ExtraTimeWorked,
        SoundWarning = viewModel.SoundWarning,
        TimeLogging = viewModel.TimeLogging,
        TimeLogFileLocationName = viewModel.TimeLogFileLocationName,
        WorkingDaysPerWeek = viewModel.WorkingDaysPerWeek,
        WorkingHoursPerWeek = viewModel.WorkingHoursPerWeek
      };
      if (_commonProperties != null) _commonProperties.ConfigurationProperties = configProps;
      SaveFile();
    }

    #endregion Load and Save

    private static void RestoreUserDetails(UserProperties userProps, EmailViewModel viewModel)
    {
      viewModel.Subject = userProps.Subject;
      viewModel.BodyText = userProps.Body;
      viewModel.UserEmail = userProps.UserName;
      viewModel.UserPassword = userProps.Password;
      viewModel.EmailContacts = new ObservableCollection<string>(userProps.ContactList);
    }

    private static void RestoreConfiguration(ConfigurationProperties configProps, MainViewModel viewModel)
    {
      viewModel.MinimizeOnStartUp = configProps.MinimizeOnStartUp;
      viewModel.MinutesAlert = configProps.AlertTime;
      viewModel.MinutesBreak = configProps.LunchBreakTime;
      viewModel.EmailCheckBox = configProps.EmailCheckBox;
      viewModel.SoundWarning = configProps.SoundWarning;
      viewModel.ExtraTimeWorked = configProps.ExtraTimeWorked;
      viewModel.WorkingDaysPerWeek = configProps.WorkingDaysPerWeek;
      viewModel.WorkingHoursPerWeek = configProps.WorkingHoursPerWeek;
      viewModel.TimeLogging = configProps.TimeLogging;
      viewModel.TimeLogFileLocationName = configProps.TimeLogFileLocationName;
    }

    #region Functions

    private static void SetDefaultProperties()
    {
      _commonProperties = new CommonProperties();
      var propConfig = new ConfigurationProperties
      {
        AlertTime = 60,
        EmailCheckBox = false,
        ExtraTimeWorked = "0:00:00",
        LunchBreakTime = 30,
        MinimizeOnStartUp = true,
        SoundWarning = true,
        TimeLogFileLocationName = Constants.FileLocationName,
        TimeLogging = true,
        WorkingDaysPerWeek = 5,
        WorkingHoursPerWeek = 8
      };
      var propUser = new UserProperties
      {
        Subject = "Vacations",
        Body = "",
        ContactList = new List<string> { "fmuenter@itworks.ec", "candrade@itworks.ec" },
        UserName = "emaldonado@itworks.ec",
        Password = "Pwd1234"
      };
      _commonProperties.Version = Constants.Version;
      _commonProperties.ConfigurationProperties = propConfig;
      _commonProperties.UserProperties = propUser;
    }

    private static void EnsureDirectory()
    {
      var fileInfo = new FileInfo(SettingsFile);
      if (Directory.Exists(fileInfo.DirectoryName) && File.Exists(fileInfo.ToString())) return;
      Directory.CreateDirectory(fileInfo.DirectoryName ?? throw new InvalidOperationException());
      File.Create(SettingsFile).Dispose();
    }

    private static void SaveFile()
    {
      var jsonText = JsonConvert.SerializeObject(_commonProperties);
      EnsureDirectory();
      File.WriteAllText(SettingsFile, jsonText);
    }

    #endregion Functions
  }
}