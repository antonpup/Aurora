using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using System.Timers;

namespace Aurora
{
    public struct Bitmaping
    {
        public int topleft_x, topleft_y, bottomright_x, bottomright_y;

        public Bitmaping(int tlx, int tly, int brx, int bry)
        {
            topleft_x = tlx;
            topleft_y = tly;
            bottomright_x = brx;
            bottomright_y = bry;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bitmaping)obj);
        }

        public bool Equals(Bitmaping p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            return topleft_x == p.topleft_x &&
                    topleft_y == p.topleft_y &&
                    bottomright_x == p.bottomright_x &&
                    bottomright_y == p.bottomright_y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + topleft_x.GetHashCode();
                hash = hash * 23 + topleft_y.GetHashCode();
                hash = hash * 23 + bottomright_x.GetHashCode();
                hash = hash * 23 + bottomright_y.GetHashCode();
                return hash;
            }
        }

        public EffectsEngine.Functions.EffectPoint GetCenter()
        {
            return new EffectsEngine.Functions.EffectPoint(topleft_x + ((bottomright_x - topleft_x) / 2.0f), topleft_y + ((bottomright_y - topleft_y) / 2.0f));
        }
    }

    public delegate void NewLayerRendered(Bitmap bitmap);

    public class Effects
    {
        int filenamecount = 0;
        public bool isrecording = false;
        Bitmap previousframe = null;
        long nextsecond = 0L;
        long currentsecond = 0L;
        int renderedframes = 0;
        long render_time = 0L;
        Timer recordTimer = new Timer(1000D / 15D); // 30fps
        int pushedframes = 0;
        Timer fpsDebugTimer = new Timer(1000D);


        private static object bitmap_lock = new object();

        public event NewLayerRendered NewLayerRender = delegate { };

        public static int canvas_width = 1;
        public static int canvas_height = 1;

        public static float grid_baseline_x = 0.0f;
        public static float grid_baseline_y = 0.0f;
        public static float grid_width = 1.0f;
        public static float grid_height = 1.0f;

        public static float canvas_width_center
        {
            get
            {
                return canvas_width / 2.0f;
            }
        }

        public static float canvas_height_center
        {
            get
            {
                return canvas_height / 2.0f;
            }
        }

        public static float editor_to_canvas_width
        {
            get
            {
                return canvas_width / grid_width;
            }
        }

        public static float editor_to_canvas_height
        {
            get
            {
                return canvas_height / grid_height;
            }
        }

        public static int canvas_biggest
        {
            get
            {
                return Effects.canvas_width > Effects.canvas_height ? Effects.canvas_width : Effects.canvas_height;
            }
        }

        private static Dictionary<Devices.DeviceKeys, Bitmaping> bitmap_map = new Dictionary<Devices.DeviceKeys, Bitmaping>();

        private static Dictionary<Devices.DeviceKeys, Color> keyColors = new Dictionary<Devices.DeviceKeys, Color>();

        public Effects()
        {
            Devices.DeviceKeys[] allKeys = bitmap_map.Keys.ToArray();

            foreach (Devices.DeviceKeys key in allKeys)
            {
                keyColors.Add(key, Color.FromArgb(0, 0, 0));
            }

            recordTimer.Elapsed += RecordTimer_Elapsed;

            fpsDebugTimer.Elapsed += FpsDebugTimer_Elapsed;
        }

        private void FpsDebugTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fps = " + pushedframes);
            pushedframes = 0;
        }

        private void RecordTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (nextsecond == 0L)
                nextsecond = currentsecond + 1000L;

            currentsecond += (long)recordTimer.Interval;

            if (previousframe != null)
            {
                Bitmap tempbmp = new Bitmap(previousframe);

                renderedframes++;
                tempbmp.Save("renders\\" + (filenamecount++) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }

            if (currentsecond >= nextsecond)
            {
                System.Diagnostics.Debug.WriteLine("fps = " + (double)renderedframes / ((double)(currentsecond - (nextsecond - 1000L)) / 1000D));

                nextsecond = currentsecond + 1000L;
                renderedframes = 0;
            }

        }

        public void ToggleRecord()
        {
            isrecording = !isrecording;

            if (isrecording)
                recordTimer.Start();
            else
                recordTimer.Stop();
        }

        public void SetCanvasSize(int width, int height)
        {
            canvas_width = width;
            canvas_height = height;
        }

        public static Bitmaping GetBitmappingFromDeviceKey(DeviceKeys key)
        {
            if (bitmap_map.ContainsKey(key))
                return bitmap_map[key];
            else
                return new Bitmaping(0, 0, 0, 0);
        }

        public void SetBitmapping(Dictionary<DeviceKeys, Bitmaping> bitmap_map)
        {
            Effects.bitmap_map = bitmap_map;
        }

        public void PushFrame(EffectFrame frame)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            lock (bitmap_lock)
            {
                EffectLayer background = new EffectLayer("Global Background", Color.FromArgb(0, 0, 0));

                EffectLayer[] over_layers_array = frame.GetOverlayLayers().ToArray();
                EffectLayer[] layers_array = frame.GetLayers().ToArray();

                foreach (EffectLayer layer in layers_array)
                {
                    background += layer;
                }

                foreach (EffectLayer layer in over_layers_array)
                {
                    background += layer;
                }

                //Apply Brightness
                Color peri_col = background.Get(DeviceKeys.Peripheral);

                background.Set(DeviceKeys.Peripheral, Utils.ColorUtils.BlendColors(peri_col, Color.Black, (1.0f - Global.Configuration.peripheral_brightness_modifier)));
                background.Fill(Color.FromArgb((int)(255.0f * (1.0f - Global.Configuration.keyboard_brightness_modifier)), Color.Black));

                if (Global.Configuration.use_volume_as_brightness)
                    background *= Global.Configuration.global_brightness;

                Dictionary<DeviceKeys, Color> keyColors = new Dictionary<DeviceKeys, Color>();
                Devices.DeviceKeys[] allKeys = bitmap_map.Keys.ToArray();

                foreach (Devices.DeviceKeys key in allKeys)
                {
                    keyColors[key] = background.Get(key);
                }

                Effects.keyColors = new Dictionary<DeviceKeys, Color>(keyColors);

                pushedframes++;
                Global.dev_manager.UpdateDevices(keyColors);

                var hander = NewLayerRender;

                if (hander != null)
                    hander.Invoke(background.GetBitmap());

                if (isrecording)
                {

                    EffectLayer pizelated_render = new EffectLayer();
                    foreach (Devices.DeviceKeys key in allKeys)
                    {
                        pizelated_render.Set(key, background.Get(key));
                    }

                    using (Bitmap map = pizelated_render.GetBitmap())
                    {
                        previousframe = new Bitmap(map);
                    }
                }


                frame.Dispose();
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
        }

        public Dictionary<DeviceKeys, Color> GetKeyboardLights()
        {
            return Effects.keyColors;
        }

        [System.Runtime.InteropServices.DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }
    }
}
