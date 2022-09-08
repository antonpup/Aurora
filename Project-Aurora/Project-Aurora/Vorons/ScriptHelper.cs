
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings;

namespace Aurora.Scripts.VoronScripts
{
    public static class ScriptHelper
    {
        
        public static void RegProp(this VariableRegistry registry,
            string name, object defaultValue, string remark = "", object min = null, object max = null)
        {
            registry.Register(name, defaultValue, name, max, min, remark);
        }

        public static string SpectrumToString(ColorSpectrum spectrum)
        {
            return string.Join(" | ",
                spectrum.GetSpectrumColors().Select(x => string.Format("#{0:X2}{1:X2}{2:X2}{3:X2} @ {4}", x.Value.A, x.Value.R,
                    x.Value.G, x.Value.B, x.Key)));
        }

        public static ColorSpectrum StringToSpectrum(string text)
        {
            var colors = text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().Split('@').Select(x2 => x2.Trim()).ToArray())
                .ToArray();
            var spectrum = new ColorSpectrum();
            foreach (var colorset in colors.Select((x, i) => new
                     {
                         Color = ColorTranslator.FromHtml(x[0]),
                         Position = x.Length > 1 ? float.Parse(x[1]) : (1f / (colors.Length - 1) * i)
                     }))
            {
                spectrum.SetColorAt(colorset.Position, colorset.Color);
            }
            return spectrum;
        }

        public static KeyValuePair<string, ColorSpectrum> UpdateSpectrumProperty(KeyValuePair<string, ColorSpectrum> current,
            string newValue)
        {
            return newValue == current.Key ? current :
                new KeyValuePair<string, ColorSpectrum>(newValue, StringToSpectrum(newValue));
        }
    }
}