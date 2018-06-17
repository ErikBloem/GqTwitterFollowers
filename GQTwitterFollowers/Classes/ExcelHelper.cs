using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;

namespace GQTwitterFollowers
{
    public class ExcelHelper
    {
        public static BindingList<Twitter.TweetId> ImportTweetIdsFromExcel(string sFilename)
        {
            BindingList<Twitter.TweetId> tweetids = null;

            try {
                Application xlObject = null;
                Workbook xlWB = null;
                Worksheet xlSh = null;
                try {
                    xlObject = new Application();
                    xlObject.ErrorCheckingOptions.NumberAsText = false;

                    //'This Adds a new woorkbook, you could open the workbook from file also
                    xlWB = xlObject.Workbooks.Open(sFilename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    xlSh = (Worksheet)xlObject.ActiveWorkbook.ActiveSheet;

                    Range usedrange = xlSh.UsedRange;
                    tweetids = new BindingList<Twitter.TweetId>();

                    int index = 1;
                    foreach (Range r in usedrange.Rows) {
                        //no header
                        if (r.Row > 1) {
                            Twitter.TweetId tweetid = new Twitter.TweetId();
                            tweetid.Index = index;
                            for (int cCnt = 1; cCnt <= usedrange.Columns.Count; cCnt++) {
                                Range rgobj = (usedrange.Cells[r.Row, cCnt] as Range);
                                object str = (object)rgobj.Text;
                                if (cCnt == 1) {
                                    tweetid.Tweet_Id = str.ToString();
                                } else if (cCnt == 2) {
                                    tweetid.In_reply_to_status_id = str.ToString();
                                } else if (cCnt == 3) {
                                    tweetid.In_reply_to_user_id = str.ToString();
                                } else if (cCnt == 4) {
                                    tweetid.Timestamp = str.ToString();
                                } else if (cCnt == 5) {
                                    tweetid.Source = str.ToString();
                                } else if (cCnt == 6) {
                                    tweetid.Text = str.ToString();
                                } else if (cCnt == 7) {
                                    tweetid.Retweeted_status_id = str.ToString();
                                } else if (cCnt == 8) {
                                    tweetid.Retweeted_status_user_id = str.ToString();
                                } else if (cCnt == 9) {
                                    tweetid.Retweeted_status_timestamp = str.ToString();
                                } else if (cCnt == 10) {
                                    tweetid.Expanded_urls = str.ToString();
                                }
                            }
                            tweetids.Add(tweetid);
                            index++;
                        }
                    }
                } catch (System.Runtime.InteropServices.COMException) {

                } catch (Exception) {

                } finally {
                    try {
                        if (xlWB != null) xlWB.Close(null, null, null);
                        xlObject.Workbooks.Close();
                        xlObject.Quit();
                        if (xlSh != null) Marshal.ReleaseComObject(xlSh);
                        if (xlWB != null) Marshal.ReleaseComObject(xlWB);
                        if (xlObject != null) Marshal.ReleaseComObject(xlObject);
                    } catch {
                    }
                    xlSh = null;
                    xlWB = null;
                    xlObject = null;
                    // force final cleanup!
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            } catch (Exception) {
            }
            return tweetids;
        }

        public static BindingList<Twitter.User> ImportFromExcel(string sFilename)
        {
            BindingList<Twitter.User> users = null;

            try {
                Application xlObject = null;
                Workbook xlWB = null;
                Worksheet xlSh = null;
                try {
                    xlObject = new Application();

                    //'This Adds a new woorkbook, you could open the workbook from file also
                    xlWB = xlObject.Workbooks.Open(sFilename, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    //xlSh = (Worksheet)xlObject.ActiveWorkbook.ActiveSheet;

                    Range usedrange = xlSh.UsedRange;

                    users = new BindingList<Twitter.User>();

                    int index = 1;
                    foreach (Range r in usedrange.Rows) {
                        //no header
                        if (r.Row > 1) {
                            Twitter.User user = new Twitter.User();
                            user.Index = index;
                            for (int cCnt = 1; cCnt <= usedrange.Columns.Count; cCnt++)
                            {
                                object str = (object)(usedrange.Cells[r.Row, cCnt] as Range).Value2;
                                if (cCnt == 1) {
                                    user.ScreenName = str.ToString();
                                } else if (cCnt == 2) {
                                    user.ProfileImage = str.ToString();
                                } else if (cCnt == 3) {
                                    user.UserId = str.ToString();
                                }
                            }
                            users.Add(user);
                            index++;
                        }
                    }
                }
                catch (System.Runtime.InteropServices.COMException) {

                } catch (Exception) {

                } finally {
                    try {
                        if (xlWB != null) xlWB.Close(null, null, null);
                        xlObject.Workbooks.Close();
                        xlObject.Quit();
                        if (xlSh != null) Marshal.ReleaseComObject(xlSh);
                        if (xlWB != null) Marshal.ReleaseComObject(xlWB);
                        if (xlObject != null) Marshal.ReleaseComObject(xlObject);
                    } catch {
                    }
                    xlSh = null;
                    xlWB = null;
                    xlObject = null;
                    // force final cleanup!
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            } catch (Exception) {
            }
            return users;
        }

        public static string ExportToExcel(string sFilename, BindingList<Twitter.User> users)
        {
            string sErrorMessage = string.Empty;
            try {
                Application xlObject = null;
                Workbook xlWB = null;
                Worksheet xlSh = null;
                CultureInfo oci = CultureInfo.CurrentCulture;
                try {
                    xlObject = new Application();
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                    xlObject.AlertBeforeOverwriting = false;
                    xlObject.DisplayAlerts = false;

                    //'This Adds a new woorkbook, you could open the workbook from file also
                    xlWB = xlObject.Workbooks.Add(Type.Missing);
                    xlWB.SaveAs(sFilename, 56, Missing.Value, Missing.Value, Missing.Value, Missing.Value, XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                    xlSh = (Worksheet)xlObject.ActiveWorkbook.ActiveSheet;

                    xlSh.Cells[1, 1] = Constants.GetVariableName(() => users[0].ScreenName);
                    xlSh.Cells[1, 2] = Constants.GetVariableName(() => users[0].ProfileImage);
                    xlSh.Cells[1, 3] = Constants.GetVariableName(() => users[0].UserId);

                    xlObject.ErrorCheckingOptions.NumberAsText = false;
                    for (int i = 1; i <= users.Count; i++) {
                        ((Range)xlSh.Cells[i + 1, 3]).NumberFormat = "@";
                        xlSh.Cells[i + 1, 1] = users[i - 1].ScreenName;
                        xlSh.Cells[i + 1, 2] = users[i - 1].ProfileImage;
                        xlSh.Cells[i + 1, 3] = users[i - 1].UserId;
                    }
                    Range usedrange = xlSh.UsedRange;

                    xlSh.Columns.AutoFit();
                    xlSh.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, usedrange, Missing.Value, Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes, Missing.Value).Name = "Table7";
                    xlSh.ListObjects.get_Item("Table7").TableStyle = "TableStyleMedium2";

                    xlWB.Save();
                } catch (System.Runtime.InteropServices.COMException ex) {
                    if (ex.ErrorCode == -2147221164) {
                        sErrorMessage = String.Concat(Resources.Resource.ErrorExport, ": ", Resources.Resource.MOfficeInstall);
                    } else if (ex.ErrorCode == -2146827284) {
                        sErrorMessage = String.Concat(Resources.Resource.ErrorExport, ": ", ex.Message, Environment.NewLine, Constants.SPACE, Resources.Resource.Error, ": ", Resources.Resource.ExcelMaxRows);
                    } else {
                        sErrorMessage = String.Concat(Resources.Resource.ErrorExport, ": ", ex.Message, Environment.NewLine, Constants.SPACE, Resources.Resource.Error, ": ", ex.ErrorCode.ToString());
                    }
                } catch (Exception ex) {
                    sErrorMessage = String.Concat(Resources.Resource.ErrorExport, ": ", ex.Message);
                } finally {
                    try {
                        if (xlWB != null) xlWB.Close(null, null, null);
                        xlObject.Workbooks.Close();
                        xlObject.Quit();
                        if (xlSh != null) Marshal.ReleaseComObject(xlSh);
                        if (xlWB != null) Marshal.ReleaseComObject(xlWB);
                        if (xlObject != null) Marshal.ReleaseComObject(xlObject);
                    } catch {
                    }
                    xlSh = null;
                    xlWB = null;
                    xlObject = null;
                    // force final cleanup!
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.CurrentThread.CurrentCulture = oci;
                }
            } catch (Exception ex) {
                sErrorMessage = String.Concat(Resources.Resource.ErrorExport, ": ", ex.Message);
            }
            return sErrorMessage;
        }
    }
}