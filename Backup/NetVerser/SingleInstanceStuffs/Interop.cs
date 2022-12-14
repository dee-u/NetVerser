using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NetVerser
{
    //Native Types
    internal sealed class NativeTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public sealed class SECURITY_ATTRIBUTES : IDisposable
        {
            public Int32 nLength;
            public IntPtr lpSecurityDescriptor;
            public Boolean bInheritHandle;
            public SECURITY_ATTRIBUTES()
            {
                this.nLength = Marshal.SizeOf(typeof(NativeTypes.SECURITY_ATTRIBUTES));
            }

            public void Dispose()
            {
                DisposeCore();
                GC.SuppressFinalize(this);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void DisposeCore()
            {
                if (this.lpSecurityDescriptor != IntPtr.Zero)
                {
                    UnsafeNativeMethods.LocalFree(this.lpSecurityDescriptor);
                    this.lpSecurityDescriptor = IntPtr.Zero;
                }
            }

            ~SECURITY_ATTRIBUTES()
            {
                this.DisposeCore();
            }
        }
    }

    //Safe handle
    internal sealed class DCSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private DCSafeHandle() : base(true) { }

        protected override Boolean ReleaseHandle()
        {
            return UnsafeNativeMethods.DeleteDC(base.handle);
        }
    }

    //Unsafe methods
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetModuleFileName(HandleRef hModule, StringBuilder buffer, Int32 length);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFileMapping(HandleRef hFile, [MarshalAs(UnmanagedType.LPStruct)] NativeTypes.SECURITY_ATTRIBUTES lpAttributes, Int32 flProtect, Int32 dwMaxSizeHi, Int32 dwMaxSizeLow, String lpName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr MapViewOfFile(HandleRef hFileMapping, Int32 dwDesiredAccess, Int32 dwFileOffsetHigh, Int32 dwFileOffsetLow, Int32 dwNumberOfBytesToMap);

        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean UnmapViewOfFile(HandleRef pvBaseAddress);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenFileMapping(Int32 dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] Boolean bInheritHandle, String lpName);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr LocalFree(IntPtr LocalHandle);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Boolean DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern Int32 GetDeviceCaps(DCSafeHandle hDC, Int32 nIndex);

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto)]
        public static extern DCSafeHandle IntCreateDC(String lpszDriver, String lpszDeviceName, String lpszOutput, IntPtr devMode);

        public static DCSafeHandle CreateDC(String lpszDriver)
        {
            return UnsafeNativeMethods.IntCreateDC(lpszDriver, null, null, IntPtr.Zero);
        }
    }
}

namespace NetVerser.Internal
{
    //Native Methods
    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ConvertStringSecurityDescriptorToSecurityDescriptor(String StringSecurityDescriptor, UInt32 StringSDRevision, ref IntPtr SecurityDescriptor, IntPtr SecurityDescriptorSize);
    }
}