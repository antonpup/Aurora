using Aurora.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurora.Settings
{
    public static class Logger
    {
        public static NLog.Logger Log;
        
        public static bool Initialized { get; private set; } = false;

        public static bool Initialize()
        {
            if (Log != null)
                return (Initialized = true);

            Log = LogManager.GetLogger("global");
            OutputLogHeader();
            return (Initialized = true);
        }

        private static void OutputLogHeader()
        {

        }
    }
}
