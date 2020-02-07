using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TimeJobRecord.ViewModel;

namespace TimeJobRecord.Data
{
  public class DataAccess
  {
    #region Properties

    private static string SettingsFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TimeJobTracking\Settings.xml");

    #endregion Properties

    #region Load and Save

    public static void LoadSettings(EmailViewModel viewModel)
    {
      if (!File.Exists(SettingsFile))
        EnsureSettings(SettingsFile);
      var xmlDoc = XDocument.Load(SettingsFile);
      if (xmlDoc.Root == null) return;
      var properties = xmlDoc.Root.Elements("Properties");
      properties = properties.Elements("Properties");
      foreach (var item in properties)
      {
        if (item.FirstAttribute.Value == Constants.UserDetails)
        {
          RestoreUserDetails(item, viewModel);
        }
        else if (item.FirstAttribute.Value == Constants.Contacts)
        {
          RestoreContacts(item, viewModel);
        }
        else if (item.FirstAttribute.Value == Constants.EmailDetails)
        {
          RestoreEmailDetails(item, viewModel);
        }
      }
    }

    public static void LoadConfiguration(MainViewModel viewModel)
    {
      if (!File.Exists(SettingsFile) || File.Exists(SettingsFile) && new FileInfo(SettingsFile).Length == 0)
        EnsureSettings(SettingsFile);
      var xmlDoc = XDocument.Load(SettingsFile);
      if (xmlDoc.Root == null) return;
      var properties = xmlDoc.Root.Elements("Properties");
      properties = properties.Elements("Properties");
      foreach (var item in properties)
      {
        if (item.FirstAttribute.Value == Constants.Configuration)
        {
          RestoreConfiguration(item, viewModel);
        }
      }
    }

    public static void SaveSettings(EmailViewModel viewModel)
    {
      WriteSettingProperties(viewModel);
    }

    public static void SaveConfiguration(MainViewModel viewModel)
    {
      WriteConfiguration(viewModel);
    }

    #endregion Load and Save

    #region Write XML

    private static void WriteSettingProperties(EmailViewModel viewModel)
    {
      EnsureDirectory(SettingsFile);

      using (var writer = XmlWriter.Create(SettingsFile, new XmlWriterSettings()
      {
        Indent = true,
        IndentChars = "  "
      }))
      {
        writer.WriteStartDocument(true);
        writer.WriteStartElement("CommonProperties");
        writer.WriteAttributeString("version", "1");

        writer.WriteStartElement("Properties");

        // write properties
        WriteUserDetails(writer, viewModel);
        WriteContacts(writer, viewModel);
        WriteEmailDetails(writer, viewModel);

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
      }
    }

    private static void WriteConfiguration(MainViewModel viewModel)
    {
      EnsureDirectory(SettingsFile);

      using (var writer = XmlWriter.Create(SettingsFile, new XmlWriterSettings()
      {
        Indent = true,
        IndentChars = "  ",
      }))
      {
        writer.WriteStartDocument(true);
        writer.WriteStartElement("CommonProperties");
        writer.WriteAttributeString("version", "1");

        writer.WriteStartElement("Properties");

        WriteConfigurations(writer, viewModel);

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
      }
    }

    private static void WriteUserDetails(XmlWriter writer, EmailViewModel viewModel)
    {
      writer.WriteStartElement("Properties");
      writer.WriteAttributeString("name", Constants.UserDetails);

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.User);
      writer.WriteString(viewModel.UserEmail);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.Password);
      writer.WriteString(viewModel.UserPassword);
      writer.WriteEndElement();

      writer.WriteEndElement();
    }

    private static void WriteContacts(XmlWriter writer, EmailViewModel viewModel)
    {
      writer.WriteStartElement("Properties");
      writer.WriteAttributeString("name", Constants.Contacts);

      foreach (var contact in viewModel.EmailContacts)
      {
        writer.WriteStartElement("Property");
        writer.WriteAttributeString("name", Constants.Contact);
        writer.WriteString(contact);
        writer.WriteEndElement();
      }
      writer.WriteEndElement();
    }

    private static void WriteEmailDetails(XmlWriter writer, EmailViewModel viewModel)
    {
      writer.WriteStartElement("Properties");
      writer.WriteAttributeString("name", Constants.EmailDetails);

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.Subject);
      writer.WriteString(viewModel.Subject);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.FileAttachment);
      writer.WriteString(viewModel.FileAttachmentLocation);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.BodyText);
      writer.WriteString(viewModel.BodyText);
      writer.WriteEndElement();

      writer.WriteEndElement();
    }

    private static void WriteConfigurations(XmlWriter writer, MainViewModel viewModel)
    {
      writer.WriteStartElement("Properties");
      writer.WriteAttributeString("name", Constants.Configuration);

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.ExtraTimeWorked);
      writer.WriteValue(viewModel.ExtraTimeWorked);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.WorkingHoursPerWeek);
      writer.WriteValue(viewModel.WorkingHoursPerWeek);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.WorkingDaysPerWeek);
      writer.WriteValue(viewModel.WorkingDaysPerWeek);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.LunchBreakTime);
      writer.WriteValue(viewModel.MinutesBreak);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.AlertTime);
      writer.WriteValue(viewModel.MinutesAlert);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.TimeLogFileLocationName);
      writer.WriteString(viewModel.TimeLogFileLocationName);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.TimeLogging);
      writer.WriteValue(viewModel.TimeLogging? 1 : 0);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.SoundWarning);
      writer.WriteValue(viewModel.SoundWarning? 1 : 0);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.MinimizeOnStartUp);
      writer.WriteValue(viewModel.MinimizeOnStartUp ? 1 : 0);
      writer.WriteEndElement();

      writer.WriteStartElement("Property");
      writer.WriteAttributeString("name", Constants.EmailCheckBox);
      writer.WriteValue(viewModel.EmailCheckBox ? 1 : 0);
      writer.WriteEndElement();

      writer.WriteEndElement();
    }

    #endregion Write XML

    #region Read XML

    private static void RestoreUserDetails(XElement element, EmailViewModel viewModel)
    {
      foreach (var item in element.Descendants())
      {
        if (item.FirstAttribute.Value == Constants.User)
          viewModel.UserEmail = item.Value;
        if (item.FirstAttribute.Value == Constants.Password)
          viewModel.UserPassword = item.Value;
      }
    }

    private static void RestoreContacts(XElement element, EmailViewModel viewModel)
    {
      viewModel.EmailContacts.Clear();
      foreach (var elem in element.Descendants())
      {
        viewModel.EmailContacts.Add(elem.Value);
      }
    }

    private static void RestoreEmailDetails(XElement element, EmailViewModel viewModel)
    {
      foreach (var elem in element.Descendants())
      {
        if (elem.FirstAttribute.Value == Constants.Subject)
          viewModel.Subject = elem.Value;
        else if (elem.FirstAttribute.Value == Constants.FileAttachment)
          viewModel.FileAttachmentLocation = elem.Value;
        else if (elem.FirstAttribute.Value == Constants.BodyText)
          viewModel.BodyText = elem.Value;
      }
    }

    private static void RestoreConfiguration(XElement element, MainViewModel viewModel)
    {
      foreach (var elem in element.Descendants())
      {
        if (elem.FirstAttribute.Value == Constants.ExtraTimeWorked)
          viewModel.ExtraTimeWorked = elem.Value;
        if (elem.FirstAttribute.Value == Constants.WorkingHoursPerWeek)
          viewModel.WorkingHoursPerWeek = int.Parse(elem.Value);
        if (elem.FirstAttribute.Value == Constants.WorkingDaysPerWeek)
          viewModel.WorkingDaysPerWeek = int.Parse(elem.Value);
        if (elem.FirstAttribute.Value == Constants.LunchBreakTime)
          viewModel.MinutesBreak = double.Parse(elem.Value);
        if (elem.FirstAttribute.Value == Constants.AlertTime)
          viewModel.MinutesAlert = double.Parse(elem.Value);
        if (elem.FirstAttribute.Value == Constants.TimeLogFileLocationName)
          viewModel.TimeLogFileLocationName = elem.Value;
        if (elem.FirstAttribute.Value == Constants.TimeLogging)
        {
          var value = short.Parse(elem.Value);
          viewModel.TimeLogging = value != 0;
        }
        if (elem.FirstAttribute.Value == Constants.SoundWarning)
        {
          var value = short.Parse(elem.Value);
          viewModel.SoundWarning = value != 0;
        }
        if (elem.FirstAttribute.Value == Constants.EmailCheckBox)
        {
          var value = short.Parse(elem.Value);
          viewModel.EmailCheckBox = value != 0;
        }
        if (elem.FirstAttribute.Value == Constants.MinimizeOnStartUp)
        {
          var value = short.Parse(elem.Value);
          viewModel.MinimizeOnStartUp = value != 0;
        }
      }
    }

    #endregion Read XML

    #region Functions

    public static void EnsureDirectory(string path)
    {
      var fileInfo = new FileInfo(path);
      if (Directory.Exists(fileInfo.DirectoryName) && File.Exists(fileInfo.ToString())) return;
      Directory.CreateDirectory(fileInfo.DirectoryName ?? throw new InvalidOperationException());
      File.Create(path).Dispose();

      var line = new StringBuilder();
      line.AppendLine("Date,Start,End,Extra time,Remark (!only one line!)");
      File.WriteAllText(path, line.ToString());
    }

    private static void EnsureSettings(string path)
    {
      var fileInfo = new FileInfo(path);
      if (Directory.Exists(fileInfo.DirectoryName) && File.Exists(fileInfo.ToString()) && new FileInfo(SettingsFile).Length != 0) return;
      if (!Directory.Exists(fileInfo.DirectoryName))
        Directory.CreateDirectory(fileInfo.DirectoryName ?? throw new InvalidOperationException());
      if (!File.Exists(fileInfo.ToString()))
        File.Create(path).Dispose();
      File.WriteAllText(path,Properties.Resources.Settings);
    }

    #endregion Functions
  }
}