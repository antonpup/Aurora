using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public enum FreeFormType
    {
        Line,
        Rectangle,
        Circle,
        RectangleFilled,
        CircleFilled
    }

    public delegate void ValuesChangedEventHandler(FreeFormObject newobject);

    public class FreeFormObject
    {
        FreeFormType _type;
        public FreeFormType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        float _x;
        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        float _y;
        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        float _width;
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        float _height;
        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        float _angle;
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                if (ValuesChanged != null) ValuesChanged(this);
            }
        }

        public event ValuesChangedEventHandler ValuesChanged;

        public FreeFormObject()
        {
            _type = FreeFormType.Rectangle;
            _x = 0;
            _y = 0;
            _width = 30;
            _height = 30;
            _angle = 0.0f;
        }

        public FreeFormObject(float x, float y, float width = 30.0f, float height = 30.0f, float angle = 0.0f)
        {
            _type = FreeFormType.Rectangle;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _angle = angle;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FreeFormObject)obj);
        }

        public bool Equals(FreeFormObject p)
        {
            return _type == p._type &&
                _x == p._x &&
                _y == p._y &&
                _width == p._width &&
                _height == p._height &&
                _angle == p._angle;
        }

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
