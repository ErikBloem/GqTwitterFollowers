using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace GQTwitterFollowers
{
    public class CreateDestroyList
    {
        public static BindingList<Twitter.User> GetDestroyList(List<Twitter.User> ListFriends, List<Twitter.User> ListFollowers)
        {
            List<Twitter.User> ListFollowersAndInclude = new List<Twitter.User>(ListFollowers);
            List<Twitter.User> ListFriendsInclude = Serializer.ReadListXML(Constants.FriendsFollowers.Exclude.ToString());
            if (ListFriendsInclude != null) {
                foreach (Twitter.User user in ListFriendsInclude)
                {
                    if (!ListFollowersAndInclude.Exists(x => x.UserId == user.UserId))
                    {
                        ListFollowersAndInclude.Add(user);
                    }
                }
            }

            BindingList<Twitter.User> destroylist = new BindingList<Twitter.User>();
            foreach (Twitter.User friend in ListFriends)
            {
                if (!ListFollowersAndInclude.Exists(x => x.UserId == friend.UserId)) {
                    destroylist.Add(friend);
                }
            }
            Serializer.WriteFollowersXML(destroylist.ToList(), Constants.FriendsFollowers.Unfollow.ToString());
            return destroylist;
        }

        public static BindingList<Twitter.User> GetNotFollowingList(List<Twitter.User> ListFriends, List<Twitter.User> ListFollowers)
        {
            BindingList<Twitter.User> notfollowingList = new BindingList<Twitter.User>();
            foreach (Twitter.User friend in ListFollowers)
            {
                if (!ListFriends.Exists(x => x.UserId == friend.UserId))
                {
                    if (!notfollowingList.Contains(friend)) notfollowingList.Add(friend);
                }
            }
            return notfollowingList;
        }

        public static void AddToExcludeList(List<Twitter.User> addToExclude)
        {
            List<Twitter.User> ListFriendsInclude = Serializer.ReadListXML(Constants.FriendsFollowers.Exclude.ToString());
            if (ListFriendsInclude == null) ListFriendsInclude = new List<Twitter.User>();
            foreach (Twitter.User user in addToExclude)
            {
                if (!ListFriendsInclude.Exists(x => x.UserId == user.UserId))
                {
                    ListFriendsInclude.Add(user);
                }
            }
            Serializer.WriteFollowersXML(ListFriendsInclude.ToList(), Constants.FriendsFollowers.Exclude.ToString());
        }

        public static void AddToNeverFollowList(List<Twitter.User> addToNeverFollowList)
        {
            List<Twitter.User> ListFriendsExclude = Serializer.ReadListXML(Constants.FriendsFollowers.NeverFollow.ToString());
            if (ListFriendsExclude == null) ListFriendsExclude = new List<Twitter.User>();
            foreach (Twitter.User user in addToNeverFollowList) {
                if (!ListFriendsExclude.Exists(x => x.UserId == user.UserId)) {
                    ListFriendsExclude.Add(user);
                }
            }
            Serializer.WriteFollowersXML(ListFriendsExclude.ToList(), Constants.FriendsFollowers.NeverFollow.ToString());
        }
    }
}