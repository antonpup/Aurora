using System.Windows.Media;
using System.Windows;

namespace ColorBox
{
    public class BrushChangedEventArgs : RoutedEventArgs
    {
        public BrushChangedEventArgs(RoutedEvent routedEvent, Brush brush)
        {
            this.RoutedEvent = routedEvent;
            this.Brush = brush;
        }

        private Brush _Brush;
        public Brush Brush
        {
            get { return _Brush; }
            set { _Brush = value; }
        }
    }
}

