using System;
using System.Collections.ObjectModel;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using TimeJobTracker.Data;

namespace TimeJobTracker.ViewModel
{
  public class EmailViewModel : ViewModelBase
  {
    #region Fields
    //private static readonly int port = 587;

    #endregion Fields

    #region Constructor

    public EmailViewModel()
    {
      InitGeneralSettings();
      EmailContacts = new ObservableCollection<string>();
      DataAccess.LoadSettings(this);
      CmdSaveEmailSettings = new RelayCommand(CmdSaveEmailSettingsExecute);
    }
    #endregion Constructor

    #region Properties

    private string _userEmail;

    public string UserEmail
    {
      get => _userEmail;
      set { _userEmail = value; RaisePropertyChanged(); }
    }

    private string _userPassword;

    public string UserPassword
    {
      get => _userPassword;
      set { _userPassword = value; RaisePropertyChanged(); }
    }

    private string _emailContact;
    public string EmailContact
    {
      get => _emailContact;
      set { _emailContact = value; RaisePropertyChanged(); }
    }

    public ObservableCollection<string> EmailContacts { get; }

    private string _selectedEmail;

    public string SelectedEmail
    {
      get => _selectedEmail;
      set { _selectedEmail = value; RaisePropertyChanged(); }
    }

    private string _subject;

    public string Subject
    {
      get => _subject;
      set { _subject = value; RaisePropertyChanged(); }
    }

    private string _fileAttachmentLocation;

    public string FileAttachmentLocation
    {
      get => _fileAttachmentLocation;
      set { _fileAttachmentLocation = value; RaisePropertyChanged(); }
    }

    private string _bodyText;
    public  string BodyText
    {
      get => _bodyText;
      set { _bodyText = value; RaisePropertyChanged(); }
    }
    #endregion Properties

    #region Functions
    private void InitGeneralSettings()
    {
      CmdDeleteEmail = new RelayCommand(CmdDeleteEmailExecute);
      CmdOpenFileAttachmentLocation = new RelayCommand(CmdOpenFileAttachmentLocationExecute);
      CmdAddEmail = new RelayCommand(CmdAddEmailExecute);
    }

    #endregion Functions

    #region Commands

    public RelayCommand CmdSaveEmailSettings { get; set; }

    private void CmdSaveEmailSettingsExecute()
    {
      DataAccess.SaveSettings(this);
    }

    public RelayCommand CmdDeleteEmail { get; set; }

    private void CmdDeleteEmailExecute()
    {
      if (_selectedEmail != null)
      {
        var index = EmailContacts.IndexOf(_selectedEmail) - 1;
        EmailContacts.Remove(_selectedEmail);
        if (EmailContacts.Count == 0)
        {
          _selectedEmail = null;
          return;
        }
        _selectedEmail = index > 0 ? EmailContacts[index] : EmailContacts[0];
        RaisePropertyChanged($"SelectedEmail");
      }
    }

    public RelayCommand CmdOpenFileAttachmentLocation { get; set; }

    private void CmdOpenFileAttachmentLocationExecute()
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter =
          "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png |PDF (*.pdf) | *.pdf |All files (*.*) | *.* "
      };
      if (openFileDialog.ShowDialog() == true)
        FileAttachmentLocation = openFileDialog.FileName;
    }

    public RelayCommand CmdAddEmail { get; set; }

    private void CmdAddEmailExecute()
    {
      try
      {
        if (!EmailContact.Contains("@") && EmailContact == null) return;
        EmailContacts.Add(EmailContact);
        SelectedEmail = EmailContact;
        EmailContact = string.Empty;
        RaisePropertyChanged($"SelectedEmail");
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, Properties.Resources.Warning);
        throw;
      }
    }
    #endregion Commands
  }
}
