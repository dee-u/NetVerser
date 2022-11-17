using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace NetVerser
{
    public static class InstanceManager
    {
        public static bool IsNextInstance { get; set; }

        public static void SetSecondInstance()
        {
            IsNextInstance = true;
            MessageBox.Show("Only one instance of NetVerser is allowed to run.", "NetVerser");
        }
    }
}
