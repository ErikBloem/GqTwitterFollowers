using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GQTwitterFollowers.Master
{
    public partial class Twitter : MasterPage
    {
        public string screen_name = string.Empty;
        private Constants.FriendsFollowers friendorfollower = Constants.FriendsFollowers.Friends;
        public string REDIRECTSTRING = "~/twitter_{0}.aspx?friendorfollower={1}";
        public Constants.FriendsFollowers Friendorfollower
        {
            get {
                if (ViewState["friendorfollower"] == null)
                    return friendorfollower;

                return (Constants.FriendsFollowers)ViewState["friendorfollower"];
            }
            set {
                ViewState["friendorfollower"] = value;
            }
        }

        public geoqualMenuBar.GQMenuBar GQMenuBar { get { return GQMenuBar1; } }
        public Label LblFriendsOrFollowers { get { return lblFriendsOrFollowers; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            string screen_name_description = string.Empty;
            string screen_image = string.Empty;

            string configvarserror = "'{0}' not defined in the web config file";
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name)]))
                throw new Exception(string.Format(configvarserror, Constants.GetVariableName(() => screen_name)));
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name_description)]))
                throw new Exception(string.Format(configvarserror, Constants.GetVariableName(() => screen_name_description)));
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_image)]))
                throw new Exception(string.Format(configvarserror, Constants.GetVariableName(() => screen_image)));

            screen_name = ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name)];
            screen_name_description = ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name_description)];
            screen_image = ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_image)];

            ascreenname.HRef = string.Concat(Constants.HTTPTWITTER, screen_name);
            ascreenname.InnerText = screen_name_description;
            ascreennamelink.HRef = ascreenname.HRef;
            UserImage.ImageUrl = screen_image;

            GQMenuBar1.NavMenuXML = Serializer.GetNavMenuXMLPath();
            GQMenuBar1.DataBind();
        }
    }
}