using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;

namespace NetVerser
{
    public static class Notify
    {
        private static WindowHwndSource source;        
        private const int NIM_ADD = 0;
        private const int NIM_MODIFY = 1;
        private const int NIM_DELETE = 2;
        //messages to trap
        public const int PK_TRAYICON = 1025;
        public const int RButtonUp = 0x0205;
        public const int NIN_BALLOONUSERCLICK = 1029;
        public const int WM_LBUTTONDBLCLK = 515;

        [Flags]
        public enum NotifyIconFlags
        {
            // The hIcon member is valid.
            Icon = 2,
            // The uCallbackMessage member is valid.
            Message = 1,
            // The szTip member is valid.
            ToolTip = 4,
            // The dwState and dwStateMask members are valid.
            State = 8,
            // Use a balloon ToolTip instead of a standard ToolTip. The szInfo, uTimeout, szInfoTitle, and dwInfoFlags members are valid.
            Balloon = 0x10,
        }

        public enum NotifyBalloonIcon
        {
            // No icon.
            None,
            // An information icon.
            Info,
            // A warning icon.
            Warning,
            // An error icon.
            Error,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class NOTIFYICONDATA
        {
            public int cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }
        
        [DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern int Shell_NotifyIcon(int message, NOTIFYICONDATA pnid);

        public static IntPtr AddToTray(Window window, string stringTip)
        {
            //hook window
            source = new WindowHwndSource((winChat)window);    
        
            //get icon
            Icon icon = new System.Drawing.Icon(global::NetVerser.Properties.Resources.Comment, 16, 16);

            //uID is the same uID used in ShowBallon
            NOTIFYICONDATA pnid = new NOTIFYICONDATA
            {
                hWnd = source.Handle,
                uID = 999,
                uFlags = (int)NotifyIconFlags.Message | (int)NotifyIconFlags.ToolTip | (int)NotifyIconFlags.Icon,
                uCallbackMessage = PK_TRAYICON,
                szTip = stringTip,
                hIcon = (IntPtr)icon.Handle,
            };
            //add icon
            Shell_NotifyIcon(NIM_ADD, pnid);

            //return source handle to be used in SetForegroundWindow
            return source.Handle;
        }

        public static void RemoveFromTray()
        {
            NOTIFYICONDATA pnid = new NOTIFYICONDATA
            {
                hWnd = source.Handle,
                uID = 999
            };
            //remove icon from tray
            Shell_NotifyIcon(NIM_DELETE, pnid);
        }

        public static IntPtr ShowBallon(int timeout, string stringTip, string tipTitle, string tipText, NotifyBalloonIcon tipIcon)
        {
            //get icon
            Icon icon = new System.Drawing.Icon(global::NetVerser.Properties.Resources.Comment, 16, 16);
            //uID is the same uID used in AddToTray 
            NOTIFYICONDATA pnid = new NOTIFYICONDATA
            {
                hWnd = source.Handle,
                uID = 999,
                uFlags = (int)NotifyIconFlags.Balloon | (int)NotifyIconFlags.Message | (int)NotifyIconFlags.ToolTip | (int)NotifyIconFlags.Icon,
                uTimeoutOrVersion = timeout,
                szInfoTitle = tipTitle,
                szInfo = tipText,
                dwInfoFlags = (int)tipIcon,
                hIcon = (IntPtr)icon.Handle,
                uCallbackMessage = PK_TRAYICON,
                szTip = stringTip,
            };
            //modify icon
            Shell_NotifyIcon(NIM_MODIFY, pnid);
            //return source handle to be used in SetForegroundWindow
            return source.Handle;
        }
    }

    //this is to hook WPF window messages
    public class WindowHwndSource : HwndSource
    {
        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);

        private winChat _reference;
        private const int Close = 0x10;

        internal WindowHwndSource(winChat component) : base(0, 0, 0, 0, 0, null, IntPtr.Zero)
        {
            _reference = component;
            AddHook(_reference.WndProc);
        }

        ~WindowHwndSource()
        {
            if (Handle != IntPtr.Zero)
            {
                PostMessage(new HandleRef(this, Handle), Close, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
