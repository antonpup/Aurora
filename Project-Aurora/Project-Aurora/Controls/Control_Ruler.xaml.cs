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

                GenerateRulerMarks(true);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty DisplayNumberCountProperty = DependencyProperty.Register("DisplayNumberCount", typeof(bool), typeof(Control_Ruler));

        public bool DisplayNumberCount
        {
            get
            {
                return (bool)GetValue(DisplayNumberCountProperty);
            }
            set
            {
                SetValue(DisplayNumberCountProperty, value);

                GenerateRulerMarks(true);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty NumberCountSuffixProperty = DependencyProperty.Register("NumberCountSuffix", typeof(string), typeof(Control_Ruler));

        public string NumberCountSuffix
        {
            get
            {
                return (string)GetValue(NumberCountSuffixProperty);
            }
            set
            {
                SetValue(NumberCountSuffixProperty, value);

                GenerateRulerMarks(true);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty IsVerticalProperty = DependencyProperty.Register("IsVertical", typeof(bool), typeof(Control_Ruler));

        public bool IsVertical
        {
            get
            {
                return (bool)GetValue(IsVerticalProperty);
            }
            set
            {
                SetValue(IsVerticalProperty, value);

                GenerateRulerMarks(true);
            }
        }

        private double _calculatedWidth = 0.0;

        public Control_Ruler()
        {
            InitializeComponent();

            SetValue(MarkSizeProperty, 50.0);
            SetValue(IsVerticalProperty, false);

            GenerateRulerMarks();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GenerateRulerMarks();
        }

        private void GenerateRulerMarks(bool forceRedraw = false)
        {
            if(forceRedraw)
                _calculatedWidth = 0.0;

            double width = ActualWidth;

            if (IsVertical)
                width = ActualHeight;

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
                        (IsVertical ?
                            new Rectangle()
                            {
                                Margin = new Thickness(0, MarkSize * (double)tickMark, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Top,
                                Height = 1,
                                MaxHeight = 1,
                                MinHeight = 1,
                                Fill = Foreground
                            }
                        :
                            new Rectangle()
                            {
                                Margin = new Thickness(MarkSize * (double)tickMark, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Width = 1,
                                MaxWidth = 1,
                                MinWidth = 1,
                                Fill = Foreground
                            }
                        )
                    );

                    if (DisplayNumberCount && tickMark > 0)
                    {
                        gridRulerSpace.Children.Add(
                        new TextBlock()
                        {
                            Margin = (IsVertical ? new Thickness(0, (MarkSize * (double)tickMark) + 2, 0, 0) : new Thickness((MarkSize * (double)tickMark) + 2, 0, 0, 0)),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Text = $"{tickMark} {(string.IsNullOrWhiteSpace(NumberCountSuffix) ? "" : NumberCountSuffix.TrimStart(' '))}",
                            Foreground = Foreground
                        }
                    );
                    }
                }

                _calculatedWidth = width;

                gridRulerSpace.Width = width;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateRulerMarks();
        }
    }
}
