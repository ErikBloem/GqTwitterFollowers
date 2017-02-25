using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace GQTwitterFollowers
{
    public class BasePage : Page
    {
        // oauth implementation details
        protected string friendsorfollowers = string.Empty;
        protected int count = 200;
        protected Twitter twdata = null;

        protected virtual void TwitterAuthenticate()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => count)]))
            {
                string cnt = ConfigurationManager.AppSettings[Constants.GetVariableName(() => count)];
                if (!string.IsNullOrEmpty(cnt)) int.TryParse(cnt, out count);
            }
        }
    }
}