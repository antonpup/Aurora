using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class KeyRecorder
    {
        private String recordingType = "";
        private bool isSingleKey = false;
        private List<DeviceKeys> recordedKeys = new List<DeviceKeys>();
        public delegate void RecordingFinishedHandler(DeviceKeys[] resulting_keys);
        public event RecordingFinishedHandler FinishedRecording;

        public KeyRecorder()
        {
            Reset();
        }

        public void AddKey(DeviceKeys key)
        {
            if (!IsRecording())
                return;

            if (!HasRecorded(key))
            {
                recordedKeys.Add(key);

                if(isSingleKey)
                {
                    StopRecording();
                }
            }
        }

        public void RemoveKey(DeviceKeys key)
        {
            if (!IsRecording())
                return;

            if (HasRecorded(key))
            {
                recordedKeys.Remove(key);
            }
        }

        public bool HasRecorded(DeviceKeys key)
        {
            return recordedKeys.Contains(key);
        }

        public DeviceKeys[] GetKeys()
        {
            return recordedKeys.ToArray();
        }

        public void StartRecording(String type, bool isSingleKey = false)
        {
            Reset();

            recordingType = type;
            this.isSingleKey = isSingleKey;
        }

        public void StopRecording()
        {
            recordingType = "";
            isSingleKey = false;

            if (FinishedRecording != null)
            {
                FinishedRecording(GetKeys());
            }
        }

        public bool IsRecording(String type = "")
        {
            if(String.IsNullOrWhiteSpace(type))
            {
                return !String.IsNullOrWhiteSpace(recordingType);
            }
            else
            {
                return recordingType.Equals(type);
            }
        }

        public string GetRecordingType()
        {
            return recordingType;
        }

        public void Reset()
        {
            recordingType = "";
            isSingleKey = false;
            recordedKeys = new List<DeviceKeys>();
        }

        public bool IsSingleKey()
        {
            return isSingleKey;
        }

    }
}
