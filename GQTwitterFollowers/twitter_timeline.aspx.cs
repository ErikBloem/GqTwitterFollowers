using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GQTwitterFollowers
{
    public partial class twitter_timeline : BasePage
    {
        public BindingList<Twitter.Timeline> ListTimeLine
        {
            get {
                return (BindingList<Twitter.Timeline>)ViewState["timeLine"];
            }
            set { ViewState["timeLine"] = value; }
        }
        private bool databound = false;
        private bool btnNextClicked = false;
        private static string resource_urlFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&count={1}&nclude_rts=1";
        private static string resource_tweetIdFormat = "https://api.twitter.com/1.1/statuses/show/{0}.json";

        protected void Page_init(object sender, EventArgs e)
        {
            Master.GQMenuBar.Command += new CommandEventHandler(GQMenuBar_Command);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Master.Friendorfollower = Constants.FriendsFollowers.Timeline;
            if (IsPostBack) {
                Constants.FriendsFollowers? NullableFriendorFollower = EnumUtils.Parse<Constants.FriendsFollowers>(Master.LblFriendsOrFollowers.Text);
                if (NullableFriendorFollower.HasValue) {
                    Master.Friendorfollower = NullableFriendorFollower.Value;
                }
            } else {
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
                case Constants.FriendsFollowers.Friends:
                case Constants.FriendsFollowers.Followers:
                    Response.Redirect(string.Format(Master.REDIRECTSTRING, Constants.FriendsFollowers.Users.ToString().ToLower(), friendsorfollowers));
                    break;
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            if (this.Page.IsPostBack && ScriptManager.GetCurrent(this.Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "document_Ready", "document_Ready();", true);

            friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
            if (!btnNextClicked) {
                if (cbxTwitterData.Checked && !databound) {
                    btnNext.Text = Resources.Resource.First;
                    btnNext.Visible = true;
                } else
                    GetData(false);
            }
            Master.LblFriendsOrFollowers.Text = friendsorfollowers;
            base.OnLoadComplete(e);
        }

        protected override void TwitterAuthenticate()
        {
            base.TwitterAuthenticate();
            try {
                twdata = new Twitter(Twitter.TwitterType.TimeLine);
            } catch (Exception) {
                //do nothing
            }
            if (twdata != null) {
                try {
                    if (btnNext.Text == Resources.Resource.First) {
                        twdata.FirstCallData(resource_urlFormat, friendsorfollowers, count, string.Empty);
                    } else {
                        if (btnNextClicked && ListTimeLine != null && string.IsNullOrEmpty(txtLastID.Text) && ListTimeLine.Count > 0) {
                            twdata.FirstCallData(resource_urlFormat, friendsorfollowers, count, string.Empty, ListTimeLine[ListTimeLine.Count - 1].ID.ToString());
                        } else {
                            if (string.IsNullOrEmpty(txtLastID.Text)) {
                                twdata.FirstCallData(resource_urlFormat, friendsorfollowers, count, string.Empty);
                            } else {
                                twdata.FirstCallData(resource_urlFormat, friendsorfollowers, count, string.Empty, txtLastID.Text);
                            }
                        }
                    }
                } catch (Exception ex) {
                    //do nothing
                    lblError.Text = ex.Message;
                    lblError.Visible = true;
                } finally {
                    if (twdata != null) ListTimeLine = twdata.listTimeline;
                }
                GetData(true);
                btnNext.Visible = true;
            }
        }

        private void GetData(bool TwitterData)
        {
            try {
                friendsorfollowers = Master.Friendorfollower.ToString();
                //try to serialize
                if (TwitterData) {
                    if (lblError.Visible & ListTimeLine.Count == 0)
                        ListTimeLine = Serializer.ReadTimelineBindingListXML(friendsorfollowers);
                    else if (!lblError.Visible)
                        Serializer.WriteTimelineXML(ListTimeLine.ToList(), friendsorfollowers);
                } else {
                    ListTimeLine = Serializer.ReadTimelineBindingListXML(friendsorfollowers);
                }
            } catch (Exception ex) {
                string exerror = ex.Message;
            }

            int totalTimeLine = 0;
            if (ListTimeLine != null) totalTimeLine = ListTimeLine.Count;

            string screen_name = string.Empty;
            if (twdata != null && twdata.ConsumerCredentials != null) {
                screen_name = twdata.ConsumerCredentials.screen_name;
            } else {
                Twitter.MyConsumerCredentials mycreds = new Twitter.MyConsumerCredentials();
                screen_name = mycreds.screen_name;
            }
            lblTotal.Text = string.Concat(screen_name, Constants.SPACE, Resources.Resource.Has, Constants.SPACE, ListTimeLine.Count, Constants.SPACE, friendsorfollowers);
            BindData();
        }

        private void BindData()
        {
            databound = true;
            gvTimeLine.DataSource = ListTimeLine;
            gvTimeLine.DataBind();
        }

        protected void gvTimeLine_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTimeLine.PageIndex = e.NewPageIndex;
            BindData();
        }

        protected void gvTimeLine_Sorting(object sender, GridViewSortEventArgs e)
        {
            // prop is null at this point, so the next line fails
            if (ListTimeLine.Count > 0) {
                string orderby = gvTimeLine.OrderBy;
                SortDirection direction = SortDirection.Ascending;
                if (orderby.ToLower().EndsWith("desc")) {
                    direction = SortDirection.Descending;
                    orderby = orderby.Split(' ')[0];
                }

                List<Twitter.Timeline> sortedList = null;
                if (orderby == Constants.GetVariableName(() => ListTimeLine[0].Index)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListTimeLine.OrderBy(x => x.Index).ToList();
                    } else {
                        sortedList = ListTimeLine.OrderByDescending(x => x.Index).ToList();
                    }
                } else if (orderby == Constants.GetVariableName(() => ListTimeLine[0].ID)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListTimeLine.OrderBy(x => x.ID).ToList();
                    } else {
                        sortedList = ListTimeLine.OrderByDescending(x => x.ID).ToList();
                    }
                } else if (orderby == Constants.GetVariableName(() => ListTimeLine[0].Text)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListTimeLine.OrderBy(x => x.Text).ToList();
                    } else {
                        sortedList = ListTimeLine.OrderByDescending(x => x.Text).ToList();
                    }
                } else if (orderby == Constants.GetVariableName(() => ListTimeLine[0].Retweeted)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListTimeLine.OrderBy(x => x.Retweeted).ToList();
                    } else {
                        sortedList = ListTimeLine.OrderByDescending(x => x.Retweeted).ToList();
                    }
                }
                ListTimeLine = new BindingList<Twitter.Timeline>(sortedList);
                BindData();
            }
        }

        protected void gvTimeLine_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            switch (e.Row.RowType) {
                case DataControlRowType.Header:
                    break;
                case DataControlRowType.DataRow:
                    Panel pnl = (Panel)e.Row.FindControl("pnlUser");
                    if (pnl != null)
                    {
                        Label lblID = (Label)e.Row.FindControl("lblID");
                        if (lblID != null) {
                            long id = -1;
                            long.TryParse(lblID.Text, out id);
                            Twitter.Timeline tl = ListTimeLine.FirstOrDefault(x => x.ID == id);
                            if (tl != null && tl.User != null) {
                                HtmlGenericControl ianchor = new HtmlGenericControl(Constants.A);
                                ianchor.Attributes.Add(Constants.TARGET, Constants.BLANK);
                                ianchor.Attributes.Add(Constants.TITLE, tl.User.ScreenName);
                                ianchor.Attributes.Add(Constants.HREF, string.Concat(Constants.HTTPTWITTER, tl.User.ScreenName));

                                HtmlImage img = new HtmlImage();
                                img.Src = tl.User.ProfileImage;
                                ianchor.Controls.Add(img);

                                pnl.Controls.Add(ianchor);
                            }
                        }
                    }
                    break;
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (ListTimeLine.Count > 0) {
                bool timelineChanged = false;
                //TwitterAuthenticate();
                try {
                    foreach (GridViewRow row in gvTimeLine.Rows) {
                        Control ctrl = row.FindControl("cbxDelete");
                        if (ctrl != null) {
                            CheckBox cbx = (CheckBox)ctrl;
                            if (cbx.Checked) {
                                Control lblctrl = row.FindControl("lblID");
                                if (lblctrl != null) {
                                    Label lbl = (Label)lblctrl;

                                    long id = -1;
                                    long.TryParse(lbl.Text, out id);
                                    var timeline = ListTimeLine.Where(k => k.ID == id).FirstOrDefault();
                                    if (timeline != null) {
                                        try {
                                            twdata = new Twitter(timeline, Twitter.TwitterType.TimeLine);
                                            ListTimeLine.Remove(timeline);
                                            timelineChanged = true;
                                        } catch (WebException webex) {
                                            if (webex.Message.Contains("(404)") && webex.Message.ToLower().Contains("not found")) {
                                                ListTimeLine.Remove(timeline);
                                                timelineChanged = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (timelineChanged) {
                        friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
                        Serializer.WriteTimelineXML(ListTimeLine.ToList(), friendsorfollowers);
                        ListTimeLine = Serializer.ReadTimelineBindingListXML(friendsorfollowers);
                    }
                    BindData();
                } catch (WebException wex) {
                    BindData();
                    ScriptManager.RegisterStartupScript(this, GetType(), "displayalertmessage", string.Format("alert('{0}');", "No internet connection: " + wex.Message.Replace("'", string.Empty)), true);
                } catch (Exception ex) {
                    //do nothing
                }
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            Master.Friendorfollower = Constants.FriendsFollowers.Timeline;
            btnNextClicked = true;
            TwitterAuthenticate();
            btnNext.Text = Resources.Resource.Next;
            lblCount.Text = (Convert.ToInt32(lblCount.Text) + 1).ToString();
        }

        protected void btnDeleteAllReply_Click(object sender, EventArgs e)
        {
            destroy(true, false);
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            destroy(false, false);
        }

        protected void btnDeleteAllRetweets_Click(object sender, EventArgs e)
        {
            destroy(false, true);
        }

        private void destroy(bool isreply, bool isRetweets)
        {
            if (ListTimeLine != null) {
                try {
                    List<Twitter.Timeline> timelinestoremove = new List<Twitter.Timeline>();
                    if (ListTimeLine.Count > 0) {
                        foreach (Twitter.Timeline timeline in ListTimeLine) {
                            if (isreply) {
                                if (timeline.IsReply) {
                                    try {
                                        twdata = new Twitter(timeline, Twitter.TwitterType.TimeLine);
                                        timelinestoremove.Add(timeline);
                                    } catch (WebException webex) {
                                        if (webex.Message.Contains("(404)") && webex.Message.ToLower().Contains("not found")) {
                                            timelinestoremove.Add(timeline);
                                        }
                                    }
                                }
                            } else {
                                if (isRetweets) {
                                    if (timeline.User != null && timeline.User.ScreenName != Master.screen_name) {
                                        try {
                                            twdata = new Twitter(timeline, Twitter.TwitterType.TimeLine);
                                            timelinestoremove.Add(timeline);
                                        } catch (WebException webex) {
                                            if (webex.Message.Contains("(404)") && webex.Message.ToLower().Contains("not found")) {
                                                timelinestoremove.Add(timeline);
                                            }
                                        }
                                    }
                                } else {
                                    try {
                                        twdata = new Twitter(timeline, Twitter.TwitterType.TimeLine);
                                        timelinestoremove.Add(timeline);
                                    } catch (WebException webex) {
                                        if (webex.Message.Contains("(404)") && webex.Message.ToLower().Contains("not found")) {
                                            timelinestoremove.Add(timeline);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (timelinestoremove.Count > 0) {
                        foreach (Twitter.Timeline timeline in timelinestoremove) {
                            ListTimeLine.Remove(timeline);
                        }
                        friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
                        Serializer.WriteTimelineXML(ListTimeLine.ToList(), friendsorfollowers);
                        ListTimeLine = Serializer.ReadTimelineBindingListXML(friendsorfollowers);
                    }
                    BindData();
                } catch (WebException wex) {
                    BindData();
                    ScriptManager.RegisterStartupScript(this, GetType(), "displayalertmessage", string.Format("alert('{0}');", "No internet connection: " + wex.Message.Replace("'", string.Empty)), true);
                } catch (Exception ex) {
                    //do nothing
                }
            }
        }

        protected void cbxTwitterData_CheckedChanged(object sender, EventArgs e)
        {
            Master.Friendorfollower = Constants.FriendsFollowers.Timeline;
            if (cbxTwitterData.Checked)
                lblCount.Text = "0";
            else
                btnNext.Visible = false;
        }

        protected void btnFindByTweetId_Click(object sender, EventArgs e)
        {
            Master.Friendorfollower = Constants.FriendsFollowers.TweetId;
            btnNextClicked = true;
            if (cbxTweetIdTwitterData.Checked) {
                base.TwitterAuthenticate();
                try {
                    twdata = new Twitter(Twitter.TwitterType.ByTweetId);
                } catch (Exception) {
                    //do nothing
                }
                if (twdata != null) {
                    try {
                        twdata.CallTweetIdData(resource_tweetIdFormat, cbxTweetIdExcelData.Checked);
                    } catch (Exception ex) {
                        //do nothing
                        lblError.Text = ex.Message;
                        lblError.Visible = true;
                    } finally {
                        if (twdata != null) ListTimeLine = twdata.listTimeline;
                    }
                    GetData(true);
                }
            } else {
                GetData(false);
            }
        }
    }
}