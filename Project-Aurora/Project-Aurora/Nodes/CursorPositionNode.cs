using System.Windows.Forms;

namespace Aurora.Nodes;

public class CursorPositionNode : Node
{
    public float X => Cursor.Position.X;
    public float Y => Cursor.Position.Y;
}