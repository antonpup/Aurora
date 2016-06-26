/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorBox
{
    class GradientStopSlider : Slider
    {
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (this.ColorBox != null)
            {
                this.ColorBox._BrushSetInternally = true;
                this.ColorBox._UpdateBrush = false;        

                this.ColorBox.SelectedGradient = this.SelectedGradient;
                this.ColorBox.Color = this.SelectedGradient.Color;
                
                this.ColorBox._UpdateBrush = true;
                //this.ColorBox._BrushSetInternally = false;

                //e.Handled = true;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            if (this.ColorBox != null)
            {
                //this.ColorBox._HSBSetInternally = true;
                //this.ColorBox._RGBSetInternally = true;
                this.ColorBox._BrushSetInternally = true;
                this.ColorBox.SetBrush();
                this.ColorBox._HSBSetInternally = false;
                //this.ColorBox._RGBSetInternally = false;
                //this.ColorBox._BrushSetInternally = false;
            }            
        }

        public ColorBox ColorBox
        {
            get { return (ColorBox)GetValue(ColorBoxProperty); }
            set { SetValue(ColorBoxProperty, value); }
        }
        public static readonly DependencyProperty ColorBoxProperty =
            DependencyProperty.Register("ColorBox", typeof(ColorBox), typeof(GradientStopSlider));

        public GradientStop SelectedGradient
        {
            get { return (GradientStop)GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }
        public static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof(GradientStop), typeof(GradientStopSlider));
    }
}
