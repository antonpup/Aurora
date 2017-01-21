using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_Ruler.xaml
    /// </summary>
    public partial class Control_Ruler : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty MarkSizeProperty = DependencyProperty.Register("MarkSize", typeof(double), typeof(Control_Ruler));

        public double MarkSize
        {
            get
            {
                return (double)GetValue(MarkSizeProperty);
            }
            set
            {
                SetValue(MarkSizeProperty, value);

                GenerateRulerMarks(ActualWidth, true);
            }
        }

        private double _calculatedWidth = 0.0;

        public Control_Ruler()
        {
            InitializeComponent();

            SetValue(MarkSizeProperty, 50.0);

            GenerateRulerMarks(ActualWidth);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GenerateRulerMarks(e.NewSize.Width);
        }

        private void GenerateRulerMarks(double width, bool forceRedraw = false)
        {
            if (_calculatedWidth < width)
            {
                int _existingMarks = (int)(_calculatedWidth / MarkSize);
                int _newSizeMarks = (int)(width / MarkSize);

                if (_newSizeMarks < _existingMarks || forceRedraw)
                {
                    gridRulerSpace.Children.Clear();
                    _existingMarks = 0;
                }

                for (int tickMark = _existingMarks; tickMark <= _newSizeMarks; tickMark++)
                {
                    gridRulerSpace.Children.Add(
                        new Rectangle()
                        {
                            Margin = new Thickness(MarkSize * (double)tickMark, 0, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Width = 1,
                            MaxWidth = 1,
                            MinWidth = 1,
                            Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125))
                        }
                    );
                }

                _calculatedWidth = width;

                gridRulerSpace.Width = width;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateRulerMarks(ActualWidth);
        }
    }
}
