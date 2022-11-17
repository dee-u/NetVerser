using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;

namespace NetVerser
{
    public class NetworkComputers
    {
        ArrayList aList = new ArrayList();
        NETRESOURCE nResource = new NETRESOURCE();
        // The WNetOpenEnum function starts an enumeration of network resources or 
        // existing connections. You can continue the enumeration by calling the 
        // WNetEnumResource function.
        [DllImport("mpr.dll")]
        public static extern int WNetOpenEnum(ResourceScope dwScope, ResourceType dwType, ResourceUsage dwUsage, NETRESOURCE lpNetResource, out IntPtr lphEnum);

        // The WNetEnumResource function continues an enumeration of network resources 
        // that was started by a call to the WNetOpenEnum function.
        [DllImport("mpr.dll")]
        public static extern int WNetEnumResource(IntPtr hEnum, ref uint lpcCount, IntPtr lpBuffer, ref uint lpBufferSize);

        // The WNetCloseEnum function ends a network resource enumeration started by a 
        // call to the WNetOpenEnum function.
        [DllImport("mpr.dll")]
        public static extern int WNetCloseEnum(IntPtr hEnum);

        // Structure NETRESOURCE.
        [StructLayout(LayoutKind.Sequential)]
        public class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public ResourceUsage dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        }

        // Enum ResourceScope.
        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
        };

        // Enum ResourceType.
        public enum ResourceType
        {
            RESOURCETYPE_ANY = 0,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
        };

        // Enum ResourceDisplayType.
        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC = 0,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
        };

        // Enum ResourceUsage.
        public enum ResourceUsage
        {
            RESOURCEUSAGE_ALL = 0,
            RESOURCEUSAGE_CONNECTABLE,
            RESOURCEUSAGE_CONTAINER,
        };

        // Enum Error Codes.
        public const int NO_ERROR = 0;
        public const int ERROR_NO_MORE_ITEMS = 259;

        public ArrayList ComputerList()
        {
            try
            {
                nResource.dwDisplayType = ResourceDisplayType.RESOURCEDISPLAYTYPE_SERVER;
                //nResource.lpProvider = "WORKGROUP";
                EnumerateServers(ResourceScope.RESOURCE_GLOBALNET, ResourceType.RESOURCETYPE_DISK, ResourceDisplayType.RESOURCEDISPLAYTYPE_GENERIC, ResourceUsage.RESOURCEUSAGE_CONTAINER, nResource);
                return aList;
            }
            catch (Exception exc)
            {
                Logger.LogException(exc);
                return null;
            }

        }

        // Enumerate network resources or existing connections.
        // --------------------------------------------------------------------------
        public void EnumerateServers(ResourceScope rScope, ResourceType rType, ResourceDisplayType rDisplayType, ResourceUsage rUsage, NETRESOURCE pNR)
        {

            try
            {

                // Variables.
                int rAPI;					// API return.
                IntPtr hEnum = IntPtr.Zero;	// Handle to the enum.
                uint bSize = 16384;			// 16K is a good size.
                IntPtr buffer = Marshal.AllocHGlobal((int)bSize); // Allocate memory.
                uint eEntries = 1;			// Enumerate all possible entries

                // Start an enumeration of network resources or existing connections.
                rAPI = WNetOpenEnum(rScope, rType, rUsage, pNR, out hEnum);

                if (rAPI != NO_ERROR)
                {
                    //Common.Logger.LogMessage("Errored in WNetOpenEnum");
                    // Process errors with an application-defined error handler.
                }
                else
                {
                    do
                    {
                        // Continue an enumeration of network resources. 
                        rAPI = WNetEnumResource(hEnum, ref eEntries, buffer, ref bSize);

                        if (rAPI != NO_ERROR)
                        {
                            //Common.Logger.LogMessage("Errored in WNetEnumResource");
                            // Process errors with an application-defined error handler.
                            return;
                        }
                        else
                        {
                            // Marshal data from an unmanaged block of memory to a 
                            // managed object.
                            Marshal.PtrToStructure(buffer, pNR);
                            if (pNR.dwDisplayType == ResourceDisplayType.RESOURCEDISPLAYTYPE_SERVER)
                            {
                                // Add lpRemoteName from NETRESOURCE to Array list.
                                if (pNR.lpRemoteName.Substring(0, 2).Equals(@"\\"))
                                {
                                    aList.Add(pNR.lpRemoteName.Replace(@"\\", ""));
                                }
                            }
                            // If the NETRESOURCE structure represents a container resource, 
                            // call the EnumerateServers function recursively.
                            if ((ResourceUsage.RESOURCEUSAGE_CONTAINER == (pNR.dwUsage & ResourceUsage.RESOURCEUSAGE_CONTAINER)))
                            {
                                EnumerateServers(rScope, rType, rDisplayType, rUsage, pNR);
                            }
                        }
                        // Loop until rAPI != 259, Constant = no more items.
                    } while (rAPI != ERROR_NO_MORE_ITEMS);

                    // Call WNetCloseEnum to end the enumeration.
                    WNetCloseEnum(hEnum);
                }

                // Frees memory previously allocated from the unmanaged memory of the 
                // process with AllocHGlobal.
                Marshal.FreeHGlobal((IntPtr)buffer);
            }
            catch (Exception exc)
            {
                Logger.LogException(exc);
            }
        }
    }
}
