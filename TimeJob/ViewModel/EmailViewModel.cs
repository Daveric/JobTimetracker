using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TimeJobRecord.Data;

namespace TimeJobRecord.ViewModel
{
  public class EmailViewModel : ViewModelBase
  {
    #region Fields
    //private static readonly int port = 587;
    private readonly DataAccess _dataAccess;
    private readonly PasswordBox _passwordBox;

    #endregion Fields

    #region Constructor

    public EmailViewModel(DataAccess dataAccess, PasswordBox passwordBox)
    {
      InitGeneralSettings();
      EmailContacts = new ObservableCollection<string>();
      _dataAccess = dataAccess;
      _passwordBox = passwordBox;
      _dataAccess.LoadEmailSettings(this);
      CmdSaveEmailSettings = new RelayCommand<Window>(CmdSaveEmailSettingsExecute);
    }
    #endregion Constructor

    #region Properties

    private string _userEmail;

    public string UserEmail
    {
      get => _userEmail;
      set { _userEmail = value; RaisePropertyChanged(); }
    }

    private string _emailContact;
    public string EmailContact
    {
      get => _emailContact;
      set { _emailContact = value; RaisePropertyChanged(); }
    }

    public ObservableCollection<string> EmailContacts { get; set; }

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
    
    private string _bodyText;

    public string BodyText
    {
      get => _bodyText;
      set { _bodyText = value; RaisePropertyChanged(); }
    }
    #endregion Properties

    #region Functions
    private void InitGeneralSettings()
    {
      CmdDeleteEmail = new RelayCommand(CmdDeleteEmailExecute);
      CmdAddEmail = new RelayCommand(CmdAddEmailExecute);
    }

    #endregion Functions

    #region Commands

    public RelayCommand<Window> CmdSaveEmailSettings { get; set; }

    private void CmdSaveEmailSettingsExecute(Window window)
    {
      _dataAccess.SaveEmailSettings(this, _passwordBox.SecurePassword);
      window?.Close();
    }

    public RelayCommand CmdDeleteEmail { get; set; }

    private void CmdDeleteEmailExecute()
    {
      if (_selectedEmail == null) return;
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
