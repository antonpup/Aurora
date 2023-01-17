using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Aurora.Devices;
using Aurora.Utils;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings;

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
public sealed class KeySequence : ICloneable, IEquatable<KeySequence>
{
    [JsonIgnore] private readonly ObservableCollection<DeviceKeys> _keys;

    /// <summary>
    /// An array of DeviceKeys keys to be used with KeySequenceType.Sequence type.
    /// </summary>
    [JsonConverter(typeof(ConcurrentListJsonConverterAdapter<DeviceKeys>))]
    [JsonProperty("keys")]
    public ObservableCollection<DeviceKeys> Keys
    {
        get => _keys;
        set
        {
            if (ReferenceEquals(_keys, value))
            {
                return;
            }

            _keys.Clear();
            _keys.AddRange(value);
        }
    }

    /// <summary>
    /// The type of this KeySequence instance.
    /// </summary>
    [JsonProperty("type")]
    public KeySequenceType Type { get; set; }

    /// <summary>
    /// The Freeform object to be used with KeySequenceType.FreeForm type
    /// </summary>
    [JsonProperty("freeform")]
    public FreeFormObject Freeform { get; set; }

    public static KeySequence Empty { get; } = new();

    public KeySequence()
    {
        _keys = new ObservableCollection<DeviceKeys>();
        Type = KeySequenceType.Sequence;
        Freeform = new FreeFormObject();
    }

    public KeySequence(KeySequence other)
    {
        _keys = new ObservableCollection<DeviceKeys>();
        Type = other.Type;
        Freeform = other.Freeform;
    }

    public KeySequence(FreeFormObject freeform)
    {
        _keys = new ObservableCollection<DeviceKeys>();
        Type = KeySequenceType.FreeForm;
        Freeform = freeform;
    }

    public KeySequence(IEnumerable<DeviceKeys> keys)
    {
        _keys = new ObservableCollection<DeviceKeys>(keys);
        Type = KeySequenceType.Sequence;
        Freeform = new FreeFormObject();
    }

    public RectangleF GetAffectedRegion()
    {
        switch (Type)
        {
            case KeySequenceType.FreeForm:
                return new RectangleF((Freeform.X + Effects.grid_baseline_x) * Effects.EditorToCanvasWidth,
                    (Freeform.Y + Effects.grid_baseline_y) * Effects.EditorToCanvasHeight,
                    Freeform.Width * Effects.EditorToCanvasWidth, Freeform.Height * Effects.EditorToCanvasHeight);
            default:

                var left = 0.0f;
                var top = left;
                var right = top;
                var bottom = right;

                foreach (var key in Keys)
                {
                    BitmapRectangle keyMapping = Effects.GetBitmappingFromDeviceKey(key);
                    if (keyMapping.Left < left)
                        left = keyMapping.Left;
                    if (keyMapping.Top < top)
                        top = keyMapping.Top;
                    if (keyMapping.Right > right)
                        right = keyMapping.Right;
                    if (keyMapping.Bottom > bottom)
                        bottom = keyMapping.Bottom;
                }

                return new RectangleF(left, top, right - left, bottom - top);
        }
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((KeySequence)obj);
    }

    public bool Equals(KeySequence p)
    {
        if (ReferenceEquals(null, p)) return false;
        if (ReferenceEquals(this, p)) return true;

        return Keys.Equals(p.Keys) &&
               Type == p.Type &&
               Freeform.Equals(p.Freeform);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Keys.GetHashCode();
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Freeform.GetHashCode();
            return hash;
        }
    }

    public object Clone()
    {
        return new KeySequence(this);
    }
}