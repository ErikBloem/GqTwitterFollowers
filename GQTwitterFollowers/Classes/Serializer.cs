﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.Serialization;
using System.Xml;

namespace GQTwitterFollowers
{
    internal class Serializer
    {
        private static string Extension = ".xml";
        private static string ExcelExtension = ".xls";
        private const string ExcelXExtension = ".xlsx";
        private static string Copy = " - Copy";
        private static string XMLMENU = "XmlMenuBar";

        internal static string GetNavMenuXMLPath()
        {
            return string.Concat(AppDomain.CurrentDomain.BaseDirectory, XMLMENU, Extension);
        }

        internal static XmlDocument GetNavMenuXML()
        {
            string filename = string.Concat(AppDomain.CurrentDomain.BaseDirectory, XMLMENU, Extension);
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                return doc;
            }
            return null;
        }

        internal static void DeleteListXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists)
            {
                string newfilename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Copy, Extension));
                FileInfo fileinfonew = new FileInfo(newfilename);
                if (fileinfonew.Exists) { File.Delete(newfilename); }
                File.Copy(filename, newfilename, false);

                File.Delete(filename);
            }
        }

        internal static void WriteFollowersXML(List<Twitter.User> users, string startupFile)
        {
            if (users != null) {
                int index = 1;
                foreach (Twitter.User user in users) {
                    user.Index = index;
                    index++;
                }
            }

            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            //try to create copy
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists)
            {
                string newfilename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Copy, Extension));
                FileInfo fileinfonew = new FileInfo(newfilename);
                if (fileinfonew.Exists) { File.Delete(newfilename); }
                File.Copy(filename, newfilename, false);

                File.Delete(filename);
            }

            //write new file
            XmlSerializer writer = new XmlSerializer(typeof(List<Twitter.User>));
            using (FileStream file = File.OpenWrite(filename))
            {
                writer.Serialize(file, users);
            }
        }

        internal static void WriteTimelineXML(List<Twitter.Timeline> timelines, string startupFile)
        {
            if (timelines != null) {
                int index = 1;
                foreach (Twitter.Timeline timeline in timelines) {
                    timeline.Index = index;
                    index++;
                }
            }

            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            //try to create copy
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                string newfilename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Copy, Extension));
                FileInfo fileinfonew = new FileInfo(newfilename);
                if (fileinfonew.Exists) { File.Delete(newfilename); }
                File.Copy(filename, newfilename, false);

                File.Delete(filename);
            }

            //write new file
            XmlSerializer writer = new XmlSerializer(typeof(List<Twitter.Timeline>));
            using (FileStream file = File.OpenWrite(filename)) {
                writer.Serialize(file, timelines);
            }
        }

        internal static List<Twitter.User> ReadListXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists)
            {
                XmlSerializer reader = new XmlSerializer(typeof(List<Twitter.User>));
                using (FileStream input = File.OpenRead(filename))
                {
                    return reader.Deserialize(input) as List<Twitter.User>;
                }
            }
            return new List<Twitter.User>();
        }

        internal static List<Twitter.Timeline> ReadTimelineListXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                XmlSerializer reader = new XmlSerializer(typeof(List<Twitter.Timeline>));
                using (FileStream input = File.OpenRead(filename)) {
                    return reader.Deserialize(input) as List<Twitter.Timeline>;
                }
            }
            return new List<Twitter.Timeline>();
        }

        internal static BindingList<Twitter.User> ReadBindingListXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists)
            {
                XmlSerializer reader = new XmlSerializer(typeof(BindingList<Twitter.User>));
                using (FileStream input = File.OpenRead(filename))
                {
                    return reader.Deserialize(input) as BindingList<Twitter.User>;
                }
            }
            return null;
        }

        internal static BindingList<Twitter.Timeline> ReadTimelineBindingListXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                XmlSerializer reader = new XmlSerializer(typeof(BindingList<Twitter.Timeline>));
                using (FileStream input = File.OpenRead(filename)) {
                    return reader.Deserialize(input) as BindingList<Twitter.Timeline>;
                }
            }
            return new BindingList<Twitter.Timeline>();
        }

        internal static BindingList<Twitter.TweetId> TweetIdsListExcel(string startupFile)
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(startupFile, ExcelXExtension));
            return ExcelHelper.ImportTweetIdsFromExcel(filename);
        }

        internal static BindingList<Twitter.User> ReadDestroyListExcel(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, ExcelExtension));
            return ExcelHelper.ImportFromExcel(filename);
        }

        internal static BindingList<Twitter.TweetId> ReadTweetIdsExcelXML(string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                XmlSerializer reader = new XmlSerializer(typeof(BindingList<Twitter.TweetId>));
                using (FileStream input = File.OpenRead(filename)) {
                    return reader.Deserialize(input) as BindingList<Twitter.TweetId>;
                }
            }
            return new BindingList<Twitter.TweetId>();
        }

        internal static void WriteTweetIdsExcelXML(List<Twitter.TweetId> tweetids, string startupFile)
        {
            if (tweetids != null) {
                int index = 1;
                foreach (Twitter.TweetId tweetid in tweetids) {
                    tweetid.Index = index;
                    index++;
                }
            }

            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Extension));
            //try to create copy
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists) {
                string newfilename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Copy, Extension));
                FileInfo fileinfonew = new FileInfo(newfilename);
                if (fileinfonew.Exists) { File.Delete(newfilename); }
                File.Copy(filename, newfilename, false);

                File.Delete(filename);
            }

            //write new file
            XmlSerializer writer = new XmlSerializer(typeof(List<Twitter.TweetId>));
            using (FileStream file = File.OpenWrite(filename)) {
                writer.Serialize(file, tweetids);
            }
        }

        internal static string WriteDestroyListExcel(BindingList<Twitter.User> users, string startupFile)
        {
            string filename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, ExcelExtension));
            //try to create copy
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Exists)
            {
                string newfilename = Path.Combine(string.Concat(AppDomain.CurrentDomain.BaseDirectory, Constants.FILES), string.Concat(startupFile, Copy, ExcelExtension));
                FileInfo fileinfonew = new FileInfo(newfilename);
                if (fileinfonew.Exists) { File.Delete(newfilename); }
                File.Copy(filename, newfilename, false);

                File.Delete(filename);
            }

            //write new file
            return ExcelHelper.ExportToExcel(filename, users);
        }
    }

    internal static class EnumUtils
    {
        internal static Nullable<T> Parse<T>(string input) where T : struct
        {
            //since we cant do a generic type constraint
            if (!typeof(T).IsEnum) {
                throw new ArgumentException("Generic Type 'T' must be an Enum");
            }
            if (!string.IsNullOrEmpty(input)) {
                if (Enum.GetNames(typeof(T)).Any(
                      e => e.Trim().ToUpperInvariant() == input.Trim().ToUpperInvariant())) {
                    return (T)Enum.Parse(typeof(T), input, true);
                }
            }
            return null;
        }
    }
}