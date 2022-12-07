using System;
using System.Collections.Generic;

namespace Aurora.Modules.Razer
{
    public readonly struct RzSdkVersion : IComparable<RzSdkVersion>
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Revision;

        public RzSdkVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public override bool Equals(object obj)
            => obj is RzSdkVersion ver && CompareTo(ver) == 0;

        public override string ToString() => $"{Major}.{Minor}.{Revision}";

        public override int GetHashCode()
        {
            var hashCode = -327234472;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            return hashCode;
        }

        public int CompareTo(RzSdkVersion other)
        {
            var comparer = Comparer<int>.Default;

            int result;
            if ((result = comparer.Compare(Major, other.Major)) == 0)
                if ((result = comparer.Compare(Minor, other.Minor)) == 0)
                    return comparer.Compare(Revision, other.Revision);

            return result;
        }

        public static bool operator ==(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) == 0;
        public static bool operator !=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) != 0;
        public static bool operator >(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) > 0;
        public static bool operator <(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) < 0;
        public static bool operator >=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) >= 0;
        public static bool operator <=(RzSdkVersion first, RzSdkVersion second) => first.CompareTo(second) <= 0;
    }
}
