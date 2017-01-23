using Aurora.EffectsEngine.Animations;
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
    /// Interaction logic for Control_AnimationTrackPresenter.xaml
    /// </summary>
    public partial class Control_AnimationTrackPresenter : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ContextTrackProperty = DependencyProperty.Register("ContextTrack", typeof(AnimationTrack), typeof(Control_AnimationTrackPresenter));

        public AnimationTrack ContextTrack
        {
            get
            {
                return (AnimationTrack)GetValue(ContextTrackProperty);
            }
            set
            {
                SetValue(ContextTrackProperty, value);

                UpdateControls();
            }
        }

        public delegate void AnimationTrackArgs(object sender, AnimationTrack track);

        public event AnimationTrackArgs AnimationTrackUpdated;

        public event Control_AnimationFrameItem.AnimationFrameItemArgs AnimationFrameItemSelected;

        private double _height = 100.0;

        private double ConvertToLocation(float time, float shift = 0.0f)
        {
            return (time + shift) * 50.0;
        }

        private float ConvertToTime(double loc, float shift = 0.0f)
        {
            return (float)(loc / 50.0f) - shift;
        }

        public Control_AnimationTrackPresenter()
        {
            InitializeComponent();
        }

        private void UpdateControls()
        {
            txtblkTrackName.Text = ContextTrack.GetName();

            gridTrackItems.Children.Clear();

            foreach (var kvp in ContextTrack.GetAnimations())
            {
                Control_AnimationFrameItem newFrame = new Control_AnimationFrameItem() { ContextFrame = kvp.Value, Margin = new Thickness(ConvertToLocation(kvp.Key, ContextTrack.GetShift()), 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, Width = ConvertToLocation(kvp.Value.Duration) };
                newFrame.LeftSplitterDrag += Control_AnimationFrameItem_LeftSplitterDrag;
                newFrame.RightSplitterDrag += Control_AnimationFrameItem_RightSplitterDrag;
                newFrame.ContentSplitterDrag += Control_AnimationFrameItem_ContentSplitterDrag;
                newFrame.CompletedDrag += Control_AnimationFrameItem_CompletedDrag;
                newFrame.PreviewMouseDown += Control_AnimationFrameItem_PreviewMouseDown;
                newFrame.AnimationFrameItemUpdated += Control_AnimationFrameItem_AnimationFrameItemUpdated;

                gridTrackItems.Children.Add(newFrame);
            }
        }

        private void Control_AnimationFrameItem_AnimationFrameItemUpdated(object sender, AnimationFrame track)
        {
            UpdateAnimationTrack();
            UpdateControls();
        }

        private void Control_AnimationFrameItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Selected
            AnimationFrameItemSelected?.Invoke(sender, (sender as Control_AnimationFrameItem).ContextFrame);
        }

        private void Control_AnimationFrameItem_CompletedDrag(object sender, double delta)
        {
            UpdateAnimationTrack();
        }

        private void Control_AnimationFrameItem_LeftSplitterDrag(object sender, double delta)
        {
            double oldMargin = (sender as Control_AnimationFrameItem).Margin.Left;
            double oldWidth = (sender as Control_AnimationFrameItem).Width;
            double newMargin = oldMargin + delta;
            double newWidth = oldWidth - delta;

            if (newWidth > 0 && newMargin > 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, delta, -delta))
            {
                (sender as Control_AnimationFrameItem).Width = newWidth;
                (sender as Control_AnimationFrameItem).Margin = new Thickness(newMargin, 0, 0, 0);
            }
        }

        private void Control_AnimationFrameItem_RightSplitterDrag(object sender, double delta)
        {
            double oldWidth = (sender as Control_AnimationFrameItem).Width;
            double newWidth = oldWidth + delta;

            if(newWidth > 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, 0, delta))
            {
                (sender as Control_AnimationFrameItem).Width = newWidth;
            }
        }

        private void Control_AnimationFrameItem_ContentSplitterDrag(object sender, double delta)
        {
            double oldMargin = (sender as Control_AnimationFrameItem).Margin.Left;
            double newMargin = oldMargin + delta;

            if (newMargin > 0 && !CheckControlOverlap(sender as Control_AnimationFrameItem, delta))
            {
                (sender as Control_AnimationFrameItem).Margin = new Thickness(newMargin, 0, 0, 0);
            }
        }

        private bool CheckControlOverlap(Control_AnimationFrameItem item, double leftMarginDelta = 0.0, double widthDelta = 0.0)
        {
            Rect itemPosition = new Rect();
            itemPosition.Location = item.PointToScreen(new Point(leftMarginDelta + 0, 0));
            itemPosition.Height = item.ActualHeight;
            itemPosition.Width = item.ActualWidth + widthDelta;

            bool doesIntersect = false;

            foreach (var child in gridTrackItems.Children)
            {
                if (child is Control_AnimationFrameItem && (child as Control_AnimationFrameItem) != item)
                {
                    Rect childPos = new Rect();
                    childPos.Location = (child as Control_AnimationFrameItem).PointToScreen(new Point(0, 0));
                    childPos.Height = (child as Control_AnimationFrameItem).ActualHeight;
                    childPos.Width = (child as Control_AnimationFrameItem).ActualWidth;

                    if (itemPosition.IntersectsWith(childPos))
                    {
                        doesIntersect = true;
                        break;
                    }
                }
            }

            return doesIntersect;
        }

        private void UpdateAnimationTrack()
        {
            AnimationTrack newTrack = new AnimationTrack(ContextTrack.GetName(), 0.0f, ContextTrack.GetShift());

            foreach (var child in gridTrackItems.Children)
            {
                if (child is Control_AnimationFrameItem)
                {
                    Control_AnimationFrameItem item = (child as Control_AnimationFrameItem);

                    newTrack.SetFrame(ConvertToTime(item.Margin.Left, ContextTrack.GetShift()), item.ContextFrame.SetDuration(ConvertToTime(item.Width)));
                }
            }

            ContextTrack = newTrack;

            AnimationTrackUpdated?.Invoke(this, newTrack);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            _height = this.Height;
            this.Height = 50;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            this.Height = _height;
        }

        private void gridTrackItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Point mouseLoc = e.GetPosition(sender as Grid);

                float time = ConvertToTime(mouseLoc.X, ContextTrack.GetShift());

                ContextTrack.SetFrame(time, ContextTrack.GetFrame(time));

                UpdateControls();
            }
        }
    }
}
