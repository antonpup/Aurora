using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aurora.Utils {

    public class AudioDevice : IDisposable {

        private readonly DataFlow flow;
        private string deviceName;
        private WasapiLoopbackCapture waveIn;
        private EventHandler<WaveInEventArgs> waveInDataAvailable; // Store the current event listeners so when device changes, we can re-attach listeners

        public AudioDevice(DataFlow flow) {
            this.flow = flow;
        }

        public event EventHandler<WaveInEventArgs> WaveInDataAvailable {
            add {
                waveInDataAvailable += value; // Update stored event listeners
                if (waveIn != null) waveIn.DataAvailable += value; // If WaveIn is valid, pass the event handler on
            }
            remove {
                waveInDataAvailable -= value; // Update stored event listeners
                if (waveIn != null) waveIn.DataAvailable -= value; // If WaveIn is valid, pass the event handler on
            }
        }

        public string DeviceName {
            get => deviceName;
            set {
                if (deviceName == value) return; // Do not re-create the MMDevice is it's not changed
                deviceName = value;
                SetupDevice();
            }
        }

        public MMDevice Device { get; private set; }

        private void SetupDevice() {
            DisposeDeviceAndWaveIn();

            // Re-create device
            Device = string.IsNullOrEmpty(deviceName) || deviceName == AudioUtils.DEVICE_DEFAULT
                ? AudioUtils.DeviceEnumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia) // If string is "", use default device
                : AudioUtils.DeviceEnumerator.EnumerateAudioEndPoints(flow, DeviceState.Active).FirstOrDefault(d => d.DeviceFriendlyName == deviceName); // Else find device with this name

            // If found device, create a WaveIn for it
            if (Device != null) {
                waveIn = new WasapiLoopbackCapture(Device);
                if (waveInDataAvailable != null)
                    waveIn.DataAvailable += waveInDataAvailable;
                waveIn.StartRecording();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing)
                    DisposeDeviceAndWaveIn();
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        private void DisposeDeviceAndWaveIn() {
            Device?.Dispose();
            Device = null;
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;
        }
        #endregion
    }

    public static class AudioUtils {

        internal const string DEVICE_DEFAULT = "Default";

        public static MMDeviceEnumerator DeviceEnumerator { get; } = new MMDeviceEnumerator();

        public static ObservableCollection<string> PlaybackDevices { get; } = new ObservableCollection<string>(DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(x => x.DeviceFriendlyName));
        public static ObservableCollection<string> RecordingDevices { get; } = new ObservableCollection<string>(DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => x.DeviceFriendlyName));
    }
}
