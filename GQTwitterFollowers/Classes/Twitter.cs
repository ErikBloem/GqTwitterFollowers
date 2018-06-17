using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace GQTwitterFollowers
{
    public class Twitter
    {
        public List<User> listFollowers = new List<User>();
        public BindingList<Timeline> listTimeline = new BindingList<Timeline>();
        public TwitAuthenticateResponse twitAuthResponse = null;
        public string authHeader = string.Empty;

        public TwitterType tType = TwitterType.Friends;
        public enum TwitterType {
            Friends,
            Unfollow,
            TimeLine,
            ByTweetId
        }

        private enum DestroyCreate
        {
            destroy,
            create
        }

        public MyDestroyCredentials Credentials { get; set; }
        public MyConsumerCredentials ConsumerCredentials { get; set; }

        public Twitter(TwitterType tp)
        {
            ConsumerCredentials = new MyConsumerCredentials();

            tType = tp;

            // Do the Authenticate
            var oAuthUrl = Constants.OAUTHURL;
            // You need to set your own keys and screen name
            switch (tType) {
                case TwitterType.Unfollow:
                    authHeader = string.Concat(Constants.BASIC, Constants.SPACE,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(ConsumerCredentials.oauth_consumer_key) + Constants.COLON +
                            Uri.EscapeDataString((ConsumerCredentials.oauth_consumer_secret)))
                    ));
                    break;
                case TwitterType.Friends:
                    authHeader = string.Concat(Constants.BASIC, Constants.SPACE,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(ConsumerCredentials.oauth_consumer_key) + Constants.COLON +
                            Uri.EscapeDataString((ConsumerCredentials.oauth_consumer_secret)))
                    ));
                    break;
                case TwitterType.TimeLine:
                    authHeader = string.Concat(Constants.BASIC, Constants.SPACE,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(ConsumerCredentials.oauth_consumer_key) + Constants.COLON +
                            Uri.EscapeDataString((ConsumerCredentials.oauth_consumer_secret)))
                    ));
                    break;
                case TwitterType.ByTweetId:
                    authHeader = string.Concat(Constants.BASIC, Constants.SPACE,
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(ConsumerCredentials.oauth_consumer_key) + Constants.COLON +
                            Uri.EscapeDataString((ConsumerCredentials.oauth_consumer_secret)))
                    ));
                    break;
            }
            var postBody = Constants.POSTBODY;

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            authRequest.Headers.Add(Constants.AUTHORIZATION, authHeader);
            authRequest.Method = Constants.POST;
            authRequest.ContentType = Constants.CONTENTTYPE;
            authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (Stream stream = authRequest.GetRequestStream()) {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }
            authRequest.Headers.Add(Constants.ACCEPTENCODING, Constants.GZIP);
            WebResponse authResponse = authRequest.GetResponse();
            // deserialize into an object
            using (authResponse) {
                using (var reader = new StreamReader(authResponse.GetResponseStream())) {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objectText = reader.ReadToEnd();
                    twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
                }
            }
        }

        public Twitter(User user, TwitterType tp)
        {
            Credentials = new MyDestroyCredentials();

            // oauth implementation details
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            string destroycreate = "https://api.twitter.com/1.1/friendships/{0}.json";

            // message api details
            string resource_url = string.Empty;
            switch (tp) {
                case TwitterType.Friends:
                    resource_url = string.Format(destroycreate, DestroyCreate.create.ToString());
                    break;
                case TwitterType.Unfollow:
                    resource_url = string.Format(destroycreate, DestroyCreate.destroy.ToString());
                    break;
                case TwitterType.ByTweetId:
                    resource_url = string.Format(destroycreate, DestroyCreate.create.ToString());
                    destroycreate = "https://api.twitter.com/1.1/statuses/show{0}.json";
                    break;
            }

            // create oauth signature
            var baseFormat = "oauth_consumer_key={0}&" +
                "oauth_nonce={1}&" +
                "oauth_signature_method={2}&" +
                "oauth_timestamp={3}&" +
                "oauth_token={4}&" +
                "oauth_version={5}&" +
                "user_id={6}";

            var baseString = string.Format(baseFormat,
                    Credentials.oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp,
                    Credentials.oauth_token,
                    oauth_version,
                    Uri.EscapeDataString(user.UserId)
            );

            baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(Credentials.oauth_consumer_secret),
            "&", Uri.EscapeDataString(Credentials.oauth_token_secret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var headerFormat = "OAuth oauth_consumer_key=\"{0}\", " +
                "oauth_nonce=\"{1}\", " +
                "oauth_signature=\"{2}\", " +
                "oauth_signature_method=\"{3}\", " +
                "oauth_timestamp=\"{4}\", " +
                "oauth_token=\"{5}\", " +
                "oauth_version=\"{6}\"";

            authHeader = string.Format(headerFormat,
                Uri.EscapeDataString(Credentials.oauth_consumer_key),
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(Credentials.oauth_token),
                Uri.EscapeDataString(oauth_version)
            );

            // make the request
            ServicePointManager.Expect100Continue = false;

            // message api details
            var postBody = string.Concat("?user_id=", Uri.EscapeDataString(user.UserId));
            resource_url = string.Concat(resource_url, postBody);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            WebResponse response = request.GetResponse();
            string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
        }

        public Twitter(Timeline timeline, TwitterType tp)
        {
            Credentials = new MyDestroyCredentials();

            // oauth implementation details
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            string destroycreate = "https://api.twitter.com/1.1/statuses/destroy/{0}.json";

            // message api details
            string resource_url = string.Empty;
            switch (tp) {
                case TwitterType.TimeLine:
                    resource_url = string.Format(destroycreate, timeline.ID);
                    break;
            }

            // create oauth signature
            var baseFormat = "oauth_consumer_key={0}&" +
                "oauth_nonce={1}&" +
                "oauth_signature_method={2}&" +
                "oauth_timestamp={3}&" +
                "oauth_token={4}&" +
                "oauth_version={5}";

            var baseString = string.Format(baseFormat,
                    Credentials.oauth_consumer_key,
                    oauth_nonce,
                    oauth_signature_method,
                    oauth_timestamp,
                    Credentials.oauth_token,
                    oauth_version
            );

            baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(Credentials.oauth_consumer_secret),
            "&", Uri.EscapeDataString(Credentials.oauth_token_secret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(
                hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var headerFormat = "OAuth oauth_consumer_key=\"{0}\", " +
                "oauth_nonce=\"{1}\", " +
                "oauth_signature=\"{2}\", " +
                "oauth_signature_method=\"{3}\", " +
                "oauth_timestamp=\"{4}\", " +
                "oauth_token=\"{5}\", " +
                "oauth_version=\"{6}\"";

            authHeader = string.Format(headerFormat,
                Uri.EscapeDataString(Credentials.oauth_consumer_key),
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(Credentials.oauth_token),
                Uri.EscapeDataString(oauth_version)
            );

            // make the request
            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            WebResponse response = request.GetResponse();
            string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
        }

        public void CallTweetIdData(string resource_tweetIdFormat, bool createfromExcelFile)
        {
            string error = string.Empty;
            try {
                BindingList<TweetId> tweetids = null;
                if (!createfromExcelFile) {
                    tweetids = Serializer.ReadTweetIdsExcelXML(Constants.FriendsFollowers.TweetIdExcel.ToString());
                }
                if (createfromExcelFile | (tweetids == null || tweetids.Count == 0)) {
                    tweetids = Serializer.TweetIdsListExcel(Constants.FriendsFollowers.TweetId.ToString());
                    Serializer.WriteTweetIdsExcelXML(tweetids.ToList(), Constants.FriendsFollowers.TweetIdExcel.ToString());
                }
                listTimeline = new BindingList<Timeline>();
                error = CallTweetIdData(resource_tweetIdFormat, tweetids);
                if (!string.IsNullOrEmpty(error)) {
                    throw new Exception(error);
                }
            } catch (Exception ex) {
                throw;
            }
        }

        private string CallTweetIdData(string resource_tweetIdFormat, BindingList<TweetId> tweetids)
        {
            string error = string.Empty;
            StringBuilder s = new StringBuilder();
            int amount = 0;

            s.Append("[");
            foreach (TweetId tid in tweetids) {
                try {
                    if (!string.IsNullOrEmpty(tid.Tweet_Id)) {
                        string tweetresult = GetJSonObject(resource_tweetIdFormat, tid.Tweet_Id);
                        s.Append(string.Format("{0},", tweetresult));

                        amount++;
                        Thread.Sleep(100);

                        double d = 500;
                        d = d / amount;
                        if (amount > 499 & (d % 1) == 0) {
                            Thread.Sleep(5000);
                        }
                    }
                } catch (Exception ex) {
                    //test if it was not found
                    //The remote server returned an error: (404) Not Found.
                    if (ex.Message.Contains("(429) Too Many Requests")) {
                        error = ex.Message;
                        break;
                    }
                }
            }
            s.Append("]");
            if (s.Length > 10 & s.ToString() != "[") {
                string result = s.ToString().Replace(",]", "]");
                createTimeLine(result);
            }
            if (amount > 0 & !string.IsNullOrEmpty(error)) {
                Serializer.WriteTimelineXML(listTimeline.ToList(), Constants.FriendsFollowers.TweetId.ToString());
            }
            return error;
        }

        public void FirstCallData(string resource_urlFormat, string friendsorfollowers, int count, string cursor, string maxid = null)
        {
            try {
                CallData(resource_urlFormat, friendsorfollowers, count, cursor, null, maxid);
            } catch (Exception ex) {
                throw;
            }
        }

        private void CallData(string resource_urlFormat, string friendsorfollowers, int count, string cursor, User user, string maxid)
        {
            string result = GetJSonObject(resource_urlFormat, friendsorfollowers, count, cursor, user, maxid);
            switch (tType) {
                case TwitterType.TimeLine:
                    createTimeLine(result);
                    break;
                default:
                    JObject j = JObject.Parse(result);
                    JArray data = (JArray)j[Constants.USERS];
                    if (data != null) {
                        int index = 1;
                        foreach (var item in data)
                        {
                            User objTwiterFollowers = new User();
                            objTwiterFollowers.Index = index;
                            objTwiterFollowers.ScreenName = item[Constants.SCREEN_NAME].ToString().Replace(Constants.BACKSLASH, string.Empty);
                            objTwiterFollowers.ProfileImage = item[Constants.PROFILE_IMAGE_URL].ToString().Replace(Constants.BACKSLASH, string.Empty);
                            objTwiterFollowers.UserId = item[Constants.ID].ToString().Replace(Constants.BACKSLASH, string.Empty);
                            listFollowers.Add(objTwiterFollowers);
                            index++;
                        }
                        if (j != null) {
                            JValue next_cursor = (JValue)j[Constants.NEXT_CURSOR];
                            if (long.Parse(next_cursor.Value.ToString()) > 0) {
                                try {
                                    CallData(resource_urlFormat, friendsorfollowers, count, next_cursor.Value.ToString(), user, maxid);
                                } catch (Exception) {
                                    if (next_cursor != null) {
                                        next_cursor.Value = 0;
                                    }
                                    throw;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void createTimeLine(string result)
        {
            JArray data = JArray.Parse(result);
            if (data != null) {
                int index = 1;
                foreach (var item in data) {
                    Timeline objTwiterTimeline = new Timeline();
                    objTwiterTimeline.Index = index;
                    foreach (var i in item) {
                        if (i.ToString().Contains(":")) {
                            List<string> arr = new List<string>();
                            arr.Add(i.ToString().Substring(0, i.ToString().IndexOf(":")).Trim().Replace("\"", string.Empty));
                            arr.Add(i.ToString().Substring(i.ToString().IndexOf(":") + 1).Trim());
                            if (arr[0] == Constants.CREATEDAT) {
                                objTwiterTimeline.Createdat = GetCorrectedDate(arr[1].Replace(Constants.BACKSLASH, string.Empty).Split(' ').ToList());
                            }
                            if (arr[0] == Constants.ID) {
                                objTwiterTimeline.ID = Convert.ToInt64(arr[1]);
                            }
                            if (arr[0] == Constants.TEXT) {
                                objTwiterTimeline.Text = arr[1].Replace("\"", string.Empty);
                            }
                            if (arr[0].Contains(Constants.INREPLY)) {
                                objTwiterTimeline.SetReply = arr[1].Replace("\"", string.Empty);
                            }
                            if (arr[0] == Constants.RETWEETEDSTATUS) {
                                string arr1 = arr[1].Substring(1, arr[1].Length - 1).Trim();
                                List<string> retweetstatus = arr1.Split(',').ToList();
                                if (retweetstatus != null) {
                                    int ind = 0;
                                    User reUser = null;
                                    foreach (var itm in retweetstatus) {
                                        if (itm.Length > 0) {
                                            List<string> arrstat = new List<string>();
                                            if (itm.Contains(":")) {
                                                arrstat.Add(itm.Substring(0, itm.IndexOf(":")).Trim().Replace("\"", string.Empty));
                                                arrstat.Add(itm.Substring(itm.IndexOf(":") + 1).Trim());

                                                if (arrstat[0] == Constants.CREATEDAT) {
                                                    if (reUser == null) reUser = new User();
                                                    reUser.Createdat = GetCorrectedDate(arrstat[1].Replace(Constants.BACKSLASH, string.Empty).Split(' ').ToList());
                                                }
                                                if (arrstat[0].ToLower() == "user") {

                                                    string it = itm;
                                                    for (int k = ind; k < retweetstatus.Count; k++) {
                                                        if (retweetstatus[k].Contains(Constants.ID)) {
                                                            //it += retweetstatus[k];
                                                        }
                                                        if (retweetstatus[k].Contains(Constants.NAME)) {
                                                            it += retweetstatus[k];
                                                        }
                                                        if (retweetstatus[k].Contains(Constants.SCREEN_NAME)) {
                                                            it += retweetstatus[k];
                                                        }
                                                        if (retweetstatus[k].Contains(Constants.PROFILE_IMAGE_URL)) {
                                                            it += retweetstatus[k];
                                                            break;
                                                        }
                                                    }
                                                    it = it.Substring(1, it.Length - 1).Trim().Replace("{\r\n ", string.Empty).Replace("\"user\":    ", string.Empty).Replace("    ", string.Empty).Replace("\r\n", ",\r\n");
                                                    List<string> retweetuser = it.Split(',').ToList();
                                                    if (retweetuser != null) {
                                                        if (reUser == null) reUser = new User();
                                                        foreach (var itemuser in retweetuser) {
                                                            List<string> arruser = new List<string>();
                                                            arruser.Add(itemuser.Substring(0, itemuser.IndexOf(":")).Trim().Replace("\"", string.Empty));
                                                            arruser.Add(itemuser.Substring(itemuser.IndexOf(":") + 1).Trim().Replace("\"", string.Empty));
                                                            if (arruser[0] == Constants.ID) {
                                                                reUser.UserId = arruser[1];
                                                            }
                                                            if (arruser[0] == Constants.NAME) {
                                                                reUser.Name = arruser[1];
                                                            }
                                                            if (arruser[0] == Constants.SCREEN_NAME) {
                                                                reUser.ScreenName = arruser[1];
                                                            }
                                                            if (arruser[0] == Constants.PROFILE_IMAGE_URL) {
                                                                reUser.ProfileImage = arruser[1];
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                        ind++;
                                    }
                                    if (reUser != null) objTwiterTimeline.User = reUser;
                                }
                            }
                            if (arr[0] == Constants.RETWEETED) {
                                objTwiterTimeline.Retweeted = Convert.ToBoolean(arr[1]);
                            }
                        }
                        Debug.Print(i.ToString());
                    }
                    listTimeline.Add(objTwiterTimeline);
                    //objTwiterTimeline.ScreenName = item[Constants.SCREEN_NAME].ToString().Replace(Constants.BACKSLASH, string.Empty);
                    index++;
                }
            }

        }

        private string GetJSonObject(string resource_tweetIdFormat, string tweetId)
        {
            string resource_url = string.Empty;
            switch (tType) {
                case TwitterType.ByTweetId:
                    resource_url = string.Format(resource_tweetIdFormat, tweetId);
                    break;
            }
            return GetWebresponse(resource_url);
        }

        private string GetJSonObject(string resource_urlFormat, string friendsorfollowers, int count, string cursor, User user, string maxid)
        {
            string resource_url = string.Empty;
            switch (tType)
            {
                case TwitterType.Unfollow:
                    resource_url = string.Format(resource_urlFormat, user.UserId);
                    break;
                case TwitterType.Friends:
                    resource_url = string.Format(resource_urlFormat, friendsorfollowers, ConsumerCredentials.screen_name, count, cursor);
                    if (string.IsNullOrEmpty(cursor) & resource_url.IndexOf(string.Concat(Constants.AND, Constants.CURSOR)) > -1)
                    {
                        resource_url = resource_url.Substring(0, resource_url.IndexOf(string.Concat(Constants.AND, Constants.CURSOR)));
                    }
                    break;
                case TwitterType.TimeLine:
                    resource_url = string.Format(resource_urlFormat, ConsumerCredentials.screen_name, count);
                    if (!string.IsNullOrEmpty(maxid)) {
                        resource_url += string.Format("&max_id={0}", maxid);
                    }
                    break;
            }
            return GetWebresponse(resource_url);
        }

        private string GetWebresponse(string resource_url)
        {
            HttpWebRequest fRequest = (HttpWebRequest)WebRequest.Create(resource_url);
            fRequest.Headers.Add(Constants.AUTHORIZATION, string.Concat(twitAuthResponse.token_type, Constants.SPACE, twitAuthResponse.access_token));
            switch (tType) {
                case TwitterType.Unfollow:
                    fRequest.Method = Constants.POST;
                    break;
                case TwitterType.Friends:
                case TwitterType.TimeLine:
                    fRequest.Method = Constants.GET;
                    break;
            }
            WebResponse response = fRequest.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private string GetCorrectedDate(List<string> datearr)
        {
            string date = string.Empty;
            foreach (var d in datearr) {
                if (!d.Contains(":") && !d.Contains("+")) {
                    date += d + " ";
                }
            }
            foreach (var d in datearr) {
                if (d.Contains(":") || d.Contains("+")) {
                    date += d + " ";
                }
            }
            return date.Trim();
        }

        public class TwitAuthenticateResponse
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
        }

        public class MyConsumerCredentials
        {
            public string oauth_consumer_key = string.Empty;
            public string oauth_consumer_secret = string.Empty;
            public string screen_name = string.Empty;

            public MyConsumerCredentials()
            {
                GenerateCredentials();
            }

            public ITwitterCredentials GenerateCredentials()
            {
                TwitterHelper.TestConnectCredentials();

                oauth_consumer_key = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_key)];
                oauth_consumer_secret = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_secret)];
                screen_name = ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name)];
                return new TwitterCredentials(oauth_consumer_key, oauth_consumer_secret, screen_name);
            }
        }

        public class MyDestroyCredentials
        {
            public string oauth_consumer_key = string.Empty;
            public string oauth_consumer_secret = string.Empty;
            public string oauth_token = string.Empty;
            public string oauth_token_secret = string.Empty;
            public string screen_name = string.Empty;

            public MyDestroyCredentials()
            {
                GenerateCredentials();
            }

            public ITwitterCredentials GenerateCredentials()
            {
                TwitterHelper.TestDestroyConnectCredentials();

                oauth_token = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_token)];
                oauth_token_secret = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_token_secret)];
                oauth_consumer_key = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_key)];
                oauth_consumer_secret = ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_secret)];
                screen_name = ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name)];
                return new TwitterCredentials(oauth_consumer_key, oauth_consumer_secret, oauth_token, oauth_token_secret, screen_name);
            }
        }

        internal class TwitterHelper
        {
            private static string oauth_consumer_key = "";
            private static string oauth_consumer_secret = "";
            private static string oauth_token = "";
            private static string oauth_token_secret = "";
            private static string screen_name = "";

            internal static void TestConnectCredentials()
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_key)]))
                    throw new Exception(string.Format("No {0} found in the web config", Constants.GetVariableName(() => oauth_consumer_key)));
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_consumer_secret)]))
                    throw new Exception(string.Format("No {0} found in the web config", Constants.GetVariableName(() => oauth_consumer_secret)));
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => screen_name)]))
                    throw new Exception(string.Format("No {0} found in the web config", Constants.GetVariableName(() => screen_name)));
            }

            internal static void TestDestroyConnectCredentials()
            {
                TestConnectCredentials();
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_token)]))
                    throw new Exception(string.Format("No {0} found in the web config", Constants.GetVariableName(() => oauth_token)));
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[Constants.GetVariableName(() => oauth_token_secret)]))
                    throw new Exception(string.Format("No {0} found in the web config", Constants.GetVariableName(() => oauth_token_secret)));
            }
        }

        [Serializable()]
        public class TweetId : ICloneable
        {
            private int index = 0;
            private DateTime createdat = DateTime.MinValue;
            private string tweet_id = string.Empty;
            private string in_reply_to_status_id = string.Empty;
            private string in_reply_to_user_id = string.Empty;
            private DateTime timestamp = DateTime.MinValue;
            private string source = string.Empty;
            private string text = string.Empty;
            private string retweeted_status_id = string.Empty;
            private string retweeted_status_user_id = string.Empty;
            private DateTime retweeted_status_timestamp = DateTime.MinValue;
            private string expanded_urls = string.Empty;

            public int Index
            {
                get {
                    return index;
                }
                set {
                    index = value;
                }
            }

            public string Createdat
            {
                get {
                    return createdat.ToString();
                }
                set {
                    if (!string.IsNullOrEmpty(value)) {
                        DateTime date = DateTime.Now;
                        DateTime.TryParse(value, out date);
                        createdat = date;
                    } else {
                        createdat = DateTime.MinValue;
                    }
                }
            }

            public string Tweet_Id
            {
                get {
                    return tweet_id;
                }
                set {
                    tweet_id = value;
                }
            }

            public string In_reply_to_status_id
            {
                get {
                    return in_reply_to_status_id;
                }

                set {
                    in_reply_to_status_id = value;
                }
            }

            public string In_reply_to_user_id
            {
                get {
                    return in_reply_to_user_id;
                }

                set {
                    in_reply_to_user_id = value;
                }
            }

            public string Timestamp
            {
                get {
                    return timestamp.ToString();
                }

                set {
                    if (!string.IsNullOrEmpty(value)) {
                        DateTime date = DateTime.Now;
                        DateTime.TryParse(value, out date);
                        timestamp = date;
                    } else {
                        timestamp = DateTime.MinValue;
                    }
                }
            }

            public string Source
            {
                get {
                    return source;
                }

                set {
                    source = value;
                }
            }

            public string Text
            {
                get {
                    return text;
                }

                set {
                    text = value;
                }
            }

            public string Retweeted_status_id
            {
                get {
                    return retweeted_status_id;
                }

                set {
                    retweeted_status_id = value;
                }
            }

            public string Retweeted_status_user_id
            {
                get {
                    return retweeted_status_user_id;
                }

                set {
                    retweeted_status_user_id = value;
                }
            }

            public string Retweeted_status_timestamp
            {
                get {
                    return retweeted_status_timestamp.ToString();
                }

                set {
                    if (!string.IsNullOrEmpty(value)) {
                        DateTime date = DateTime.Now;
                        DateTime.TryParse(value, out date);
                        retweeted_status_timestamp = date;
                    } else {
                        retweeted_status_timestamp = DateTime.MinValue;
                    }
                }
            }

            public string Expanded_urls
            {
                get {
                    return expanded_urls;
                }

                set {
                    expanded_urls = value;
                }
            }

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        [Serializable()]
        public class User : ICloneable
        {
            private int index = 0;
            private DateTime createdat;
            private string name = string.Empty;
            private string screenName = string.Empty;
            private string profileImage = string.Empty;
            private string userId = string.Empty;

            public int Index
            {
                get {
                    return index;
                }
                set {
                    index = value;
                }
            }

            public string Createdat
            {
                get {
                    return createdat.ToString();
                }
                set {
                    DateTime date = DateTime.Now;
                    DateTime.TryParse(value, out date);
                    createdat = date;
                }
            }

            public string Name
            {
                get {
                    return name;
                }
                set {
                    name = value;
                }
            }

            public string ScreenName {
                get
                {
                    return screenName;
                }
                set
                {
                    screenName = value;
                }
            }

            public string ProfileImage
            {
                get
                {
                    return profileImage;
                }
                set
                {
                    profileImage = value;
                }
            }

            public string UserId
            {
                get
                {
                    return userId;
                }
                set
                {
                    userId = value;
                }
            }

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        [Serializable()]
        public class Timeline : ICloneable
        {
            private int index = 0;
            private DateTime createdat;
            private long id = -1;
            private string userId = string.Empty;
            private string text = string.Empty;
            private string screenName = string.Empty;
            private bool isreply = false;
            private bool retweeted = false;

            private User user = null;

            public int Index
            {
                get {
                    return index;
                }
                set {
                    index = value;
                }
            }

            public string Createdat
            {
                get {
                    return createdat.ToString();
                }
                set {
                    DateTime date = DateTime.Now;
                    DateTime.TryParse(value, out date);
                    createdat = date;
                }
            }

            public string UserId
            {
                get {
                    return userId;
                }
                set {
                    userId = value;
                }
            }

            public long ID
            {
                get {
                    return id;
                }
                set {
                    id = value;
                }
            }

            public string Text
            {
                get {
                    return text;
                }
                set {
                    text = value;
                }
            }

            public User User
            {
                get {
                    return user;
                }
                set {
                    user = value;
                }
            }

            public bool Retweeted
            {
                get {
                    return retweeted;
                }
                set {
                    retweeted = value;
                }
            }

            public bool IsReply
            {
                get {
                    return isreply;
                }
                set {
                    isreply = value;
                }
            }

            public string SetReply
            {
                set {
                    if (!isreply) {
                        if (string.IsNullOrEmpty(value) || value.ToLower() == "null") {
                            isreply = false;
                        } else {
                            isreply = true;
                        }
                    }
                }
            }

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }
    }
}