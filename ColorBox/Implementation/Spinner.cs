/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ColorBox
{
    internal class Spinner : ContentControl
    {
        static Spinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spinner), new FrameworkPropertyMetadata(typeof(Spinner)));
        }

        public static readonly DependencyProperty ValidSpinDirectionProperty = DependencyProperty.Register("ValidSpinDirection", typeof(ValidSpinDirections), typeof(Spinner), new PropertyMetadata(ValidSpinDirections.Increase | ValidSpinDirections.Decrease, OnValidSpinDirectionPropertyChanged));
        public ValidSpinDirections ValidSpinDirection
        {
            get { return (ValidSpinDirections)GetValue(ValidSpinDirectionProperty); }
            set { SetValue(ValidSpinDirectionProperty, value); }
        }
        public static void OnValidSpinDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidSpinDirections oldvalue = (ValidSpinDirections)e.OldValue;
            ValidSpinDirections newvalue = (ValidSpinDirections)e.NewValue;
        }

        public event EventHandler<SpinEventArgs> Spin; 
        protected virtual void OnSpin(SpinEventArgs e)
        {
            EventHandler<SpinEventArgs> handler = Spin;
            if (handler != null)            
                handler(this, e);            
        }
    }
}
