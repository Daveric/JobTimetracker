using System.Collections.Generic;

namespace TimeJobRecord.Models
{
  public class UserProperties
  {
    public string UserName { get; set; }

    public string Password { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public List<string> ContactList { get; set; }
  }
}
