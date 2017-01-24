using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
    /// Interaction logic for Control_AnimationEditor.xaml
    /// </summary>
    public partial class Control_AnimationEditor : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty AnimationMixProperty = DependencyProperty.Register("AnimationMix", typeof(AnimationMix), typeof(Control_AnimationEditor));

        public AnimationMix AnimationMix
        {
            get
            {
                return (AnimationMix)GetValue(AnimationMixProperty);
            }
            set
            {
                SetValue(AnimationMixProperty, value);

                animMixer.ContextMix = value;
            }
        }

        private UIElement _selectedFrameItem = null;

        public Control_AnimationEditor()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void animMixer_AnimationMixRendered(object sender)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    (sender as Control_AnimationMixPresenter).RenderedBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    keyboard_overlayPreview.Source = bitmapimage;
                }
            }
            catch (Exception exc)
            {
            }
        }

        private void keyboard_overlayPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float scale = (float)(e.NewSize.Width / Effects.canvas_width);

            if (scale < 1.0f)
                scale = 1.0f;

            animMixer.AnimationScale = scale;
        }

        private void animMixer_AnimationFrameItemSelected(object sender, AnimationFrame track)
        {
            _selectedFrameItem = (Control_AnimationFrameItem)sender;

            StackPanel newPanel = new StackPanel();

            //Add default options
            if (track is AnimationCircle)
            {
                AnimationCircle _frameTrack = (track as AnimationCircle);

                Control_VariableItem varItemColor = new Control_VariableItem()
                {
                    VariableTitle = "Color",
                    VariableObject = _frameTrack.Color
                };
                varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
                Control_VariableItem varItemWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameTrack.Width
                };
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemCenterX = new Control_VariableItem()
                {
                    VariableTitle = "Center X",
                    VariableObject = _frameTrack.Center.X
                };
                varItemCenterX.VariableUpdated += VarItemCenterX_VariableUpdated;
                Control_VariableItem varItemCenterY = new Control_VariableItem()
                {
                    VariableTitle = "Center Y",
                    VariableObject = _frameTrack.Center.Y
                };
                varItemCenterY.VariableUpdated += VarItemCenterY_VariableUpdated;
                Control_VariableItem varItemDimensionRadius = new Control_VariableItem()
                {
                    VariableTitle = "Radius",
                    VariableObject = _frameTrack.Radius
                };
                varItemDimensionRadius.VariableUpdated += VarItemDimensionRadius_VariableUpdated;

                newPanel.Children.Add(varItemColor);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemWidth);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemCenterX);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemCenterY);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionRadius);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
            }
            else if (track is AnimationRectangle)
            {
                AnimationRectangle _frameTrack = (track as AnimationRectangle);

                Control_VariableItem varItemColor = new Control_VariableItem()
                {
                    VariableTitle = "Color",
                    VariableObject = _frameTrack.Color
                };
                varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
                Control_VariableItem varItemWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameTrack.Width
                };
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemPositionX = new Control_VariableItem()
                {
                    VariableTitle = "Position X",
                    VariableObject = _frameTrack.Dimension.X
                };
                varItemPositionX.VariableUpdated += VarItemPositionX_VariableUpdated;
                Control_VariableItem varItemPositionY = new Control_VariableItem()
                {
                    VariableTitle = "Position Y",
                    VariableObject = _frameTrack.Dimension.Y
                };
                varItemPositionY.VariableUpdated += VarItemPositionY_VariableUpdated;
                Control_VariableItem varItemDimensionWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameTrack.Dimension.Width
                };
                varItemDimensionWidth.VariableUpdated += VarItemDimensionWidth_VariableUpdated;
                Control_VariableItem varItemDimensionHeight = new Control_VariableItem()
                {
                    VariableTitle = "Height",
                    VariableObject = _frameTrack.Dimension.Height
                };
                varItemDimensionHeight.VariableUpdated += VarItemDimensionHeight_VariableUpdated;


                newPanel.Children.Add(varItemColor);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemWidth);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemPositionX);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemPositionY);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionWidth);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionHeight);
                newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
            }

            Button btnRemoveFrame = new Button()
            {
                Content = "Remove Frame",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            btnRemoveFrame.Click += BtnRemoveFrame_Click;

            newPanel.Children.Add(new Separator() { Height = 5, Opacity = 0 });
            newPanel.Children.Add(btnRemoveFrame);

            grpbxProperties.Content = newPanel;
        }

        private void BtnRemoveFrame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = null;

            grpbxProperties.Content = null;
        }

        private void VarItemDimensionRadius_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationCircle).SetRadius((float)newVariable);
        }

        private void VarItemCenterY_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationCircle).SetCenter(new PointF(((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationCircle).Center.X, (float)newVariable));
        }

        private void VarItemCenterX_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationCircle).SetCenter(new PointF((float)newVariable, ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationCircle).Center.Y));
        }

        private void VarItemDimensionHeight_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
            {
                if ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType() == typeof(AnimationRectangle))
                {
                    AnimationRectangle frame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationRectangle);

                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = frame.SetDimensionInt(new System.Drawing.Rectangle(frame.Dimension_int.X, frame.Dimension_int.Y, frame.Dimension_int.Width, (int)(float)newVariable));
                }
                else
                {
                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetDimension(new System.Drawing.RectangleF((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.X, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Y, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Width, (float)newVariable));
                }
            }
        }

        private void VarItemDimensionWidth_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
            {
                if ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType() == typeof(AnimationRectangle))
                {
                    AnimationRectangle frame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationRectangle);

                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = frame.SetDimensionInt(new System.Drawing.Rectangle(frame.Dimension_int.X, frame.Dimension_int.Y, (int)(float)newVariable, frame.Dimension_int.Height));
                }
                else
                {
                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetDimension(new System.Drawing.RectangleF((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.X, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Y, (float)newVariable, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Height));

                }
            }
        }

        private void VarItemPositionY_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
            {
                if ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType() == typeof(AnimationRectangle))
                {
                    AnimationRectangle frame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationRectangle);

                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = frame.SetDimensionInt(new System.Drawing.Rectangle(frame.Dimension_int.X, (int)(float)newVariable, frame.Dimension_int.Width, frame.Dimension_int.Height));
                }
                else
                {
                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetDimension(new System.Drawing.RectangleF((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.X, (float)newVariable, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Width, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Height));
                }
            }
        }

        private void VarItemPositionX_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
            {
                if ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType() == typeof(AnimationRectangle))
                {
                    AnimationRectangle frame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationRectangle);

                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = frame.SetDimensionInt(new System.Drawing.Rectangle((int)(float)newVariable, frame.Dimension_int.Y, frame.Dimension_int.Width, frame.Dimension_int.Height));
                }
                else
                {
                    (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetDimension(new System.Drawing.RectangleF((float)newVariable, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Y, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Width, (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.Dimension.Height));
                }
            }
        }

        private void VarItemWidth_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetWidth((int)newVariable);
        }

        private void VarItemColor_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.SetColor((System.Drawing.Color)newVariable);
        }
    }
}
