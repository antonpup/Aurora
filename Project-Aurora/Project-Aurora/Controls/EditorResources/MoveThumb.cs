using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora.Controls.EditorResources
{
    public class MoveThumb : Thumb
    {
        private Border boundingBox = new Border { BorderBrush = Brushes.Red, BorderThickness = new Thickness(1) };
        private bool added = true;

        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl designerItem = DataContext as ContentControl;

            if (designerItem != null)
            {
                Point dragDelta = new Point(e.HorizontalChange, e.VerticalChange);

                RotateTransform rotateTransform = designerItem.RenderTransform as RotateTransform;
                if (rotateTransform != null)
                {
                    dragDelta = rotateTransform.Transform(dragDelta);
                }

                Canvas parent_canvas = designerItem.Parent as Canvas;

                double left_canvas = Canvas.GetLeft(designerItem);
                double right_canvas = left_canvas + designerItem.ActualWidth;
                double top_canvas = Canvas.GetTop(designerItem);
                double bottom_canvas = top_canvas + designerItem.ActualHeight;

                Rect orig_bounds = new Rect(designerItem.RenderSize);
                Rect bounds = new Rect(designerItem.RenderSize);

                if (rotateTransform != null)
                {
                    bounds = rotateTransform.TransformBounds(bounds);
                }

                if (rotateTransform != null)
                {
                    left_canvas = left_canvas + (orig_bounds.Width - bounds.Width) / 2.0;
                    top_canvas = top_canvas + (orig_bounds.Height - bounds.Height) / 2.0;

                    Canvas.SetLeft(boundingBox, left_canvas);
                    Canvas.SetTop(boundingBox, top_canvas);
                    boundingBox.Width = bounds.Width;
                    boundingBox.Height = bounds.Height;
                    if (!added)
                    {
                        (designerItem.Parent as Canvas).Children.Add(boundingBox);
                        added = true;
                    }
                    else
                    {
                        boundingBox.UpdateLayout();
                    }
                }


                //Horizontal Colision
                if (left_canvas + dragDelta.X < 0)
                    Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem));
                else if (left_canvas + Math.Abs(bounds.Width) + dragDelta.X > parent_canvas.ActualWidth)
                    Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem));
                else
                    Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + dragDelta.X);

                //Vertical Collision
                if (top_canvas + dragDelta.Y < 0)
                    Canvas.SetTop(designerItem, Canvas.GetTop(designerItem));
                else if (top_canvas + Math.Abs(bounds.Height) + dragDelta.Y > parent_canvas.ActualHeight+5)
                    Canvas.SetTop(designerItem, Canvas.GetTop(designerItem));
                else
                    Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + dragDelta.Y);
            }
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member