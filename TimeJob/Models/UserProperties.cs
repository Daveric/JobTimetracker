using System.Collections.Generic;
using System.Security;

namespace JobTimeTracker.Models
{
  public class UserProperties
  {
    public string UserName { get; set; }

    public SecureString Password { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public List<string> ContactList { get; set; }
  }
}
