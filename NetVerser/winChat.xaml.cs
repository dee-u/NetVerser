using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using NetVerser.Properties;

using System.Diagnostics;

namespace NetVerser
{
    /// <summary>
    /// Interaction logic for winChat.xaml
    /// </summary>
    public partial class winChat : Window
    {
        //delegates
        delegate void NewMessageEventHandler(object sender, NewMessageEventArgs e);
        delegate void SetTextCallback(string text);
        delegate void SetStatusCallback(string text);
        delegate void ShowWindowCallback();
        delegate void SetStripColorCallback(bool positiveOrNegative);
        delegate void FlashMeCallback();

        //DllImports
        [DllImport("user32.dll")]
        public static extern int FlashWindow(int hwnd, int bInvert);
        [DllImport("kernel32.dll")]
        public static extern bool Beep(uint dwFreq, uint dwDuration);
        [DllImport("wsock32.dll")]
        public static extern int GetHostByAddress(int address, int lenght, int protocol);
        [DllImport("User32.dll")]
        public static extern int SendMessage(int hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        
        //variables
        private UdpClient client = null;
        private IPEndPoint receivePoint;
        private bool programIsActive = false;
        private event NewMessageEventHandler NewMessage;
        private string ComputerName = Environment.MachineName.ToString();
        private TextRange tr;
        private const int HTCAPTION = 2;
        private const int WM_NCLBUTTONDOWN = 161;
        private const int _port = 5000;

        //used in SetForegroundWindow
        private IntPtr sourceHandle;

        public winChat()
        {
            //hide it initially
            this.Visibility = Visibility.Hidden;

            if (InstanceManager.IsNextInstance == false)
            {
                InitializeComponent();

                sourceHandle = Notify.AddToTray(this, "NetVerser (" + ComputerName + ")");                                
                contextMenuRunAtStartup.IsChecked = WillRunAtStartup();
                
                this.NewMessage += new NewMessageEventHandler(this.PromptNewMessage);
                this.Title = "NetVerser (" + ComputerName + ")";

                try
                {
                    client = new UdpClient(_port);
                    receivePoint = new IPEndPoint(new IPAddress(0), 0);
                    Thread readThread = new Thread(new ThreadStart(this.WaitForPockets));
                    readThread.Start();
                    this.ShowInTaskbar = true;
                }
                catch (Exception exc)
                {
                    Logger.LogException(exc);
                }
            }
        }

        private void PromptNewMessage(object sender, NewMessageEventArgs e)
        {
            bool chatIsActive = programIsActive;

            string stringMessage = e.Message;

            this.SetText(stringMessage);
            if (this.Visibility == Visibility.Hidden)
            {
                try
                {
                    Beep(440, 250);
                }
                catch (Exception exc)
                {
                    Logger.LogException(exc);
                }
                //notifyIcon1.ShowBalloonTip(1000, "New Message", stringMessage + Environment.NewLine + Environment.NewLine + "Click this balloon to show the Chat Window.", System.Windows.Forms.ToolTipIcon.Info);
                Notify.ShowBallon(1000, "NetVerser (" + ComputerName + ")", "New Message",  stringMessage + Environment.NewLine + Environment.NewLine + "Click this balloon to show the Chat Window.", Notify.NotifyBalloonIcon.Info);
                ShowWindow();
            }
            else
            {
                //if (programIsActive == false)
                if (chatIsActive == false)
                {
                    do
                    {
                        FlashMe();
                        System.Threading.Thread.Sleep(350);
                    }
                    while (programIsActive == false);
                    FlashMe();
                }
                else
                {
                    for (int a = 1; a <= 3; a++)
                    {
                        this.SetStripColor(false);
                        System.Threading.Thread.Sleep(250);
                        this.SetStripColor(true);
                        System.Threading.Thread.Sleep(250);
                    }
                }
            }
        }

        private void FlashMe()
        {
            if (!this.toolStripStatusLabel1.Dispatcher.CheckAccess())
            {
                FlashMeCallback d = new FlashMeCallback(FlashMe);
                this.Dispatcher.Invoke(d, new object[] { });
            }
            else
            {                
                int retval = FlashWindow(this.GetHandle().ToInt32(), 1);
            }
        }

        private void SetStripColor(bool positiveOrNegative)
        {
            if (!this.toolStripStatusLabel1.Dispatcher.CheckAccess())
            {
                SetStripColorCallback d = new SetStripColorCallback(SetStripColor);
                this.Dispatcher.Invoke(d, new object[] { positiveOrNegative });
            }
            else
            {
                if (positiveOrNegative.Equals(true))
                {
                    toolStripStatusLabel1.Background = Brushes.Transparent;
                    toolStripStatusLabel1.FontWeight = FontWeights.Regular;
                }
                else if (positiveOrNegative.Equals(false))
                {
                    toolStripStatusLabel1.Background = Brushes.Blue;
                    toolStripStatusLabel1.FontWeight = FontWeights.Bold;
                }
            }
        }

        private void SetText(string text)
        {
            // CheckAccess required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!this.textTranscript.Dispatcher.CheckAccess())
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Dispatcher.Invoke(d, new object[] { text });
            }
            else
            {
                tr = new TextRange(textTranscript.Document.ContentEnd, textTranscript.Document.ContentEnd);
                tr.Text = text;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Goldenrod);
                textTranscript.ScrollToEnd();
                this.textMessage.Focus();

                listBoxLog.Items.Add(toolStripStatusLabel1.Content + " " + DateTime.Now.ToString());
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
        }

        private void SetStatusBar(string text)
        {
            // CheckAccess required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!this.toolStripStatusLabel1.Dispatcher.CheckAccess())
            {
                SetStatusCallback d = new SetStatusCallback(SetStatusBar);
                this.Dispatcher.Invoke(d, new object[] { text });
            }
            else
            {
                toolStripStatusLabel1.Content = text;
            }
        }

        private void ShowWindow()
        {
            // CheckAccess required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (!this.Dispatcher.CheckAccess())
            {
                ShowWindowCallback d = new ShowWindowCallback(ShowWindow);
                this.Dispatcher.Invoke(d, new object[] { });
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {    
            PopulateComputers();
            toolStripStatusLabel1.Content = "Ready...";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (MessageBox.Show("Are you sure you want to close NetVerser? You will not be able to receive messages when you do so.   ", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            //{
            //    Notify.RemoveFromTray();
            //    System.Environment.Exit(System.Environment.ExitCode);
            //}
            //else
            //{
            //    e.Cancel = true;
            //    this.Hide();
            //}
            e.Cancel = true;
            this.Hide();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            PopulateComputers();
        }

        private void PopulateComputers()
        {
            try
            {
                comboRecipient.Items.Clear();
                NetworkBrowser nb = new NetworkBrowser();
                foreach (string pc in nb.getNetworkComputers())
                {
                    comboRecipient.Items.Add(pc);
                }

                //NetworkComputers nc = new NetworkComputers();
                //ArrayList rodelio = nc.ComputerList();
                //comboRecipient.Items.Clear();
                //for (int i = 0; i <= rodelio.Count - 1; i++)
                //{
                //    comboRecipient.Items.Add(rodelio[i].ToString());
                //}
            }
            catch (Exception exc)
            {
                Logger.LogException(exc);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            programIsActive = true;
            textMessage.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            programIsActive = false;
        }

        private void buttonHide_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ParseCommand(string instruction, string sender)
        {
            SendMessage(GetApplications(), sender);
        }

        private void WaitForPockets()
        {
            for (; ; )
            {
                try
                {
                    byte[] data = client.Receive(ref receivePoint);
                    string receivedMessage = System.Text.Encoding.ASCII.GetString(data);
                    int separatorPosition = receivedMessage.IndexOf(">>");
                    string messageSender = receivedMessage.Substring(0, separatorPosition - 1).Trim();
                   
                    //Console.WriteLine(receivedMessage);
                    //string message = receivedMessage.Substring(separatorPosition, receivedMessage.Length).Trim();
                    //Console.WriteLine(receivedMessage.Contains("ListApps").ToString());
                    if (receivedMessage.Contains("ListApps") == true)
                    {
                        ParseCommand("ListApps", messageSender);
                    }
                    else
                    {
                        this.SetStatusBar("Received message from " + messageSender);

                        receivedMessage = receivedMessage.Replace(">>", "«");

                        NewMessageEventArgs e = new NewMessageEventArgs(Environment.NewLine + receivedMessage);
                        //Raise event new message
                        OnNewMessage(e);
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogException(exc);
                    this.SetStatusBar("Error receiving message from " + Dns.GetHostEntry(receivePoint.Address.ToString()).HostName);
                }
            }
        }

        
        private void RunAtStartup(string strApplicationTitle, string strApplicationName, bool bolRun)
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (bolRun == true)
                    key.SetValue(strApplicationTitle, strApplicationName);
                else
                    key.DeleteValue(strApplicationTitle);
                key.Close();
            }
            catch (Exception exc)
            {
                Logger.LogException(exc);
            }
        }

        private bool WillRunAtStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                object NetVerserKey = key.GetValue("NetVerser");
                if (NetVerserKey == null)
                    return false;
                else
                    return true;
            }
            catch (Exception exc)
            {
                Logger.LogException(exc);
                return false;
            }
        }

        protected virtual void OnNewMessage(NewMessageEventArgs e)
        {
            if (NewMessage != null)
            {
                //MessageBox.Show(programIsActive.ToString());
                //Console.WriteLine("test");


                // Invokes the delegates
                NewMessage(this, e);
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (textMessage.Text.Length > 0)
            {
                SendMessage(comboRecipient.Text);
                if (comboRecipient.Text.Length > 0)
                    textMessage.Clear();
            }
            else
            {
                MessageBox.Show("There is no message to send.");
                textMessage.Focus();
            }
        }

        private void SendMessage(string stringRecipient)
        {
            if (stringRecipient.Length == 0)
            {
                MessageBox.Show("Please select a recipient to send message.");
                comboRecipient.Focus();
                return;
            }
            try
            {
                string packet = ComputerName + " >> " + textMessage.Text;
                string message = stringRecipient + " » " + textMessage.Text;

                tr = new TextRange(textTranscript.Document.ContentEnd, textTranscript.Document.ContentEnd);
                tr.Text = Environment.NewLine + message;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.ForestGreen);
                textMessage.Focus();

                byte[] data = System.Text.Encoding.ASCII.GetBytes(packet);
                client.Send(data, data.Length, stringRecipient, _port);
                SetStatusBar("Message is sent to " + stringRecipient);
                listBoxLog.Items.Add(toolStripStatusLabel1.Content + " " + DateTime.Now.ToString());
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1; listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
            catch (Exception exc)
            {
                string exceptionMessage = exc.Message;
                switch (exceptionMessage)
                {

                    case ("No such host is known"):
                        System.Windows.MessageBox.Show(stringRecipient + " cannot be reached.   ");
                        Logger.LogMessage(stringRecipient + " : No such host is known");
                        break;
                    default:
                        Logger.LogException(exc);
                        break;
                }
                SetStatusBar("Error sending message to " + stringRecipient);
                listBoxLog.Items.Add(toolStripStatusLabel1.Content);
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
        }

        private void SendMessage(string message, string stringRecipient)
        {
            try
            {
                string packet = ComputerName + " >> " + message;
                //string message = stringRecipient + " » " + textMessage.Text;

                //tr = new TextRange(textTranscript.Document.ContentEnd, textTranscript.Document.ContentEnd);
                //tr.Text = Environment.NewLine + message;
                //tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.ForestGreen);
                //textMessage.Focus();

                byte[] data = System.Text.Encoding.ASCII.GetBytes(packet);
                client.Send(data, data.Length, stringRecipient, _port);

                //SetStatusBar("Message is sent to " + stringRecipient);
                //listBoxLog.Items.Add(toolStripStatusLabel1.Content + " " + DateTime.Now.ToString());
                //listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1; listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
            catch (Exception exc)
            {
                string exceptionMessage = exc.Message;
                switch (exceptionMessage)
                {

                    case ("No such host is known"):
                        System.Windows.MessageBox.Show(stringRecipient + " cannot be reached.   ");
                        Logger.LogMessage(stringRecipient + " : No such host is known");
                        break;
                    default:
                        Logger.LogException(exc);
                        break;
                }
                SetStatusBar("Error sending message to " + stringRecipient);
                listBoxLog.Items.Add(toolStripStatusLabel1.Content);
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear conversation?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                textTranscript.Document.Blocks.Clear();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //move borderless window
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                int a = SendMessage(this.GetHandle().ToInt32(), WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void lblExit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


        //Show Menu
        private void ShowMenu()
        {
            //without this Escape will not unload the contextmenu
            SetForegroundWindow(sourceHandle);

            ContextMenuService.SetPlacement(NetVerserContext, System.Windows.Controls.Primitives.PlacementMode.MousePoint);
            NetVerserContext.IsOpen = true;     
        }

        //Used in Notify Icon
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Notify.PK_TRAYICON)
            {
                //right mouse button was clicked
                if ((int)lParam == Notify.RButtonUp)
                {
                    ShowMenu();
                    handled = true;
                }
                //balloon was clicked
                if ((int)lParam == Notify.NIN_BALLOONUSERCLICK)
                {
                    //PopulateComputers();
                    this.Visibility = Visibility.Visible;
                    handled = true;
                }
                //left double clicked
                if ((int)lParam == Notify.WM_LBUTTONDBLCLK)
                {
                    //PopulateComputers();
                    this.Visibility = Visibility.Visible;
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }       

        private void menuShow_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }

        private void menuRunAtStartup_Click(object sender, RoutedEventArgs e)
        {
            contextMenuRunAtStartup.IsChecked = !contextMenuRunAtStartup.IsChecked;
            if (contextMenuRunAtStartup.IsChecked)
                RunAtStartup("NetVerser", System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\NetVerser.exe", true);
            else
                RunAtStartup("NetVerser", System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\NetVerser.exe", false);
        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("NetVerser is a LAN chat system developed by Rodelio M. Rodriguez." + Environment.NewLine + Environment.NewLine + "rodeliorodriguez@gmail.com", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class NewMessageEventArgs : EventArgs
        {
            private readonly string _message;

            public NewMessageEventArgs(string message)
            {
                this._message = message;
            }

            public string Message
            {
                get { return _message; }
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            PopulateComputers();
        }



        public string GetApplications()
        {
            string apps = Environment.NewLine;
            //StringBuilder sb = new StringBuilder();
            foreach (Process p in Process.GetProcesses("."))
            {
                try
                {
                    if (p.MainWindowTitle.Length > 0)
                    {
                        //Console.WriteLine("Window Title:" + p.MainWindowTitle.ToString());
                        //Console.WriteLine("Process Name:" + p.ProcessName.ToString());
                        //Console.WriteLine("Window Handle:" + p.MainWindowHandle.ToString());
                        //Console.WriteLine("Memory Allocation:" + p.PrivateMemorySize64.ToString());
                        apps += p.ProcessName.ToString() + Environment.NewLine;
                    }
                }
                catch { }
            }
            return apps;
        }

    }

    public static class WindowExtension
    {
        public static IntPtr GetHandle(this Window w)
        {
            return new WindowInteropHelper(w).Handle;
        }
    }
  }
