﻿using Aurora.EffectsEngine.Animations;
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

        private void animMixer_AnimationFrameItemSelected(object sender, AnimationFrame frame)
        {
            _selectedFrameItem = (Control_AnimationFrameItem)sender;

            StackPanel newPanel = new StackPanel();

            double separatorHeight = 3;

            //Add default options
            if (frame is AnimationCircle)
            {
                AnimationCircle _frameCircle = (frame as AnimationCircle);

                Control_VariableItem varItemColor = new Control_VariableItem()
                {
                    VariableTitle = "Color",
                    VariableObject = _frameCircle.Color
                };
                varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
                Control_VariableItem varItemWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameCircle.Width
                };
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemCenterX = new Control_VariableItem()
                {
                    VariableTitle = "Center X",
                    VariableObject = _frameCircle.Center.X
                };
                varItemCenterX.VariableUpdated += VarItemCenterX_VariableUpdated;
                Control_VariableItem varItemCenterY = new Control_VariableItem()
                {
                    VariableTitle = "Center Y",
                    VariableObject = _frameCircle.Center.Y
                };
                varItemCenterY.VariableUpdated += VarItemCenterY_VariableUpdated;
                Control_VariableItem varItemDimensionRadius = new Control_VariableItem()
                {
                    VariableTitle = "Radius",
                    VariableObject = _frameCircle.Radius
                };
                varItemDimensionRadius.VariableUpdated += VarItemDimensionRadius_VariableUpdated;

                newPanel.Children.Add(varItemColor);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                if (!(frame is AnimationFilledCircle))
                {
                    newPanel.Children.Add(varItemWidth);
                    newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                }
                newPanel.Children.Add(varItemCenterX);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemCenterY);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionRadius);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
            }
            else if (frame is AnimationRectangle)
            {
                AnimationRectangle _frameRectangle = (frame as AnimationRectangle);

                Control_VariableItem varItemColor = new Control_VariableItem()
                {
                    VariableTitle = "Color",
                    VariableObject = _frameRectangle.Color
                };
                varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
                Control_VariableItem varItemWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameRectangle.Width
                };
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemPositionX = new Control_VariableItem()
                {
                    VariableTitle = "Position X",
                    VariableObject = _frameRectangle.Dimension_int.X
                };
                varItemPositionX.VariableUpdated += VarItemPositionX_VariableUpdated;
                Control_VariableItem varItemPositionY = new Control_VariableItem()
                {
                    VariableTitle = "Position Y",
                    VariableObject = _frameRectangle.Dimension_int.Y
                };
                varItemPositionY.VariableUpdated += VarItemPositionY_VariableUpdated;
                Control_VariableItem varItemDimensionWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameRectangle.Dimension_int.Width
                };
                varItemDimensionWidth.VariableUpdated += VarItemDimensionWidth_VariableUpdated;
                Control_VariableItem varItemDimensionHeight = new Control_VariableItem()
                {
                    VariableTitle = "Height",
                    VariableObject = _frameRectangle.Dimension_int.Height
                };
                varItemDimensionHeight.VariableUpdated += VarItemDimensionHeight_VariableUpdated;


                newPanel.Children.Add(varItemColor);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                if (!(frame is AnimationFilledRectangle))
                {
                    newPanel.Children.Add(varItemWidth);
                    newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                }
                newPanel.Children.Add(varItemPositionX);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemPositionY);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionWidth);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemDimensionHeight);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
            }
            else if (frame is AnimationLine)
            {
                AnimationLine _frameLine = (frame as AnimationLine);

                Control_VariableItem varItemColor = new Control_VariableItem()
                {
                    VariableTitle = "Start Color",
                    VariableObject = _frameLine.Color
                };
                varItemColor.VariableUpdated += VarItemColor_VariableUpdated;
                Control_VariableItem varItemEndColor = new Control_VariableItem()
                {
                    VariableTitle = "End Color",
                    VariableObject = _frameLine.EndColor
                };
                varItemEndColor.VariableUpdated += VarItemEndColor_VariableUpdated;
                Control_VariableItem varItemWidth = new Control_VariableItem()
                {
                    VariableTitle = "Width",
                    VariableObject = _frameLine.Width
                };
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemStartPositionX = new Control_VariableItem()
                {
                    VariableTitle = "Start Position X",
                    VariableObject = _frameLine.StartPoint.X
                };
                varItemStartPositionX.VariableUpdated += VarItemStartPositionX_VariableUpdated;
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemStartPositionY = new Control_VariableItem()
                {
                    VariableTitle = "Start Position Y",
                    VariableObject = _frameLine.StartPoint.Y
                };
                varItemStartPositionY.VariableUpdated += VarItemStartPositionY_VariableUpdated;
                Control_VariableItem varItemEndPositionX = new Control_VariableItem()
                {
                    VariableTitle = "End Position X",
                    VariableObject = _frameLine.EndPoint.X
                };
                varItemEndPositionX.VariableUpdated += VarItemEndPositionX_VariableUpdated;
                varItemWidth.VariableUpdated += VarItemWidth_VariableUpdated;
                Control_VariableItem varItemEndPositionY = new Control_VariableItem()
                {
                    VariableTitle = "End Position Y",
                    VariableObject = _frameLine.EndPoint.Y
                };
                varItemEndPositionY.VariableUpdated += VarItemEndPositionY_VariableUpdated;


                newPanel.Children.Add(varItemColor);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemEndColor);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemWidth);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemStartPositionX);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemStartPositionY);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemEndPositionX);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
                newPanel.Children.Add(varItemEndPositionY);
                newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });
            }

            Control_VariableItem varItemAngle = new Control_VariableItem()
            {
                VariableTitle = "Angle",
                VariableObject = frame.Angle
            };
            varItemAngle.VariableUpdated += VarItemAngle_VariableUpdated; ;

            newPanel.Children.Add(varItemAngle);
            newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });

            Control_VariableItem varItemTransitionType = new Control_VariableItem()
            {
                VariableTitle = "Transition Type",
                VariableObject = frame.TransitionType
            };
            varItemTransitionType.VariableUpdated += VarItemTransitionType_VariableUpdated;

            newPanel.Children.Add(varItemTransitionType);
            newPanel.Children.Add(new Separator() { Height = separatorHeight, Opacity = 0 });

            Button btnRemoveFrame = new Button()
            {
                Content = "Remove Frame",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            btnRemoveFrame.Click += BtnRemoveFrame_Click;

            newPanel.Children.Add(btnRemoveFrame);

            grpbxProperties.Content = newPanel;
        }

        private void VarItemEndColor_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).SetEndColor((System.Drawing.Color)newVariable);
        }

        private void VarItemEndPositionY_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).SetEndPoint(new PointF(((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).EndPoint.X, (float)newVariable));
        }

        private void VarItemEndPositionX_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).SetEndPoint(new PointF((float)newVariable, ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).EndPoint.Y));
        }

        private void VarItemStartPositionY_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).SetStartPoint(new PointF(((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).StartPoint.X, (float)newVariable));
        }

        private void VarItemStartPositionX_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).SetStartPoint(new PointF((float)newVariable, ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationLine).StartPoint.Y));
        }

        private void VarItemAngle_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationFrame).SetAngle((float)newVariable);
        }

        private void VarItemTransitionType_VariableUpdated(object sender, object newVariable)
        {
            if (_selectedFrameItem != null)
                (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame = ((_selectedFrameItem as Control_AnimationFrameItem).ContextFrame as AnimationFrame).SetTransitionType((AnimationFrameTransitionType)newVariable);
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
                Type FrameType = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType();

                if (FrameType == typeof(AnimationRectangle) || FrameType == typeof(AnimationFilledRectangle))
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
                Type FrameType = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType();

                if (FrameType == typeof(AnimationRectangle) || FrameType == typeof(AnimationFilledRectangle))
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
                Type FrameType = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType();

                if (FrameType == typeof(AnimationRectangle) || FrameType == typeof(AnimationFilledRectangle))
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
                Type FrameType = (_selectedFrameItem as Control_AnimationFrameItem).ContextFrame.GetType();

                if (FrameType == typeof(AnimationRectangle) || FrameType == typeof(AnimationFilledRectangle))
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
