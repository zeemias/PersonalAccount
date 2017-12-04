using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalAccount.Models
{
    public class LogRegViewModel
    {
        public LoginViewModel Logins { get; set; } = new LoginViewModel();
        public RegisterViewModel Registers { get; set; } = new RegisterViewModel();
    }
}