using Aurora.Settings;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for LayerEditor.xaml
    /// </summary>
    public partial class LayerEditor : UserControl
    {
        //static FreeFormObject activeLayer = new FreeFormObject();

        private static Canvas static_canvas = new Canvas();
        private static Style style = new Style();

        //public static event EventHandler SequenceUpdated;

        public LayerEditor()
        {
            InitializeComponent();

            static_canvas = editor_canvas;
            style = this.FindResource("DesignerItemStyle") as Style;
        }

        private static void ClearElements()
        {
            static_canvas.Children.Clear();
        }

        public static void ClearLayers()
        {
            ClearElements();
        }

        public static void AddKeySequenceElement(FreeFormObject element, Color element_color, String element_name)
        {
            ContentControl existingControl = FindElementByTag(element);

            if (existingControl == null)
            {
                ContentControl newcontrol = new ContentControl();
                newcontrol.Width = element.Width;
                newcontrol.Height = element.Height;
                newcontrol.SetValue(Selector.IsSelectedProperty, true);
                newcontrol.SetValue(Canvas.TopProperty, (double)(element.Y + Effects.grid_baseline_y));
                newcontrol.SetValue(Canvas.LeftProperty, (double)(element.X + Effects.grid_baseline_x));
                RotateTransform transform = new RotateTransform();
                transform.Angle = element.Angle;
                newcontrol.SetValue(RenderTransformProperty, transform);
                newcontrol.Style = style;
                newcontrol.Tag = element;
                newcontrol.SizeChanged += Newcontrol_SizeChanged;
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    Canvas.LeftProperty, typeof(ContentControl)
                  );
                descriptor.AddValueChanged(newcontrol, OnCanvasLeftChanged);
                var descriptor_top = DependencyPropertyDescriptor.FromProperty(
                    Canvas.TopProperty, typeof(ContentControl)
                  );
                descriptor_top.AddValueChanged(newcontrol, OnCanvasTopChanged);
                var descriptor_angle = DependencyPropertyDescriptor.FromProperty(
                    RenderTransformProperty, typeof(ContentControl)
                  );
                descriptor_angle.AddValueChanged(newcontrol, OnAngleChanged);

                Shape content = new Rectangle();

                content = new Rectangle();
                content.Fill = new SolidColorBrush(element_color);

                content.IsHitTestVisible = false;
                content.Opacity = 0.50D;

                Grid content_grid = new Grid();
                content_grid.IsHitTestVisible = false;
                content_grid.Children.Add(content);

                Label label = new Label();
                label.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                label.Content = element_name;
                label.IsHitTestVisible = false;
                content_grid.Children.Add(label);

                newcontrol.Content = content_grid;

                static_canvas.Children.Add(newcontrol);
            }
        }

        public static void RemoveKeySequenceElement(FreeFormObject element)
        {
            ContentControl existingControl = FindElementByTag(element);

            if (existingControl != null)
            {
                static_canvas.Children.Remove(existingControl);
            }
        }

        private static ContentControl FindElementByTag(FreeFormObject tag)
        {
            ContentControl foundElement = null;

            foreach (var child in static_canvas.Children)
            {
                if (child is ContentControl && ReferenceEquals(tag, (child as ContentControl).Tag))
                {
                    foundElement = child as ContentControl;
                    break;
                }
            }

            return foundElement;
        }

        private static void OnAngleChanged(object sender, EventArgs e)
        {
            RotateTransform item = (sender as ContentControl).GetValue(RenderTransformProperty) as RotateTransform;

            if ((sender as ContentControl).Tag != null && (sender as ContentControl).Tag is FreeFormObject)
            {
                ((sender as ContentControl).Tag as FreeFormObject).Angle = (float)item.Angle;
            }
        }

        private static void OnCanvasTopChanged(object sender, EventArgs e)
        {
            object item = (sender as ContentControl).GetValue(Canvas.TopProperty);

            if ((sender as ContentControl).Tag != null && (sender as ContentControl).Tag is FreeFormObject)
            {
                ((sender as ContentControl).Tag as FreeFormObject).Y = float.Parse(item.ToString()) - Effects.grid_baseline_y;
            }
        }

        private static void OnCanvasLeftChanged(object sender, EventArgs e)
        {
            object item = (sender as ContentControl).GetValue(Canvas.LeftProperty);

            if ((sender as ContentControl).Tag != null && (sender as ContentControl).Tag is FreeFormObject)
            {
                ((sender as ContentControl).Tag as FreeFormObject).X = float.Parse(item.ToString()) - Effects.grid_baseline_x;
            }
        }

        private static void Newcontrol_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((sender as ContentControl).Tag != null && (sender as ContentControl).Tag is FreeFormObject)
            {
                ((sender as ContentControl).Tag as FreeFormObject).Width = (float)((sender as ContentControl).ActualWidth);
                ((sender as ContentControl).Tag as FreeFormObject).Height = (float)((sender as ContentControl).ActualHeight);
            }
        }
    }
}
