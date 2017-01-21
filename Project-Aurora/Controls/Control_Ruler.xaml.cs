using System;
using System.Collections.Generic;
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
        private double _calculatedWidth = 0.0;
        private double _sizeOfOneMark = 50.0;

        public Control_Ruler()
        {
            InitializeComponent();

            GenerateRulerMarks(ActualWidth);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GenerateRulerMarks(e.NewSize.Width);
        }

        private void GenerateRulerMarks(double width)
        {
            if (_calculatedWidth < width)
            {
                int _existingMarks = (int)(_calculatedWidth / _sizeOfOneMark);
                int _newSizeMarks = (int)(width / _sizeOfOneMark);

                //if (_newSizeMarks > _existingMarks)
                //{
                    for (int tickMark = _existingMarks; tickMark < _newSizeMarks; tickMark++)
                    {
                        //Don't render the 0th tick mark. It's redundant.
                        if (tickMark > 0)
                        {
                            gridRulerSpace.Children.Add(
                                new Rectangle()
                                {
                                    Margin = new Thickness(_sizeOfOneMark * (double)tickMark, 0, 0, 0),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    Width = 1,
                                    MaxWidth = 1,
                                    MinWidth = 1,
                                    Fill = new SolidColorBrush(Color.FromRgb(125, 125, 125))
                                }
                            );
                        }
                    }
                //}

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
