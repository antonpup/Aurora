using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aurora.Settings;

public class Keybind
{
    [Newtonsoft.Json.JsonProperty("_AssignedKeys")]
    private Queue<Keys> _assignedKeys = new();

    public Keybind()
    {
    }

    public Keybind(IEnumerable<Keys> keys)
    {
        _assignedKeys = new Queue<Keys>(keys);
    }

    public void SetKeys(IEnumerable<Keys> keys)
    {
        _assignedKeys = new Queue<Keys>(keys);
    }

    private bool IsEmpty()
    {
        return _assignedKeys.Count == 0;
    }

    public bool IsPressed()
    {
        var pressedKeys = Global.InputEvents.PressedKeys;

        if (pressedKeys.Count <= 0 || pressedKeys.Count != _assignedKeys.Count) return false;
        return !_assignedKeys.Where((_, i) => pressedKeys[i] != _assignedKeys.ElementAt(i)).Any();
    }

    public Keys[] ToArray()
    {
        return _assignedKeys.ToArray();
    }

    public override string ToString()
    {
        if (IsEmpty())
            return "[EMPTY]";

        var sb = new StringBuilder();
        var keysCopy = new Queue<Keys>(_assignedKeys);

        while (keysCopy.Count > 0)
        {
            var key = keysCopy.Dequeue();

            sb.Append(key.ToString());

            if (keysCopy.Count > 0)
                sb.Append(" + ");
        }

        return sb.ToString();
    }

    public Keybind Clone()
    {
        return new Keybind(_assignedKeys.ToArray());
    }
}