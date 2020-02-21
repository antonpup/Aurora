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
        private readonly AsusHandler asusHandler;
        private readonly ConcurrentQueue<Dictionary<DeviceKeys, Color>> colorQueue = new ConcurrentQueue<Dictionary<DeviceKeys, Color>>();
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly int frameRateMillis;
        private const DeviceKeys DefaultKey = DeviceKeys.Peripheral_Logo;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private const int DiscountLimit = 2500;
        private const int DiscountTries = 3;
        private int disconnectCounter = 0;
        
        
        public AuraSyncDevice(AsusHandler asusHandler, IAuraSyncDevice device, int frameRate = 30)
        {
            this.asusHandler = asusHandler;
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
            // create a scary parallel thread
            var parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = tokenSource.Token;
            Parallel.Invoke(parallelOptions, () => Thread(tokenSource.Token));
            Active = true;
        }

        public void Stop()
        {
            tokenSource.Cancel();
            Active = false;
        }

        private async void Thread(CancellationToken token)
        {
            // continue until our token has been cancelled
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(frameRateMillis, token);
                    // wait for the next tick before drawing
                    var colors = GetLatestColors();
                    if (colors == null) continue;

                    stopwatch.Restart();
                    ApplyColors(colors);
                    LastUpdateMillis = stopwatch.ElapsedMilliseconds;
                    stopwatch.Stop();
                    
                    // If the device did not take long to update, continue
                    if (stopwatch.ElapsedMilliseconds < DiscountLimit)
                    {
                        disconnectCounter = 0;
                        continue;
                    }
                    
                    Log($"Device {Name} took too long to update {stopwatch.ElapsedMilliseconds}ms");
                    // penalize the device if it took too long to update
                    disconnectCounter++;
                    
                    // disconnect device if it takes too long to update
                    if (disconnectCounter < DiscountTries)
                        continue;
                    
                    asusHandler.DisconnectDevice(this);
                    return;
                }
                catch(TaskCanceledException)
                {
                    asusHandler.DisconnectDevice(this);
                    return;
                }
                catch(Exception exception)
                {
                    Log($"ERROR {exception}");
                    asusHandler.DisconnectDevice(this);
                    return;
                }
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