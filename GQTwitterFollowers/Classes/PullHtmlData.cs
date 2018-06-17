using System.Net;
using System.IO;
using System;
using System.Text;

namespace GQTwitterFollowers.Classes
{
    public class PullHtmlData
    {
        public static string GetHtmlData(string url)
        {
            string result = null;
            WebResponse response = null;
            StreamReader reader = null;
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = reader.ReadToEnd();

            } catch (Exception ex) {
                // handle error

            } finally {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
            return result;
        }
    }
}
