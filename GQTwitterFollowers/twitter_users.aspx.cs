using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Web.UI.WebControls;

namespace GQTwitterFollowers
{
    public partial class twitter_users : BasePage
    {
        public string strTwitterFollowers { get; set; }
        public List<Twitter.User> listUsers = new List<Twitter.User>();

        private static string resource_urlFormat = "https://api.twitter.com/1.1/{0}/list.json?screen_name={1}&count={2}&cursor={3}";

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Friendorfollower = Constants.FriendsFollowers.Friends;
            if (!IsPostBack) {
                if (Request.QueryString["friendorfollower"] != null) {
                    string friendorfollower = Request.QueryString["friendorfollower"];
                    if (!string.IsNullOrEmpty(friendorfollower)) {
                        Master.Friendorfollower = (Constants.FriendsFollowers)Enum.Parse(typeof(Constants.FriendsFollowers), friendorfollower, true);
                    }
                }
                Master.GQMenuBar.SelectedItem = Master.Friendorfollower.ToString();
            }

            Thread.Sleep(1000);
        }

        protected void Page_init(object sender, EventArgs e)
        {
            Master.GQMenuBar.Command += new CommandEventHandler(GQMenuBar_Command);
        }

        private void GQMenuBar_Command(object sender, CommandEventArgs e)
        {
            string menubarclicked = e.CommandName.Replace("btn", string.Empty);
            Master.Friendorfollower = (Constants.FriendsFollowers)Enum.Parse(typeof(Constants.FriendsFollowers), menubarclicked, true);
            friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
            switch (Master.Friendorfollower) {
                case Constants.FriendsFollowers.Unfollow:
                case Constants.FriendsFollowers.Exclude:
                case Constants.FriendsFollowers.Analyze:
                case Constants.FriendsFollowers.NotFollowing:
                case Constants.FriendsFollowers.NeverFollow:
                    Response.Redirect(string.Format(Master.REDIRECTSTRING, Constants.FriendsFollowers.Unfollow.ToString().ToLower(), friendsorfollowers));
                    break;
                case Constants.FriendsFollowers.Timeline:
                    Response.Redirect(string.Format(Master.REDIRECTSTRING, Constants.FriendsFollowers.Timeline.ToString().ToLower(), friendsorfollowers));
                    break;
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
            if (cbxTwitterData.Checked)
                TwitterAuthenticate();
            else
                GetData(false);

            Master.LblFriendsOrFollowers.Text = friendsorfollowers;
            base.OnLoadComplete(e);
        }

        protected override void TwitterAuthenticate()
        {
            base.TwitterAuthenticate();
            try {
                twdata = new Twitter(Twitter.TwitterType.Friends);
            } catch (Exception) {
                //do nothing
            }
            if (twdata != null) {
                try {
                    twdata.FirstCallData(resource_urlFormat, friendsorfollowers, count, string.Empty);
                } catch (Exception ex) {
                    //do nothing
                    lblError.Text = ex.Message;
                    lblError.Visible = true;
                } finally {
                    if (twdata != null) listUsers = twdata.listFollowers;
                }
                GetData(true);
            }
        }

        private void GetData(bool TwitterData)
        {
            try {
                friendsorfollowers = Master.Friendorfollower.ToString();
                //try to serialize
                if (TwitterData) {
                    if (lblError.Visible & listUsers.Count == 0)
                        listUsers = Serializer.ReadListXML(friendsorfollowers);
                    else if (!lblError.Visible)
                        Serializer.WriteFollowersXML(listUsers, friendsorfollowers);
                } else {
                    listUsers = Serializer.ReadListXML(friendsorfollowers);
                }
            } catch (Exception ex) {
                string exerror = ex.Message;
            }

            int totalFollowers = 0;
            if (listUsers != null) totalFollowers = listUsers.Count;

            string screen_name = string.Empty;
            if (twdata != null && twdata.ConsumerCredentials != null) {
                screen_name = twdata.ConsumerCredentials.screen_name;
            } else {
                Twitter.MyConsumerCredentials mycreds = new Twitter.MyConsumerCredentials();
                screen_name = mycreds.screen_name;
            }
            lblTotalFollowers.Text = string.Concat(screen_name, Constants.SPACE, Resources.Resource.Has, Constants.SPACE, listUsers.Count, Constants.SPACE, friendsorfollowers);

            Random objRnd = new Random();
            List<Twitter.User> randomFollowers = listUsers.OrderBy(item => objRnd.Next()).ToList<Twitter.User>();

            HtmlGenericControl ul = (HtmlGenericControl)ulTwitterFollowers;
            foreach (Twitter.User tw in randomFollowers)
            {
                HtmlGenericControl li = new HtmlGenericControl(Constants.LI);

                HtmlGenericControl ianchor = new HtmlGenericControl(Constants.A);
                ianchor.Attributes.Add(Constants.TARGET, Constants.BLANK);
                ianchor.Attributes.Add(Constants.TITLE, tw.ScreenName);
                ianchor.Attributes.Add(Constants.HREF, string.Concat(Constants.HTTPTWITTER, tw.ScreenName));

                HtmlImage img = new HtmlImage();
                img.Src = tw.ProfileImage;
                ianchor.Controls.Add(img);

                HtmlGenericControl span = new HtmlGenericControl(Constants.SPAN);
                span.InnerHtml = tw.ScreenName;
                ianchor.Controls.Add(span);

                li.Controls.Add(ianchor);
                ul.Controls.Add(li);
            }
        }
    }
}
