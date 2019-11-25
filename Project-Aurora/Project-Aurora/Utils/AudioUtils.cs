using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aurora.Utils {

    /// <summary>
    /// Class for dealing with the NAudio audio devices.
    /// </summary>
    /// <remarks>
    /// Allows for finding a device with the given name and attaching recording events. If the device name is changed, the events are transferred onto the new device.
    /// Additionally keeps track of whether the target device's audio waveform should be recorded or not.
    /// </remarks>
    public class AudioDevice : IDisposable {

        private readonly DataFlow flow;
        private bool enableRecording = true;
        private string deviceName;
        private WasapiLoopbackCapture waveIn;
        private EventHandler<WaveInEventArgs> waveInDataAvailable; // Store the current event listeners so when device changes, we can re-attach listeners


        public AudioDevice(DataFlow flow) {
            this.flow = flow;
        }

        /// <summary>An event that is invoked when the wave recorder has data available.</summary>
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

        /// <summary>Gets or sets the name of the audio device targetted by this instance.
        /// Setting the value attempts to find the target device with the given name.</summary>
        public string DeviceName {
            get => deviceName;
            set {
                if (deviceName == value) return; // Do not re-create the MMDevice is it's not changed
                deviceName = value;
                SetupDevice();
            }
        }

        /// <summary>Gets or sets whether the recording on the waveform is running.</summary>
        public bool EnableRecording {
            get => enableRecording;
            set {
                if (enableRecording && !value) // If we are currently recording, but we shouldn't, stop
                    waveIn?.StopRecording();
                else if (!enableRecording && value) // If we're not currently recording but we should, start
                    waveIn?.StartRecording();
                enableRecording = value;
            }
        }

        /// <summary>Gets the current multimedia device that is being used by this instance.
        /// May be null if no device was found with the given <see cref="DeviceName"/>.</summary>
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
                
                // If there are listeners that are currently registered, add them to the new WaveIn
                if (waveInDataAvailable != null)
                    waveIn.DataAvailable += waveInDataAvailable;

                // If recording is enabled, start it
                if (enableRecording)
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
