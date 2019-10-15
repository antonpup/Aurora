using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Aurora.Devices.Omen
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightingColor
    {
        public byte r;
        public byte g;
        public byte b;

        public static LightingColor FromColor(Color c)
        {
            LightingColor lc = new LightingColor()
            {
                r = c.R,
                g = c.G,
                b = c.B
            };
            return lc;
        }
    }
}
