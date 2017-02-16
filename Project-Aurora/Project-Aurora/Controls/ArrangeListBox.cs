using Aurora.Settings.Layers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Aurora.Controls
{
    public class ArrangePanel : Panel
    {
        private UIElement _dragging;
        private Vector _draggingDelta;
        private Point _draggingStart;

        private double[] ChildHeights;

        public ArrangeListBox ParentList { get; internal set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_dragging != null)
                ReorderOthers();
            else
                this.SetBaseOrders();

            ChildHeights = new double[Children.Count];

            double height = 0;
            double max_width = 0;
            int index = 0;
            foreach (UIElement child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                Point pos = new Point(0, height);
                if (child == _dragging)
                {
                    ChildHeights[index] = child.DesiredSize.Height;
                    height += child.DesiredSize.Height;
                    index++;
                    continue;
                }


                child.Measure(availableSize);
                ChildHeights[index] = child.DesiredSize.Height;
                SetDesiredArrangement(child, new Rect(pos, new Size(child.DesiredSize.Width, child.DesiredSize.Height)));


                height += child.DesiredSize.Height;
                if (child.DesiredSize.Height > max_width)
                    max_width = child.DesiredSize.Width;

                index++;
            }

            double final_width = double.IsPositiveInfinity(availableSize.Width) ? max_width : availableSize.Width;
            double final_height = double.IsPositiveInfinity(availableSize.Height) ? height : availableSize.Height;

            return new Size(final_width, final_height);
        }

        

        protected override Size ArrangeOverride(Size finalSize)
        {
            double current_height = 0;
            foreach (UIElement child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                Rect r = GetArrangement(child);
                if (double.IsNaN(r.Top))
                    r = GetDesiredArrangement(child);
                child.Arrange(r);
                //SetArrangement(child, r);
                current_height += child.DesiredSize.Height;
            }

            return finalSize;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            StartDragging(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            StopDragging();
        }

        /*protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DoReordering(e);
            }
        }*/

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (e.OriginalSource is UserControl)
                StopDragging();
            base.OnMouseLeave(e);
        }

        private void StartDragging(MouseEventArgs e)
        {
            _dragging = GetLocalUIElement((UIElement)e.OriginalSource);
            _dragging.SetValue(ZIndexProperty, 100);

            _draggingStart = e.GetPosition(this);

            Rect position = GetArrangement(_dragging);
            _draggingDelta = position.TopLeft - _draggingStart;
            _dragging.BeginAnimation(ArrangementProperty, null);

            SetArrangement(_dragging, position);
            InvalidateArrange();
        }

        public void UpdateOrder(MouseEventArgs e)
        {
            if (_dragging == null)
                return;
            e.Handled = true;
            Point mousePosition = e.GetPosition(this);
            int index = 0;
            double total_height = 0;
            if (mousePosition.Y > 0)
            {
                foreach (double height in ChildHeights)
                {
                    if (mousePosition.Y > total_height && mousePosition.Y <= (total_height += height))
                        break;

                    index++;
                }

                index = Math.Min(index, Children.Count - 1);
            }
            //Global.logger.LogLine("Setting order to " + index);
            bool order_Changed;
            if (order_Changed = index != GetOrder(_dragging))
                SetOrder(_dragging, index);
            //ReorderOthers();
            var topLeft = mousePosition + _draggingDelta;
            topLeft.X = 0;
            var newPosition = new Rect(topLeft, GetArrangement(_dragging).Size);
            SetArrangement(_dragging, newPosition);
            if (order_Changed)
                InvalidateMeasure();
            else
                InvalidateArrange();
        }

        public void StopDragging()
        {
            if (_dragging == null) return;

            _dragging.ClearValue(ZIndexProperty);

            InvalidateMeasure();
            Rect r = GetDesiredArrangement(_dragging);
            AnimateToPosition(_dragging, r);

            /*DependencyObject dep_obj = this.TemplatedParent;
            while ((dep_obj = VisualTreeHelper.GetParent(dep_obj)) != null && !(dep_obj is ListBox))
            {
                continue;
            }*/

            
            if (this.ParentList != null)
            {
                if (this.ParentList.ItemsSource is System.Collections.Specialized.INotifyCollectionChanged)
                {

                    dynamic items_lst = this.ParentList.ItemsSource;
                    int old_index = items_lst.IndexOf(((FrameworkElement)_dragging).DataContext as Layer);
                    int new_index = GetOrder(_dragging);
                    if (old_index != new_index)
                        items_lst.Move(old_index, new_index);
                }

            }

            _dragging = null;
        }

        

        private void AnimateToPosition(DependencyObject d, Rect desiredPosition)
        {
            Rect position = GetArrangement(d);
            if (double.IsNaN(position.X))
            {
                //Global.logger.LogLine("No anim for me");
                SetArrangement(d, desiredPosition);
                InvalidateArrange();
                return;
            }

            double distance = Math.Max(
                (desiredPosition.TopLeft - position.TopLeft).Length,
                (desiredPosition.BottomRight - position.BottomRight).Length);

            TimeSpan animationTime = TimeSpan.FromMilliseconds(distance * 2);
            ((UIElement)d).BeginAnimation(ArrangementProperty, new RectAnimation(position, desiredPosition, new Duration(animationTime)) { DecelerationRatio = 1 });
        }

        private void ReorderOthers()
        {
            //Global.logger.LogLine("Reorder others");
            int s = GetOrder(_dragging);
            int i = 0;
            foreach (var child in Children.OfType<UIElement>().OrderBy(GetOrder))
            {
                if (i == s) i++;

                if (child == _dragging) continue;

                var current = GetOrder(child);

                if (i != current)
                    SetOrder(child, current = i);

                i++;
            }
        }

        private void SetBaseOrders()
        {
            if (Children.Count == 0)
                return;

            /*bool any_empty = false;
            foreach(UIElement child in Children)
            {
                if (GetOrder(child) == -1)
                    any_empty = true;
            }

            if (!any_empty)
                return;*/

            int index = 0;
            UIElement[] real_order = new UIElement[Children.Count];
            foreach (UIElement child in Children)
            {
                SetOrder(child, index);
                index++;
            }
        }

        private UIElement GetLocalUIElement(UIElement e)
        {
            var obj = e;
            UIElement parent;
            while ((parent = (UIElement)VisualTreeHelper.GetParent(obj)) != null && parent != this)
            {
                obj = parent;
            }
            return obj;
        }

        public static readonly DependencyProperty OrderProperty = DependencyProperty.RegisterAttached("Order", typeof(int), typeof(ArrangePanel),
                new FrameworkPropertyMetadata(-1));
        public static readonly DependencyProperty ArrangementProperty = DependencyProperty.RegisterAttached("Arrangement", typeof(Rect), typeof(ArrangePanel),
                new FrameworkPropertyMetadata(new Rect(double.NaN, double.NaN, double.NaN, double.NaN), FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public static readonly DependencyProperty DesiredArrangementProperty = DependencyProperty.RegisterAttached("DesiredArrangement", typeof(Rect), typeof(ArrangePanel),
                new FrameworkPropertyMetadata(new Rect(double.NaN, double.NaN, double.NaN, double.NaN)));


        public static int GetOrder(DependencyObject obj)
        {
            return (int)obj.GetValue(OrderProperty);
        }

        public static void SetOrder(DependencyObject obj, int value)
        {
            obj.SetValue(OrderProperty, value);
        }

        public static Rect GetArrangement(DependencyObject obj)
        {
            return (Rect)obj.GetValue(ArrangementProperty);
        }

        public static void SetArrangement(DependencyObject obj, Rect value)
        {
            obj.SetValue(ArrangementProperty, value);
        }

        private Rect GetDesiredArrangement(UIElement obj)
        {
            return (Rect)obj.GetValue(DesiredArrangementProperty);
        }

        private void SetDesiredArrangement(UIElement obj, Rect r)
        {
            obj.SetValue(DesiredArrangementProperty, r);
            AnimateToPosition(obj, r);
        }
    }

    public class ArrangeListBox : ListBox
    {
        public ArrangeListBox() : base()
        {
            var itemspanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(ArrangePanel)));

            itemspanel.VisualTree.AddHandler(ArrangePanel.LoadedEvent, new RoutedEventHandler(ItemsPanel_Loaded));
            itemspanel.Seal();
            ItemsPanelProperty.OverrideMetadata(typeof(ArrangeListBox), new FrameworkPropertyMetadata(itemspanel));
            this.SelectionMode = SelectionMode.Single;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //base.OnMouseMove(e);
            //if (Mouse.Captured == this)
                //ReleaseMouseCapture();

        }

        private ArrangePanel InternalItemsPanel;

        void ItemsPanel_Loaded(object sender, EventArgs e)
        {
            InternalItemsPanel = (ArrangePanel)sender;
            InternalItemsPanel.ParentList = this;
        }

        public void StopReordering()
        {
            InternalItemsPanel?.StopDragging();
        }

        public void UpdateReordering(MouseEventArgs e)
        {
            InternalItemsPanel?.UpdateOrder(e);
        }
    }
}
