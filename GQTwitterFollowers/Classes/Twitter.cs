using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace GQTwitterFollowers
{
    public class Twitter
    {
        public List<User> listFollowers = new List<User>();
        public TwitAuthenticateResponse twitAuthResponse = null;
        public string authHeader = string.Empty;

        public TwitterType tType = TwitterType.Friends;
        public enum TwitterType {
            Friends,
            Unfollow
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
            switch (tType)
            {
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
            }
            var postBody = Constants.POSTBODY;

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
            authRequest.Headers.Add(Constants.AUTHORIZATION, authHeader);
            authRequest.Method = Constants.POST;
            authRequest.ContentType = Constants.CONTENTTYPE;
            authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (Stream stream = authRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }
            authRequest.Headers.Add(Constants.ACCEPTENCODING, Constants.GZIP);
            WebResponse authResponse = authRequest.GetResponse();
            // deserialize into an object
            using (authResponse)
            {
                using (var reader = new StreamReader(authResponse.GetResponseStream()))
                {
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
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
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

        public void DestroyUser(string resource_urlFormat, User user)
        {
            try {
                CallData(resource_urlFormat, string.Empty, -1, string.Empty, user);
            } catch (Exception) {
                throw;
            }
        }

        public void FirstCallData(string resource_urlFormat, string friendsorfollowers, int count, string cursor)
        {
            try {
                CallData(resource_urlFormat, friendsorfollowers, count, cursor, null);
            } catch (Exception) {
                throw;
            }
        }

        private void CallData(string resource_urlFormat, string friendsorfollowers, int count, string cursor, User user)
        {
            JObject j = GetJSonObject(resource_urlFormat, friendsorfollowers, count, cursor, user);
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

                JValue next_cursor = (JValue)j[Constants.NEXT_CURSOR];
                if (long.Parse(next_cursor.Value.ToString()) > 0) {
                    try {
                        CallData(resource_urlFormat, friendsorfollowers, count, next_cursor.Value.ToString(), user);
                    } catch (Exception) {
                        if (next_cursor != null) {
                            next_cursor.Value = 0;
                        }
                        throw;
                    }
                }
            }
        }

        private JObject GetJSonObject(string resource_urlFormat, string friendsorfollowers, int count, string cursor, User user)
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
            }

            HttpWebRequest fRequest = (HttpWebRequest)WebRequest.Create(resource_url);
            fRequest.Headers.Add(Constants.AUTHORIZATION, string.Concat(twitAuthResponse.token_type, Constants.SPACE, twitAuthResponse.access_token));
            switch (tType)
            {
                case TwitterType.Unfollow:
                    fRequest.Method = Constants.POST;
                    break;
                case TwitterType.Friends:
                    fRequest.Method = Constants.GET;
                    break;
            }
            WebResponse response = fRequest.GetResponse();
            string result = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return JObject.Parse(result);
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
        public class User : ICloneable
        {
            private int index = 0;
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
    }
}