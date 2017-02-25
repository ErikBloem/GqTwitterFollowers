using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GQTwitterFollowers
{
    public interface IConsumerCredentials
    {
        /// <summary>
        /// Key identifying a specific consumer application
        /// </summary>
        string ConsumerKey { get; set; }

        /// <summary>
        /// Secret Key identifying a specific consumer application
        /// </summary>
        string ConsumerSecret { get; set; }

        /// <summary>
        /// Screen name of user
        /// </summary>
        string ScreenName { get; set; }

        /// <summary>
        /// Token required for Application Only Authentication
        /// </summary>
        string ApplicationOnlyBearerToken { get; set; }

        /// <summary>
        /// Clone the current credentials.
        /// </summary>
        IConsumerCredentials Clone();

        /// <summary>
        /// Are credentials correctly set up for application only authentication.
        /// </summary>
        bool AreSetupForApplicationAuthentication();
    }

    public class ConsumerCredentials : IConsumerCredentials
    {
        public ConsumerCredentials(string consumerKey, string consumerSecret, string screen_name)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            ScreenName = screen_name;
        }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string ScreenName { get; set; }

        public string ApplicationOnlyBearerToken { get; set; }

        public IConsumerCredentials Clone()
        {
            var clone = new ConsumerCredentials(ConsumerKey, ConsumerSecret, ScreenName);

            CopyPropertiesToClone(clone);

            return clone;
        }

        public bool AreSetupForApplicationAuthentication()
        {
            return !string.IsNullOrEmpty(ConsumerKey) &&
                   !string.IsNullOrEmpty(ConsumerSecret);
        }

        protected void CopyPropertiesToClone(IConsumerCredentials clone)
        {
            clone.ApplicationOnlyBearerToken = ApplicationOnlyBearerToken;
        }
    }

    /// <summary>
    /// Defines a contract of 4 information to connect to an OAuth service
    /// </summary>
    public interface ITwitterCredentials : IConsumerCredentials
    {
        /// <summary>
        /// Key provided to the consumer to provide an authentication of the client
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// Secret Key provided to the consumer to provide an authentication of the client
        /// </summary>
        string AccessTokenSecret { get; set; }

        /// <summary>
        /// Clone the current credentials.
        /// </summary>
        new ITwitterCredentials Clone();

        /// <summary>
        /// Are credentials correctly set up for user authentication.
        /// </summary>
        bool AreSetupForUserAuthentication();
    }

    /// <summary>
    /// This class provides host basic information for authorizing a OAuth
    /// consumer to connect to a service. It does not contain any logic
    /// </summary>
    public class TwitterCredentials : ConsumerCredentials, ITwitterCredentials
    {
        public TwitterCredentials() : base(null, null, null) { }

        public TwitterCredentials(string consumerKey, string consumerSecret, string screenName)
            : base(consumerKey, consumerSecret, screenName)
        {
        }

        public TwitterCredentials(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string screenName)
            : this(consumerKey, consumerSecret, screenName)
        {
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
        }

        public TwitterCredentials(IConsumerCredentials credentials) : base("", "", "")
        {
            if (credentials != null)
            {
                ConsumerKey = credentials.ConsumerKey;
                ConsumerSecret = credentials.ConsumerSecret;
                ScreenName = credentials.ScreenName;

                ApplicationOnlyBearerToken = credentials.ApplicationOnlyBearerToken;
            }
        }

        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public new ITwitterCredentials Clone()
        {
            var clone = new TwitterCredentials(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret, ScreenName);

            CopyPropertiesToClone(clone);

            return clone;
        }

        public bool AreSetupForUserAuthentication()
        {
            return AreSetupForApplicationAuthentication() &&
                   !string.IsNullOrEmpty(AccessToken) &&
                   !string.IsNullOrEmpty(AccessTokenSecret);
        }
    }
}