using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Settings
{
    public class Keybind
    {
        [Newtonsoft.Json.JsonProperty]
        private Queue<Keys> _AssignedKeys = new Queue<Keys>();

        public Keybind()
        {
        }

        public Keybind(Keys[] keys)
        {
            _AssignedKeys = new Queue<Keys>(keys);
        }

        public Keybind SetKeys(Keys[] keys)
        {
            _AssignedKeys = new Queue<Keys>(keys);

            return this;
        }

        public bool IsEmpty()
        {
            return _AssignedKeys.Count == 0;
        }

        public bool IsPressed()
        {
            Keys[] PressedKeys = Global.InputEvents.PressedKeys;

            if (PressedKeys.Length > 0 && PressedKeys.Length == _AssignedKeys.Count)
            {
                for(int i = 0; i < _AssignedKeys.Count; i++)
                {
                    if(PressedKeys[i] != _AssignedKeys.ElementAt(i))
                        return false;
                }

                return true;
            }

            return false;
        }

        public Keys[] ToArray()
        {
            return _AssignedKeys.ToArray();
        }

        public override string ToString()
        {
            if (IsEmpty())
                return "[EMPTY]";

            StringBuilder _sb = new StringBuilder();

            Queue<Keys> _KeysCopy = new Queue<Keys>(_AssignedKeys);

            while (_KeysCopy.Count > 0)
            {
                Keys key = _KeysCopy.Dequeue();

                _sb.Append(key.ToString());

                if (_KeysCopy.Count > 0)
                    _sb.Append(" + ");
            }

            return _sb.ToString();
        }

        public Keybind Clone()
        {
            return new Keybind(_AssignedKeys.ToArray());
        }
    }
}
