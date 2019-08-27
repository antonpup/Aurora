using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.NZXT
{
    public static class NZXTLayoutMap
    {
        public static readonly Dictionary<DeviceKeys, int> HuePlus = new Dictionary<DeviceKeys, int>
        {
            {DeviceKeys.ONE, 0},
            {DeviceKeys.TWO, 3},
            {DeviceKeys.THREE, 6},
            {DeviceKeys.FOUR, 9},
            {DeviceKeys.FIVE, 12},
            {DeviceKeys.SIX, 15},
            {DeviceKeys.SEVEN, 18},
            {DeviceKeys.EIGHT, 21},
            {DeviceKeys.NINE, 24},
            {DeviceKeys.ZERO, 27},
            {DeviceKeys.Q, 30},
            {DeviceKeys.W, 33},
            {DeviceKeys.E, 36},
            {DeviceKeys.R, 39},
            {DeviceKeys.T, 42},
            {DeviceKeys.Y, 45},
            {DeviceKeys.U, 48},
            {DeviceKeys.I, 51},
            {DeviceKeys.O, 54},
            {DeviceKeys.P, 57},
            {DeviceKeys.A, 60},
            {DeviceKeys.S, 63},
            {DeviceKeys.D, 66},
            {DeviceKeys.F, 69},
            {DeviceKeys.G, 72},
            {DeviceKeys.H, 75},
            {DeviceKeys.J, 78},
            {DeviceKeys.K, 81},
            {DeviceKeys.L, 84},
            {DeviceKeys.SEMICOLON, 87},
            {DeviceKeys.Z ,90},
            {DeviceKeys.X ,93},
            {DeviceKeys.C ,96},
            {DeviceKeys.V ,99},
            {DeviceKeys.B ,102},
            {DeviceKeys.N ,105},
            {DeviceKeys.M ,108},
            {DeviceKeys.COMMA ,111},
            {DeviceKeys.PERIOD ,114},
            {DeviceKeys.FORWARD_SLASH ,117}
        };

        public static readonly Dictionary<DeviceKeys, int> KrakenX = new Dictionary<DeviceKeys, int>
        {
            {DeviceKeys.ONE, 0},
            {DeviceKeys.TWO, 3},
            {DeviceKeys.THREE, 6},
            {DeviceKeys.FOUR, 9},
            {DeviceKeys.FIVE, 12},
            {DeviceKeys.SIX, 15},
            {DeviceKeys.SEVEN, 18},
            {DeviceKeys.EIGHT, 21}
        };
    }
}
