using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace GQTwitterFollowers
{
    public class Constants
    {
        public const string USERS = "users";
        public const string SCREEN_NAME = "screen_name";
        public const string PROFILE_IMAGE_URL = "profile_image_url";
        public const string ID = "id";
        public const string CURSOR = "cursor";
        public const string NEXT_CURSOR = "next_cursor";

        public const string AUTHORIZATION = "Authorization";
        public const string GET = "Get";
        public const string POST = "POST";

        public const string OAUTHURL = "https://api.twitter.com/oauth2/token";
        public const string CONTENTTYPE = "application/x-www-form-urlencoded; charset=UTF-8";
        public const string ACCEPTENCODING = "Accept-Encoding";
        public const string GZIP = "gzip";
        public const string POSTBODY = "grant_type=client_credentials";

        public const string AND = "&";
        public const string BACKSLASH = "\"";
        public const string FILES = "Files";
        public const string SPACE = " ";
        public const string STYLE = "style";
        public const string COLON = ":";
        public const string CLASS = "class";

        public const string BASIC = "Basic";
        public const string BEARER = "Bearer";

        public const string SPAN = "span";
        public const string LI = "li";
        public const string A = "a";

        public const string TITLE = "title";
        public const string TARGET = "target";
        public const string BLANK = "_blank";
        public const string GRIDIMAGE = "Grid-image";
        public const string GRIDIMAGETD = "Grid-image-td";
        public const string GRIDCHECKBOXTD = "Grid-checkbox-td";
        public const string DISPLAYNONE = "display: none;";

        public const string HREF = "href";
        public const string HTTPTWITTER = "https://twitter.com/";

        public const string IMG = "img";
        public enum FriendsFollowers
        {
            Friends,
            Followers,
            Analyze,
            Exclude,
            Unfollow,
            NotFollowing,
            NeverFollow,
            Users
        };

        internal static string GetVariableName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;
            return body.Member.Name;
        }
    }
}