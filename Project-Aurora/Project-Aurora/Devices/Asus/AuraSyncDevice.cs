using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    public class AuraSyncDevice
    {
        public string Name => device.Name;
        public long LastUpdateMillis { get; private set; }
        public bool Active { get; private set; }
        public AsusHandler.AsusDeviceType DeviceType => (AsusHandler.AsusDeviceType)device.Type;
        
        private readonly IAuraSyncDevice device;
        private readonly ConcurrentQueue<Dictionary<DeviceKeys, Color>> colorQueue = new ConcurrentQueue<Dictionary<DeviceKeys, Color>>();
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly int frameRateMillis;
        private const DeviceKeys DefaultKey = DeviceKeys.Peripheral_Logo;
        private Stopwatch stopwatch = new Stopwatch();

        public AuraSyncDevice(IAuraSyncDevice device, int frameRate = 30)
        {
            this.device = device;
            frameRateMillis = (int)((1f/frameRate) * 1000f);
        }

        public void UpdateColors(Dictionary<DeviceKeys, Color> colors)
        {
            //empty queue
            while (!colorQueue.IsEmpty)
                colorQueue.TryDequeue(out _);
            
            // queue a clone of the colors
            colorQueue.Enqueue(new Dictionary<DeviceKeys, Color>(colors));
        }

        public void Start()
        {
            if (!tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
                
            tokenSource = new CancellationTokenSource();
            Thread(tokenSource.Token);
            Active = true;
        }

        public void Stop()
        {
            tokenSource.Cancel();
            Active = false;
        }

        private async void Thread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(frameRateMillis, token);
                // wait for the next tick before drawing
                var colors = GetLatestColors();
                if (colors == null) continue;
                
                stopwatch.Restart();
                ApplyColors(colors);
                LastUpdateMillis = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Try to apply the aurora color collection to this device
        /// </summary>
        /// <param name="colors">The colors to apply</param>
        protected virtual void ApplyColors(Dictionary<DeviceKeys, Color> colors)
        {
            // simple implementation is to assign all colors to DefaultKey
            var color = colors[DefaultKey];
            foreach (IAuraRgbLight light in device.Lights)
                SetRgbLight(light, color);
            
            device.Apply();
        }

        //0x00BBGGRR
        protected void SetRgbLight(IAuraRgbKey rgbLight, Color color)
        {
            rgbLight.Color = (uint)(color.R | color.G << 8 | color.B << 16); 
        }
        
        //0x00BBGGRR
        protected void SetRgbLight(IAuraRgbLight rgbLight, Color color)
        {
            rgbLight.Color = (uint)(color.R | color.G << 8 | color.B << 16); 
        }

        private Dictionary<DeviceKeys, Color> GetLatestColors()
        {
            Dictionary<DeviceKeys, Color> colors = null;
            while (colorQueue.Count > 0)
                colorQueue.TryDequeue(out colors);

            return colors;
        }
        
        protected void Log(string text)
        {
            Global.logger.Info($"[ASUS] [{device.Name}] {text}");
        }
    }
}