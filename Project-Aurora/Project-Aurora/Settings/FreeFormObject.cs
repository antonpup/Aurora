namespace Aurora.Settings
{
    /// <summary>
    /// The type of the FreeForm region.
    /// </summary>
    public enum FreeFormType
    {
        Line,
        Rectangle,
        Circle,
        RectangleFilled,
        CircleFilled
    }

    /// <summary>
    /// A delegate for a changed value
    /// </summary>
    /// <param name="newobject">The current instance of FreeFormObject</param>
    public delegate void ValuesChangedEventHandler(FreeFormObject newobject);

    /// <summary>
    /// A class representing a region within a bitmap.
    /// </summary>
    public class FreeFormObject
    {
        FreeFormType _type;
        /// <summary>
        /// Get/Set the type of this region.
        /// </summary>
        public FreeFormType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _x;
        /// <summary>
        /// Get/Set the X coordinate for this region.
        /// </summary>
        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _y;
        /// <summary>
        /// Get/Set the Y coordinate for this region.
        /// </summary>
        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _width;
        /// <summary>
        /// Get/Set the Width of this region.
        /// </summary>
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _height;
        /// <summary>
        /// Get/Set the Height of this region.
        /// </summary>
        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _angle;
        /// <summary>
        /// Get/Set the rotation angle of this region.
        /// </summary>
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                ValuesChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Event for when any value of this FreeFormObject changes.
        /// </summary>
        public event ValuesChangedEventHandler ValuesChanged;

        /// <summary>
        /// Creates a default instance of the FreeFormObject
        /// </summary>
        public FreeFormObject()
        {
            _type = FreeFormType.Rectangle;
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
            _type = FreeFormType.Rectangle;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _angle = angle;
        }

        /// <summary>
        /// An equals function, compares this instance of FreeFormObject to another object and returns whether or not they are equal.
        /// </summary>
        /// <param name="obj">An object to be compared</param>
        /// <returns>A boolean value representing equality</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FreeFormObject)obj);
        }

        /// <summary>
        /// An equals function, compares this instance of FreeFormObject to another instance of FreeFormObject and returns whether or not they are equal.
        /// </summary>
        /// <param name="p">An instance of FreeFormObject to be compared</param>
        /// <returns>A boolean value representing equality</returns>
        public bool Equals(FreeFormObject p)
        {
            return _type == p._type &&
                _x == p._x &&
                _y == p._y &&
                _width == p._width &&
                _height == p._height &&
                _angle == p._angle;
        }

        /// <summary>
        /// Generates a hash code representing this FreeFormObject
        /// </summary>
        /// <returns>A hashcode unique to this FreeFormObject</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _type.GetHashCode();
                hash = hash * 23 + _x.GetHashCode();
                hash = hash * 23 + _y.GetHashCode();
                hash = hash * 23 + _width.GetHashCode();
                hash = hash * 23 + _height.GetHashCode();
                hash = hash * 23 + _angle.GetHashCode();
                return hash;
            }
        }
    }
}
