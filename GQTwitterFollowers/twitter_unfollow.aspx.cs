using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GQTwitterFollowers
{
    public partial class twitter_unfollow : BasePage
    {
        public BindingList<Twitter.User> ListUsers
        {
            get {
                return (BindingList<Twitter.User>)ViewState["listUsers"];
            }
            set { ViewState["listUsers"] = value; }
        }

        public List<Twitter.User> ListFriends
        {
            get {
                if ((List<Twitter.User>) ViewState["listFriends"] == null) {
                    List<Twitter.User> twitterusers = Serializer.ReadListXML(Constants.FriendsFollowers.Friends.ToString());
                    ViewState["listFriends"] = twitterusers;
                    return twitterusers;
                }
                return (List<Twitter.User>)ViewState["listFriends"];
            }
            set { ViewState["listFriends"] = value; }
        }

        public List<Twitter.User> ListFollowers
        {
            get {
                if ((List<Twitter.User>)ViewState["listFollowers"] == null) {
                    List<Twitter.User> twitterusers = Serializer.ReadListXML(Constants.FriendsFollowers.Followers.ToString());
                    ViewState["listFollowers"] = twitterusers;
                    return twitterusers;
                }
                return (List<Twitter.User>)ViewState["listFollowers"];
            }
            set { ViewState["listFollowers"] = value; }
        }

        public Dictionary<string, SortDirection> dir
        {
            get {
                if (ViewState["dirState"] == null) {
                    ViewState["dirState"] = new Dictionary<string, SortDirection>();
                }
                return (Dictionary<string, SortDirection>)ViewState["dirState"];
            }
            set
            {
                ViewState["dirState"] = value;
            }
        }

        protected void Page_init(object sender, EventArgs e)
        {
            Master.GQMenuBar.Command += new CommandEventHandler(GQMenuBar_Command);
        }

        private void GQMenuBar_Command(object sender, CommandEventArgs e)
        {
            if (e != null) {
                string menubarclicked = e.CommandName.Replace("btn", string.Empty);
                Master.Friendorfollower = (Constants.FriendsFollowers)Enum.Parse(typeof(Constants.FriendsFollowers), menubarclicked, true);
            }
            friendsorfollowers = Master.Friendorfollower.ToString().ToLower();
            ListUsers = Serializer.ReadBindingListXML(Master.Friendorfollower.ToString());
            switch (Master.Friendorfollower) {
                case Constants.FriendsFollowers.Friends:
                case Constants.FriendsFollowers.Followers:
                    Response.Redirect(string.Format(Master.REDIRECTSTRING, Constants.FriendsFollowers.Users.ToString().ToLower(), friendsorfollowers));
                    break;
                case Constants.FriendsFollowers.Exclude:
                    ListUsers = Serializer.ReadBindingListXML(Master.Friendorfollower.ToString());
                    break;
                case Constants.FriendsFollowers.Analyze:
                case Constants.FriendsFollowers.Unfollow:
                    //analyze automatically
                    if (ListFriends != null & ListFollowers != null) {
                        //create destroylist
                        ListUsers = CreateDestroyList.GetDestroyList(ListFriends, ListFollowers);
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Unfollow.ToString());
                    } else {
                        //ListUsers = Serializer.ReadDestroyListExcel(Master.friendorfollower.ToString());
                    }
                    break;
                case Constants.FriendsFollowers.NeverFollow:
                    ListUsers = Serializer.ReadBindingListXML(Master.Friendorfollower.ToString());
                    break;
                case Constants.FriendsFollowers.NotFollowing:
                    if (ListFriends != null & ListFollowers != null) {
                        ListUsers = CreateDestroyList.GetNotFollowingList(ListFriends, ListFollowers);
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NotFollowing.ToString());
                    }
                    break;
            }
            StartBinding();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) {
                Master.Friendorfollower = Constants.FriendsFollowers.Unfollow;
                if (Request.QueryString["friendorfollower"] != null) {
                    string friendorfollower = Request.QueryString["friendorfollower"];
                    if (!string.IsNullOrEmpty(friendorfollower)) {
                        Master.Friendorfollower = (Constants.FriendsFollowers)Enum.Parse(typeof(Constants.FriendsFollowers), friendorfollower, true);
                    }
                }
                Master.GQMenuBar.SelectedItem = Master.Friendorfollower.ToString();

                switch (Master.Friendorfollower) {
                    case Constants.FriendsFollowers.Analyze:
                    case Constants.FriendsFollowers.Unfollow:
                        ListUsers = Serializer.ReadBindingListXML(Master.Friendorfollower.ToString());
                        if (ListUsers == null)
                            GQMenuBar_Command(Master.GQMenuBar, null);
                        else
                            StartBinding();
                        break;
                    default:
                        GQMenuBar_Command(Master.GQMenuBar, null);
                        break;
                }

            }
        }

        private void StartBinding()
        {
            Dictionary<string, SortDirection> sorting = new Dictionary<string, SortDirection>();
            sorting.Add(Constants.GetVariableName(() => ListUsers[0].Index), SortDirection.Ascending);
            sorting.Add(Constants.GetVariableName(() => ListUsers[0].ScreenName), SortDirection.Ascending);
            sorting.Add(Constants.GetVariableName(() => ListUsers[0].ProfileImage), SortDirection.Ascending);
            sorting.Add(Constants.GetVariableName(() => ListUsers[0].UserId), SortDirection.Ascending);
            dir = sorting;

            BindData();

        }

        protected override void OnLoadComplete(EventArgs e)
        {
            friendsorfollowers = Master.Friendorfollower.ToString();

            Master.LblFriendsOrFollowers.Text = friendsorfollowers;
            base.OnLoadComplete(e);
        }

        private void BindData()
        {
            gvUsers.DataSource = ListUsers;
            gvUsers.DataBind();
        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            BindData();
        }

        protected void gvUsers_Sorting(object sender, GridViewSortEventArgs e)
        {
            // prop is null at this point, so the next line fails
            if (ListUsers.Count > 0)
            {
                string orderby = gvUsers.OrderBy;
                SortDirection direction = SortDirection.Ascending;
                if (orderby.ToLower().EndsWith("desc")) {
                    direction = SortDirection.Descending;
                    orderby = orderby.Split(' ')[0];
                }

                List<Twitter.User> sortedList = null;
                if (orderby == Constants.GetVariableName(() => ListUsers[0].Index)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListUsers.OrderBy(x => x.Index).ToList();
                    } else {
                        sortedList = ListUsers.OrderByDescending(x => x.Index).ToList();
                    }
                }
                else if(orderby == Constants.GetVariableName(() => ListUsers[0].ScreenName)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListUsers.OrderBy(x => x.ScreenName).ToList();
                    } else {
                        sortedList = ListUsers.OrderByDescending(x => x.ScreenName).ToList();
                    }
                } else if (orderby == Constants.GetVariableName(() => ListUsers[0].ProfileImage)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListUsers.OrderBy(x => x.ProfileImage).ToList();
                    } else {
                        sortedList = ListUsers.OrderByDescending(x => x.ProfileImage).ToList();
                    }
                } else if (orderby == Constants.GetVariableName(() => ListUsers[0].UserId)) {
                    if (direction == SortDirection.Ascending) {
                        sortedList = ListUsers.OrderBy(x => x.UserId).ToList();
                    } else {
                        sortedList = ListUsers.OrderByDescending(x => x.UserId).ToList();
                    }
                }
                ListUsers = new BindingList<Twitter.User>(sortedList);
                BindData();
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            switch (e.Row.RowType) {
                case DataControlRowType.Header:
                    break;
                case DataControlRowType.DataRow:
                    Control ctrl = e.Row.FindControl("liImage");
                    if (ctrl != null) {
                        HtmlGenericControl li = (HtmlGenericControl)ctrl;
                        if (li.Parent is DataControlFieldCell) {
                            DataControlFieldCell td = (DataControlFieldCell)li.Parent;
                            td.Attributes.Add(Constants.CLASS, string.Concat(Constants.GRIDIMAGETD, Constants.SPACE, Constants.GRIDCHECKBOXTD));
                        }

                        Control ctrlScreenName = e.Row.FindControl("lblScreenName");
                        if (ctrlScreenName != null) {
                            Label lblScreenName = (Label)ctrlScreenName;
                            Control ctrlProfileImage = e.Row.FindControl("lblProfileImage");
                            if (ctrlProfileImage != null) {
                                Label lblProfileImage = (Label)ctrlProfileImage;

                                Twitter.User tw = new Twitter.User();
                                tw.Index = e.Row.RowIndex;
                                tw.ScreenName = lblScreenName.Text;
                                tw.ProfileImage = lblProfileImage.Text;

                                HtmlGenericControl ianchor = new HtmlGenericControl(Constants.A);
                                ianchor.Attributes.Add(Constants.TARGET, Constants.BLANK);
                                ianchor.Attributes.Add(Constants.TITLE, tw.ScreenName);
                                ianchor.Attributes.Add(Constants.HREF, string.Concat(Constants.HTTPTWITTER, tw.ScreenName));

                                HtmlImage img = new HtmlImage();
                                img.Src = tw.ProfileImage;
                                ianchor.Controls.Add(img);
                                img.Attributes.Add(Constants.CLASS, Constants.GRIDIMAGE);

                                li.Controls.Add(ianchor);
                            }
                        }
                    }
                    break;
                case DataControlRowType.Footer:

                    //e.Row.Cells.Remove(e.Row.Cells[4]);
                    Control ctrllblTotal = e.Row.FindControl("lblTotal");
                    if (ctrllblTotal != null) {
                        if (ctrllblTotal.Parent is DataControlFieldCell) {
                            DataControlFieldCell cell = (DataControlFieldCell)ctrllblTotal.Parent;
                            //cell.ColumnSpan = 2;
                        }
                        Label lblTotal = (Label)ctrllblTotal;

                        int startamount = gvUsers.PageIndex * gvUsers.PageSize;
                        int endamount = startamount + gvUsers.PageSize;
                        if (ListUsers.Count < endamount)
                            endamount = ListUsers.Count;

                        lblTotal.Text = string.Concat(startamount.ToString(), " to ", endamount.ToString(), " of ", Resources.Resource.Total, Constants.COLON, Constants.SPACE, ListUsers.Count.ToString());
                    }
                    switch (Master.Friendorfollower) {
                        case Constants.FriendsFollowers.NotFollowing:
                            Control ctrlbtnFollow = e.Row.FindControl("btnFollow");
                            if (ctrlbtnFollow != null) {
                                ctrlbtnFollow.Visible = true;
                            }
                            Control ctrlbtnFollowAll = e.Row.FindControl("btnFollowAll");
                            if (ctrlbtnFollowAll != null) {
                                ctrlbtnFollowAll.Visible = true;
                            }
                            break;
                    }

                    switch (Master.Friendorfollower) {
                        case Constants.FriendsFollowers.Exclude:
                        case Constants.FriendsFollowers.Unfollow:
                        case Constants.FriendsFollowers.NotFollowing:
                        case Constants.FriendsFollowers.NeverFollow:
                            Control ctrlbtnSave = e.Row.FindControl("btnSave");
                            if (ctrlbtnSave != null) ctrlbtnSave.Visible = true;

                            Control ctrlbtnAddExclude = e.Row.FindControl("btnAddExclude");
                            if (ctrlbtnAddExclude != null) ctrlbtnAddExclude.Visible = false;

                            if (Master.Friendorfollower != Constants.FriendsFollowers.Unfollow) {
                                Control ctrlbtnDestroy = e.Row.FindControl("btnDestroy");
                                if (ctrlbtnDestroy != null) ctrlbtnDestroy.Visible = false;

                                Control ctrlbtnDestroyAl = e.Row.FindControl("btnDestroyAll");
                                if (ctrlbtnDestroyAl != null) ctrlbtnDestroyAl.Visible = false;
                            }
                            if (Master.Friendorfollower == Constants.FriendsFollowers.NotFollowing) {
                                Control ctrlbtnAddNeverFollow = e.Row.FindControl("btnAddNeverFollow");
                                if (ctrlbtnAddNeverFollow != null) ctrlbtnAddNeverFollow.Visible = true;
                            }

                            if (Master.Friendorfollower != Constants.FriendsFollowers.Exclude &
                                Master.Friendorfollower != Constants.FriendsFollowers.NeverFollow) {
                                Control ctrlbtnDel = e.Row.FindControl("btnDelete");
                                if (ctrlbtnDel != null) ctrlbtnDel.Visible = false;
                            }

                            break;
                        case Constants.FriendsFollowers.Analyze:
                            Control ctlbtnDestroy = e.Row.FindControl("btnDestroy");
                            if (ctlbtnDestroy != null) ctlbtnDestroy.Visible = false;

                            Control ctrlbtnDestroyAll = e.Row.FindControl("btnDestroyAll");
                            if (ctrlbtnDestroyAll != null) ctrlbtnDestroyAll.Visible = false;
                            break;
                    }

                    break;
            }

        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (ListUsers.Count > 0) {
                bool usersChanged = false;
                bool friendsChanged = false;
                bool followersChanged = false;

                foreach (GridViewRow row in gvUsers.Rows) {
                    Control ctrl = row.FindControl("cbxDelete");
                    if (ctrl != null) {
                        CheckBox cbx = (CheckBox)ctrl;
                        if (cbx.Checked) {
                            Control lblctrl = row.FindControl("lblUserId");
                            if (lblctrl != null) {
                                Label lbl = (Label)lblctrl;
                                var user = ListUsers.Where(k => k.UserId == lbl.Text).FirstOrDefault();
                                if (user != null) {
                                    var uremove = ListFriends.Where(x => x.UserId == user.UserId).FirstOrDefault();
                                    if (uremove != null) {
                                        ListFriends.Remove(uremove);
                                        friendsChanged = true;
                                    }
                                    var furemove = ListFollowers.Where(x => x.UserId == user.UserId).FirstOrDefault();
                                    if (furemove != null) {
                                        ListFollowers.Remove(furemove);
                                        followersChanged = true;
                                    }
                                    ListUsers.Remove(user);
                                    usersChanged = true;
                                }
                            }
                        }
                    }
                }
                if (friendsChanged) Serializer.WriteFollowersXML(ListFriends.ToList(), Constants.FriendsFollowers.Friends.ToString());
                if (followersChanged) Serializer.WriteFollowersXML(ListFollowers.ToList(), Constants.FriendsFollowers.Followers.ToString());
                if (usersChanged) {
                    if (friendsChanged) Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Analyze.ToString());
                    if (friendsChanged) Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Unfollow.ToString());
                    if (followersChanged) Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NotFollowing.ToString());
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
                }
                BindData();
            }
        }

        protected void btnOpen_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (GridViewRow row in gvUsers.Rows)
            {
                Control ctrl = row.FindControl("cbxDelete");
                if (ctrl != null) {
                    CheckBox cbx = (CheckBox)ctrl;
                    if (cbx.Checked) {
                        Control lblScreenName = row.FindControl("lblScreenName");
                        if (lblScreenName != null) {
                            Label lbl = (Label)lblScreenName;
                            sb.AppendLine(String.Format("window.open('{0}','_blank')", ResolveUrl(string.Concat(Constants.HTTPTWITTER, lbl.Text))));
                        }
                    }
                }
            }
            if (sb.Length > 0) {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "popup", sb.ToString(), true);
            }
            BindData();
        }

        protected void btnDestroy_Click(object sender, EventArgs e)
        {
            if (ListUsers.Count > 0) {
                bool usersChanged = false;
                bool followersChanged = false;

                TwitterAuthenticate();
                try {
                    foreach (GridViewRow row in gvUsers.Rows) {
                        Control ctrl = row.FindControl("cbxDelete");
                        if (ctrl != null) {
                            CheckBox cbx = (CheckBox)ctrl;
                            if (cbx.Checked) {
                                Control lblctrl = row.FindControl("lblUserId");
                                if (lblctrl != null) {
                                    Label lbl = (Label)lblctrl;
                                    var user = ListUsers.Where(k => k.UserId == lbl.Text).FirstOrDefault();
                                    if (user != null) {
                                        twdata = new Twitter(user, Twitter.TwitterType.Unfollow);
                                        Twitter.User utoremove = ListFriends.Find(x => x.UserId == user.UserId);
                                        if (utoremove != null && ListFriends.Contains(utoremove)) {
                                            ListFriends.Remove(utoremove);
                                            followersChanged = true;
                                        }
                                        ListUsers.Remove(user);
                                        usersChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (followersChanged) Serializer.WriteFollowersXML(ListFriends.ToList(), Constants.FriendsFollowers.Friends.ToString());
                    if (usersChanged) {
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Analyze.ToString());
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Unfollow.ToString());
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
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

        protected void btnDestroyAll_Click(object sender, EventArgs e)
        {
            if (ListUsers.Count > 0) {
                bool followersChanged = false;

                TwitterAuthenticate();
                try {
                    foreach (Twitter.User user in ListUsers)
                    {
                        twdata = new Twitter(user, Twitter.TwitterType.Unfollow);
                        Twitter.User utoremove = ListFriends.Find(x => x.UserId == user.UserId);
                        if (utoremove != null && ListFriends.Contains(utoremove)) {
                            ListFriends.Remove(utoremove);
                            followersChanged = true;
                        }
                    }
                    ListUsers.Clear();
                    if (followersChanged) Serializer.WriteFollowersXML(ListFriends.ToList(), Constants.FriendsFollowers.Friends.ToString());

                    Serializer.DeleteListXML(Constants.FriendsFollowers.Analyze.ToString());
                    Serializer.DeleteListXML(Constants.FriendsFollowers.Unfollow.ToString());
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
                    BindData();
                } catch (WebException wex) {
                    BindData();
                    ScriptManager.RegisterStartupScript(this, GetType(), "displayalertmessage", string.Format("alert('{0}');", "No internet connection: " + wex.Message.Replace("'", string.Empty)), true);
                } catch (Exception ex) {
                    //do nothing
                }
            }
        }

        protected void btnFollow_Click(object sender, EventArgs e)
        {
            if (ListUsers.Count > 0) {
                bool usersChanged = false;
                bool friendsChanged = false;

                TwitterAuthenticate();
                try {
                    foreach (GridViewRow row in gvUsers.Rows) {
                        Control ctrl = row.FindControl("cbxDelete");
                        if (ctrl != null) {
                            CheckBox cbx = (CheckBox)ctrl;
                            if (cbx.Checked) {
                                Control lblctrl = row.FindControl("lblUserId");
                                if (lblctrl != null) {
                                    Label lbl = (Label)lblctrl;
                                    var user = ListUsers.Where(k => k.UserId == lbl.Text).FirstOrDefault();
                                    if (user != null) {
                                        twdata = new Twitter(user, Twitter.TwitterType.Friends);
                                        if (!ListFriends.Contains(user)) {
                                            ListFriends.Add(user);
                                            friendsChanged = true;
                                        }
                                        ListUsers.Remove(user);
                                        usersChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (friendsChanged) Serializer.WriteFollowersXML(ListFriends.ToList(), Constants.FriendsFollowers.Friends.ToString());
                    if (usersChanged) {
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NotFollowing.ToString());
                        Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
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

        protected void btnFollowAll_Click(object sender, EventArgs e)
        {
            if (ListUsers.Count > 0) {
                bool friendsChanged = false;

                TwitterAuthenticate();
                try {
                    foreach (Twitter.User user in ListUsers) {
                        twdata = new Twitter(user, Twitter.TwitterType.Friends);
                        if (!ListFriends.Contains(user)) {
                            ListFriends.Add(user);
                            friendsChanged = true;
                        }
                        ListUsers.Remove(user);
                    }
                    ListUsers.Clear();
                    if (friendsChanged) Serializer.WriteFollowersXML(ListFriends.ToList(), Constants.FriendsFollowers.Friends.ToString());

                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NotFollowing.ToString());
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
                    BindData();
                } catch (WebException wex) {
                    BindData();
                    ScriptManager.RegisterStartupScript(this, GetType(), "displayalertmessage", string.Format("alert('{0}');", "No internet connection: " + wex.Message.Replace("'", string.Empty)), true);
                } catch (Exception ex) {
                    //do nothing
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            switch (Master.Friendorfollower) {
                case Constants.FriendsFollowers.Exclude:
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Exclude.ToString());
                    break;
                case Constants.FriendsFollowers.Unfollow:
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Unfollow.ToString());
                    break;
                case Constants.FriendsFollowers.NeverFollow:
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NeverFollow.ToString());
                    break;
                case Constants.FriendsFollowers.NotFollowing:
                    Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.NotFollowing.ToString());
                    break;
            }
            //Serializer.WriteDestroyListExcel(ListUsers, Master.friendorfollower.ToString());
            BindData();

            ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Done!');", true);
        }

        protected void btnAddExclude_Click(object sender, EventArgs e)
        {
            List<Twitter.User> addToExclude = new List<Twitter.User>();
            foreach (GridViewRow row in gvUsers.Rows) {
                Control ctrl = row.FindControl("cbxDelete");
                if (ctrl != null) {
                    CheckBox cbx = (CheckBox)ctrl;
                    if (cbx.Checked) {
                        Control lblctrl = row.FindControl("lblUserId");
                        if (lblctrl != null) {
                            Label lbl = (Label)lblctrl;
                            var user = ListUsers.Where(k => k.UserId == lbl.Text).FirstOrDefault();
                            addToExclude.Add(user);
                            ListUsers.Remove(user);
                        }
                    }
                }
            }
            CreateDestroyList.AddToExcludeList(addToExclude);
            Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
            BindData();
        }

        protected void btnAddNeverFollow_Click(object sender, EventArgs e)
        {
            List<Twitter.User> addToNeverFollowList = new List<Twitter.User>();
            foreach (GridViewRow row in gvUsers.Rows) {
                Control ctrl = row.FindControl("cbxDelete");
                if (ctrl != null) {
                    CheckBox cbx = (CheckBox)ctrl;
                    if (cbx.Checked) {
                        Control lblctrl = row.FindControl("lblUserId");
                        if (lblctrl != null) {
                            Label lbl = (Label)lblctrl;
                            var user = ListUsers.Where(k => k.UserId == lbl.Text).FirstOrDefault();
                            addToNeverFollowList.Add(user);
                            ListUsers.Remove(user);
                        }
                    }
                }
            }
            CreateDestroyList.AddToNeverFollowList(addToNeverFollowList);
            Serializer.WriteFollowersXML(ListUsers.ToList(), Constants.FriendsFollowers.Users.ToString());
            BindData();
        }
    }
}