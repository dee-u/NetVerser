using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace NetVerser
{
    public static class Logger
    {
        public static void LogException(Exception e)
        {
            string stringException = string.Format("Message     : {0}Source      : {1}StackTrace  : {2}Target Site : {3}Date & Time : {4}Computer Name : {5}", e.Message + Environment.NewLine, e.Source + Environment.NewLine, e.StackTrace + Environment.NewLine, e.TargetSite.ToString() + Environment.NewLine, DateTime.Now.ToString() + Environment.NewLine, Environment.MachineName + Environment.NewLine);
            string AppExeName = Process.GetCurrentProcess().ProcessName + ".EXE";
            try
            {
                EventLog el = new EventLog();
                el.Source = AppExeName;
                el.WriteEntry(stringException, EventLogEntryType.Error);
            }
            catch
            {
            }
            finally
            {
                LogException(stringException, System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            }
        }

        private static void LogException(string exceptionMessage, string applicationPath)
        {
            string exceptionFile = applicationPath + "\\ExceptionLog" + DateTime.Now.ToString("MMddyyyy") + ".txt";
            FileStream fsError = null;
            StreamWriter sw = null;
            try
            {
                if (File.Exists(exceptionFile))
                {
                    fsError = new FileStream(exceptionFile, FileMode.Append, FileAccess.Write);
                    sw = new StreamWriter(fsError);
                    sw.WriteLine(Environment.NewLine + "------------------------------" + Environment.NewLine);
                    sw.WriteLine(exceptionMessage);
                }
                else
                {
                    fsError = new FileStream(exceptionFile, FileMode.Create, FileAccess.Write);
                    sw = new StreamWriter(fsError);
                    sw.WriteLine(exceptionMessage);
                }
                sw.Flush();

            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "Error In Logging",  System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                sw.Close();
                fsError.Close();
                sw.Dispose();
                fsError.Dispose();
                System.Windows.MessageBox.Show(exceptionMessage + Environment.NewLine + Environment.NewLine + "Please report to the programmer any persistent error.   ", "Error Encountered", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static void LogMessage(string Message)
        {
            FileStream fsError = null;
            StreamWriter sw = null;
            string ErrorFile = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\MessageLog.txt";
            try
            {
                if (File.Exists(ErrorFile))
                {
                    fsError = new FileStream(ErrorFile, FileMode.Append, FileAccess.Write);
                    sw = new StreamWriter(fsError);
                    sw.WriteLine(Environment.NewLine + "------------------------------" + Environment.NewLine);
                    sw.WriteLine(Process.GetCurrentProcess().ProcessName + " : " + Message);
                }
                else
                {
                    fsError = new FileStream(ErrorFile, FileMode.Create, FileAccess.Write);
                    sw = new StreamWriter(fsError);
                    sw.WriteLine(Process.GetCurrentProcess().ProcessName + " : " + Message);
                }
                sw.Flush();
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "Error In Logging", System.Windows.MessageBoxButton.OK,  System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                sw.Close();
                fsError.Close();
                sw.Dispose();
                fsError.Dispose();
            }
        }
    }
}
