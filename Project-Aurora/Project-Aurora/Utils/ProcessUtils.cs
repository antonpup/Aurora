using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class ProcessUtils
    {
        public static bool AnyProcessExists(string[] processes)
        {
            foreach(string proc in processes)
            {
                if (Process.GetProcessesByName(proc).Length > 0)
                    return true;
            }


            return false;
        }
    }
}
