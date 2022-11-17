using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetVerser.Windows
{
    public delegate void StartupNextInstanceEventHandler(Object sender, StartupNextInstanceEventArgs e);

    public class StartupNextInstanceEventArgs : EventArgs
    {
        private Boolean bringToForeground;

        public StartupNextInstanceEventArgs(Boolean bringToForeground)
        {
            this.bringToForeground = bringToForeground;
        }

        public Boolean BringToForeground
        {
            get { return bringToForeground; }
            set { bringToForeground = value; }
        }
    }

    [Serializable]
    public class CannotStartSingleInstanceException : Exception
    {
        public CannotStartSingleInstanceException() : base("Cannot connect to the original single-instance application") { }
        public CannotStartSingleInstanceException(String message) : base(message) { }
        protected CannotStartSingleInstanceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public CannotStartSingleInstanceException(String message, Exception inner) : base(message, inner) { }
    }
}
