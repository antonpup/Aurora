using System;

namespace Aurora.Settings;

public sealed class FreeFormChangedEventArgs : EventArgs
{
    public FreeFormObject FreeForm { get; }

    public FreeFormChangedEventArgs(FreeFormObject freeFormObject)
    {
        FreeForm = freeFormObject;
    }
}
    
/// <summary>
/// A class representing a region within a bitmap.
/// </summary>
public sealed class FreeFormObject : IEquatable<FreeFormObject>
{
    private float _x;
    /// <summary>
    /// Get/Set the X coordinate for this region.
    /// </summary>
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            ValuesChanged?.Invoke(this, new FreeFormChangedEventArgs(this));
        }
    }

    private float _y;
    /// <summary>
    /// Get/Set the Y coordinate for this region.
    /// </summary>
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            ValuesChanged?.Invoke(this, new FreeFormChangedEventArgs(this));
        }
    }

    private float _width;
    /// <summary>
    /// Get/Set the Width of this region.
    /// </summary>
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            ValuesChanged?.Invoke(this, new FreeFormChangedEventArgs(this));
        }
    }

    private float _height;
    /// <summary>
    /// Get/Set the Height of this region.
    /// </summary>
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            ValuesChanged?.Invoke(this, new FreeFormChangedEventArgs(this));
        }
    }

    private float _angle;
    /// <summary>
    /// Get/Set the rotation angle of this region.
    /// </summary>
    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
            ValuesChanged?.Invoke(this, new FreeFormChangedEventArgs(this));
        }
    }

    /// <summary>
    /// Event for when any value of this FreeFormObject changes.
    /// </summary>
    public event EventHandler<FreeFormChangedEventArgs>? ValuesChanged;

    /// <summary>
    /// Creates a default instance of the FreeFormObject
    /// </summary>
    public FreeFormObject()
    {
        _x = 0;
        _y = 0;
        _width = 30;
        _height = 30;
        _angle = 0.0f;
    }

    /// <summary>
    /// Creates an instance of the FreeFormObject with specified parameters.
    /// </summary>
    /// <param name="x">The X coordinate</param>
    /// <param name="y">The Y coordinate</param>
    /// <param name="width">The Width</param>
    /// <param name="height">The Height</param>
    /// <param name="angle">The rotation angle</param>
    public FreeFormObject(float x, float y, float width = 30.0f, float height = 30.0f, float angle = 0.0f)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _angle = angle;
    }

    /// <summary>
    /// An equals function, compares this instance of FreeFormObject to another instance of FreeFormObject and returns whether or not they are equal.
    /// </summary>
    /// <param name="p">An instance of FreeFormObject to be compared</param>
    /// <returns>A boolean value representing equality</returns>
    public bool Equals(FreeFormObject? p)
    {
        return _x == p?._x &&
               _y == p._y &&
               _width == p._width &&
               _height == p._height &&
               _angle == p._angle;
    }
}