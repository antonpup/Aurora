using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    /// <summary>
    /// The type of the KeySequence
    /// </summary>
    public enum KeySequenceType
    {
        /// <summary>
        /// Sequence uses an array of DeviceKeys keys
        /// </summary>
        Sequence,
        /// <summary>
        /// Sequence uses a freeform region
        /// </summary>
        FreeForm
    }

    /// <summary>
    /// A class representing a series of DeviceKeys keys or a freeform region
    /// </summary>
    public class KeySequence
    {
        /// <summary>
        /// An array of DeviceKeys keys to be used with KeySequenceType.Sequence type.
        /// </summary>
        public List<Devices.DeviceKeys> keys;

        /// <summary>
        /// The type of this KeySequence instance.
        /// </summary>
        public KeySequenceType type;

        /// <summary>
        /// The Freeform object to be used with KeySequenceType.FreeForm type
        /// </summary>
        public FreeFormObject freeform;

        public KeySequence()
        {
            keys = new List<Devices.DeviceKeys>();
            type = KeySequenceType.Sequence;
            freeform = new FreeFormObject();
        }

        public KeySequence(KeySequence otherKeysequence)
        {
            this.keys = new List<Devices.DeviceKeys>(otherKeysequence.keys);
            type = otherKeysequence.type;
            this.freeform = otherKeysequence.freeform;
        }

        public KeySequence(FreeFormObject freeform)
        {
            this.keys = new List<Devices.DeviceKeys>();
            type = KeySequenceType.FreeForm;
            this.freeform = freeform;
        }

        public KeySequence(Devices.DeviceKeys[] keys)
        {
            this.keys = new List<Devices.DeviceKeys>(keys);
            type = KeySequenceType.Sequence;
            freeform = new FreeFormObject();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeySequence)obj);
        }

        public bool Equals(KeySequence p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            return (new HashSet<Devices.DeviceKeys>(keys).SetEquals(p.keys)) &&
                type == p.type &&
                freeform.Equals(p.freeform);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + keys.GetHashCode();
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + freeform.GetHashCode();
                return hash;
            }
        }
    }
}
