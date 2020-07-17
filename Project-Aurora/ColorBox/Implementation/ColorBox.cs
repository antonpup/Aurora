﻿/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using Aurora.EffectsEngine;

namespace ColorBox
{
    [TemplatePart(Name = PART_CurrentColor, Type = typeof(TextBox))]
    public class ColorBox : Control
    {
        internal const string PART_CurrentColor = "PART_CurrentColor";

        //internal bool _GradientStopSetInternally = false;
        internal bool _HSBSetInternally = false;
        internal bool _RGBSetInternally = false;
        internal bool _BrushSetInternally = false;
        internal bool _BrushTypeSetInternally = false;
        internal bool _UpdateBrush = true;

        internal TextBox CurrentColorTextBox
        {
            get;
            private set;
        }

        static ColorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorBox), new FrameworkPropertyMetadata(typeof(ColorBox)));
        }
        
        public static RoutedCommand RemoveGradientStop = new RoutedCommand();
        public static RoutedCommand ReverseGradientStop = new RoutedCommand();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CurrentColorTextBox = GetTemplateChild(PART_CurrentColor) as TextBox;
            if (CurrentColorTextBox != null)
            {
                CurrentColorTextBox.PreviewKeyDown += CurrentColorTextBox_PreviewKeyDown;        
            }

            this.CommandBindings.Add(new CommandBinding(ColorBox.RemoveGradientStop, RemoveGradientStop_Executed));
            this.CommandBindings.Add(new CommandBinding(ColorBox.ReverseGradientStop, ReverseGradientStop_Executed));
        }

        void CurrentColorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {                
                BindingExpression be = CurrentColorTextBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }
        }
        
        private void RemoveGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Gradients != null && this.Gradients.Count > 2)
            {
                this.Gradients.Remove(this.SelectedGradient);
                this.SetBrush();
            }
        }

        private void ReverseGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._UpdateBrush = false;
            this._BrushSetInternally = true;
            foreach (GradientStop gs in Gradients)
            {
                gs.Offset = 1.0 - gs.Offset;
            }
            this._UpdateBrush = true;
            this._BrushSetInternally = false;
            this.SetBrush();
        }

        /*void InitTransform()
        {
            if (this.Brush.Transform == null || this.Brush.Transform.Value.IsIdentity)
            {
                this._BrushSetInternally = true;

                TransformGroup _tg = new TransformGroup();
                _tg.Children.Add(new RotateTransform());
                _tg.Children.Add(new ScaleTransform());
                _tg.Children.Add(new SkewTransform());
                _tg.Children.Add(new TranslateTransform());
                this.Brush.Transform = _tg;

                this._BrushSetInternally = false;
            }
        }*/

        #region Private Properties
        /*float StartX
        {
            get { return (float)GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }
        static readonly DependencyProperty StartXProperty =
            DependencyProperty.Register("StartX", typeof(float), typeof(ColorBox), new PropertyMetadata(0f, new PropertyChangedCallback(StartXChanged)));
        static void StartXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            cp._BrushSetInternally = true;
            cp.Brush.start.X = (float)args.NewValue;
            cp._BrushSetInternally = false;
        }

        float StartY
        {
            get { return (float)GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }
        static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register("StartY", typeof(float), typeof(ColorBox), new PropertyMetadata(0.0f, new PropertyChangedCallback(StartYChanged)));
        static void StartYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            cp._BrushSetInternally = true;
            cp.Brush.start.Y = (float)args.NewValue;
            cp._BrushSetInternally = false;
        }

        float EndX
        {
            get { return (float)GetValue(EndXProperty); }
            set { SetValue(EndXProperty, value); }
        }
        static readonly DependencyProperty EndXProperty =
            DependencyProperty.Register("EndX", typeof(float), typeof(ColorBox), new PropertyMetadata(1.0f, new PropertyChangedCallback(EndXChanged)));
        static void EndXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            cp._BrushSetInternally = true;
            cp.Brush.end.X = (float)args.NewValue;
            cp._BrushSetInternally = false;
        }

        float EndY
        {
            get { return (float)GetValue(EndYProperty); }
            set { SetValue(EndYProperty, value); }
        }
        static readonly DependencyProperty EndYProperty =
            DependencyProperty.Register("EndY", typeof(float), typeof(ColorBox), new PropertyMetadata(1.0f, new PropertyChangedCallback(EndYChanged)));
        static void EndYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            cp._BrushSetInternally = true;
            cp.Brush.end.Y = (float)args.NewValue;
            cp._BrushSetInternally = false;
        }*/
        int Angle
        {
            get { return (int)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
        static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(int), typeof(ColorBox), new PropertyMetadata(0, new PropertyChangedCallback(AngleChanged)));
        static void AngleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            
            if (cp.Brush is LinearEffectBrush linearBrush)
            {
                cp._BrushSetInternally = true;
                linearBrush.Angle = (int)args.NewValue;
                cp.SetBrush();
                cp._BrushSetInternally = false;
            }
            
        }
        double CenterX
        {
            get { return (double)GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }
        static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(ColorBox), new PropertyMetadata(0.5, new PropertyChangedCallback(CenterXChanged)));
        static void CenterXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialEffectBrush radialBrush)
            {
                float centerX = (float)((double)args.NewValue % 1);

                cp._BrushSetInternally = true;
                radialBrush.Center.X = centerX;
                cp._BrushSetInternally = false;
            }
        }

        double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }
        static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(ColorBox), new PropertyMetadata(0.5, new PropertyChangedCallback(CenterYChanged)));
        static void CenterYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialEffectBrush radialBrush)
            {
                float centerY = (float)((double)args.NewValue);
                if (centerY > 1f) 
                {
                    centerY %= 1;
                }
                cp._BrushSetInternally = true;
                radialBrush.Center.Y = centerY;
                cp._BrushSetInternally = false;
            }
        }

        float SampleWindowSize
        {
            get { return (float)GetValue(SampleWindowSizeProperty); }
            set { SetValue(SampleWindowSizeProperty, value); }
        }
        static readonly DependencyProperty SampleWindowSizeProperty =
            DependencyProperty.Register("SampleWindowSize", typeof(float), typeof(ColorBox), new PropertyMetadata(1.0f, new PropertyChangedCallback(SampleWindowSizeChanged)));
        static void SampleWindowSizeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;

            cp._BrushSetInternally = true;
            cp.Brush.SampleWindowSize = (float)args.NewValue;
            cp._BrushSetInternally = false;
        }


        double BrushOpacity
        {
            get { return (double)GetValue(BrushOpacityProperty); }
            set { SetValue(BrushOpacityProperty, value); }
        }
        static readonly DependencyProperty BrushOpacityProperty =
            DependencyProperty.Register("BrushOpacity", typeof(double), typeof(ColorBox), new PropertyMetadata(1.0));
        //static void BrushOpacityChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        //{
        //    ColorBox cp = property as ColorBox;
        //    cp._BrushSetInternally = true;
        //    cp.Brush.Opacity = (double)args.NewValue;
        //    cp._BrushSetInternally = false;            
        //}

        /*EffectBrush.BrushWrap SpreadMethod
        {
            get { return (EffectBrush.BrushWrap)GetValue(SpreadMethodProperty); }
            set { SetValue(SpreadMethodProperty, value); }
        }
        static readonly DependencyProperty SpreadMethodProperty =
            DependencyProperty.Register("SpreadMethod", typeof(EffectBrush.BrushWrap), typeof(ColorBox), new PropertyMetadata(EffectBrush.BrushWrap.None, new PropertyChangedCallback(SpreadMethodChanged)));
        static void SpreadMethodChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            cp._BrushSetInternally = true;
            cp.Brush.wrap = (EffectBrush.BrushWrap)args.NewValue;
            cp._BrushSetInternally = false;
        }*/

        #endregion

        #region Internal Properties

        internal ObservableCollection<GradientStop> Gradients
        {
            get { return (ObservableCollection<GradientStop>)GetValue(GradientsProperty); }
            set { SetValue(GradientsProperty, value); }
        }
        internal static readonly DependencyProperty GradientsProperty =
            DependencyProperty.Register("Gradients", typeof(ObservableCollection<GradientStop>), typeof(ColorBox));

        internal GradientStop SelectedGradient
        {
            get { return (GradientStop)GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }
        internal static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof(GradientStop), typeof(ColorBox));

        internal BrushTypes BrushType
        {
            get { return (BrushTypes)GetValue(BrushTypeProperty); }
            set { SetValue(BrushTypeProperty, value); }
        }
        internal static readonly DependencyProperty BrushTypeProperty =
            DependencyProperty.Register("BrushType", typeof(BrushTypes), typeof(ColorBox),
            new FrameworkPropertyMetadata(BrushTypes.Solid, new PropertyChangedCallback(BrushTypeChanged)));
        static void BrushTypeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox c = property as ColorBox;
            if (!c._BrushTypeSetInternally)
            {
                if (c.Gradients == null)
                {
                    c.Gradients = new ObservableCollection<GradientStop>();
                    c.Gradients.Add(new GradientStop(Colors.Black, 0));
                    c.Gradients.Add(new GradientStop(Colors.White, 1));
                }

                c.SetBrush();
            }
        }
        
        #endregion

        #region Public Properties

        public IEnumerable<Enum> SpreadMethodTypes
        {
            get
            {
                GradientSpreadMethod temp = GradientSpreadMethod.Pad | GradientSpreadMethod.Reflect | GradientSpreadMethod.Repeat;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public IEnumerable<Enum> MappingModeTypes
        {
            get
            {
                BrushMappingMode temp = BrushMappingMode.Absolute | BrushMappingMode.RelativeToBoundingBox;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public IEnumerable<Enum> AvailableBrushTypes
        {
            get
            {
                BrushTypes temp = /*BrushTypes.None |*/ BrushTypes.Solid | BrushTypes.Linear | BrushTypes.Radial;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public Brush MediaBrush
        {
            get { return (Brush)GetValue(MediaBrushProperty); }
            set { SetValue(MediaBrushProperty, value); }
        }

        public static readonly DependencyProperty MediaBrushProperty =
            DependencyProperty.Register("MediaBrush", typeof(Brush), typeof(ColorBox)
            , new FrameworkPropertyMetadata(null));//, new PropertyChangedCallback(MediaBrushChangedInternal)));
        /*static void MediaBrushChangedInternal(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox c = property as ColorBox;
            Brush brush = args.NewValue as Brush;

            if (!c._BrushSetInternally)
            {
                c._BrushTypeSetInternally = true;

                if (brush == null)
                {
                    c.BrushType = BrushTypes.Solid;
                    c.Color = Color.FromArgb(255, 255, 0, 0);
                }
                else if (brush is SolidColorBrush)
                {
                    c.BrushType = BrushTypes.Solid;
                    c.Color = (brush as SolidColorBrush).Color;
                }
                else if (brush is LinearGradientBrush)
                {
                    LinearGradientBrush lgb = brush as LinearGradientBrush;
                    //c.Opacity = lgb.Opacity;

                    c.Gradients = new ObservableCollection<GradientStop>(lgb.GradientStops);
                    c.BrushType = BrushTypes.Linear;
                    //c.Color = lgb.GradientStops.OrderBy(x => x.Offset).Last().Color;
                    //c.SelectedGradient = lgb.GradientStops.OrderBy(x => x.Offset).Last();
                }
                else
                {


                    RadialGradientBrush rgb = brush as RadialGradientBrush;

                    c.Gradients = new ObservableCollection<GradientStop>(rgb.GradientStops);
                    c.BrushType = BrushTypes.Radial;
                    //c.Color = rgb.GradientStops.OrderBy(x => x.Offset).Last().Color;
                    //c.SelectedGradient = rgb.GradientStops.OrderBy(x => x.Offset).Last();

                }

                c._BrushTypeSetInternally = false;
            }
            else
            {
                c.RaiseBrushChangedEvent((Brush)args.NewValue);
            }
        }*/
        public EffectBrush Brush
        {
            get { return (EffectBrush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }
        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(EffectBrush), typeof(ColorBox)
            , new FrameworkPropertyMetadata(null, new PropertyChangedCallback(BrushChangedInternal)));
        static void BrushChangedInternal(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox c = property as ColorBox;
            EffectBrush brush = args.NewValue as EffectBrush;

            if (!c._BrushSetInternally)
            {
                c._BrushTypeSetInternally = true;

                if (brush == null)
                {
                    c.BrushType = BrushTypes.Solid;
                    c.Color = Color.FromArgb(255, 255, 0, 0);
                }
                else if (brush is SolidEffectBrush seb)
                {
                    c.BrushType = BrushTypes.Solid;
                    c.Color = seb.GetGradientStopCollection()[0].Color;
                }
                else if (brush is LinearEffectBrush leb)
                {
                   
                    c.Gradients = new ObservableCollection<GradientStop>(leb.GetGradientStopCollection());
                    c.BrushType = BrushTypes.Linear;
                    c.Angle = leb.Angle;
                    c.SampleWindowSize = leb.SampleWindowSize;
                }
                else
                {

                    RadialEffectBrush reb = brush as RadialEffectBrush;

                    c.Gradients = new ObservableCollection<GradientStop>(reb.GetGradientStopCollection());
                    c.SampleWindowSize = reb.SampleWindowSize;
                    c.BrushType = BrushTypes.Radial;
                    c.CenterX = reb.Center.X;
                    c.CenterY = reb.Center.Y;
                    //c.Color = rgb.GradientStops.OrderBy(x => x.Offset).Last().Color;
                    //c.SelectedGradient = rgb.GradientStops.OrderBy(x => x.Offset).Last();
                    
                }
                c._BrushTypeSetInternally = false;
            }
            else
            {
                c.RaiseBrushChangedEvent(((EffectBrush)args.NewValue).MediaBrush);
            }
            c.MediaBrush = brush.MediaBrush;
        }       

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorBox), new UIPropertyMetadata(Colors.Black, OnColorChanged));
        public static void OnColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox)o;

            if (e.NewValue is Color)
            {
                Color color = (Color)e.NewValue;

                if (!c._HSBSetInternally)
                {
                    // update HSB value based on new value of color

                    double H = 0;
                    double S = 0;
                    double B = 0;
                    ColorHelper.HSBFromColor(color, ref H, ref S, ref B);

                    c._HSBSetInternally = true;

                    c.Alpha = (double)(color.A / 255d);
                    c.Hue = H;
                    c.Saturation = S;
                    c.Brightness = B;

                    c._HSBSetInternally = false;
                }

                if (!c._RGBSetInternally)
                {
                    // update RGB value based on new value of color

                    c._RGBSetInternally = true;
                  
                    c.A = color.A;
                    c.R = color.R;
                    c.G = color.G;
                    c.B = color.B;

                    c._RGBSetInternally = false;
                }
                
                c.RaiseColorChangedEvent((Color)e.NewValue);
            }
        }

        #endregion

        
        #region Color Specific Properties

        private double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }
        private static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(ColorBox),
            new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(UpdateColorHSB), new CoerceValueCallback(HueCoerce)));
        private static object HueCoerce(DependencyObject d, object Hue)
        {
            double v = (double)Hue;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }
        private static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(ColorBox),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(UpdateColorHSB), new CoerceValueCallback(BrightnessCoerce)));
        private static object BrightnessCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }
        private static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(ColorBox),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(UpdateColorHSB), new CoerceValueCallback(SaturationCoerce)));
        private static object SaturationCoerce(DependencyObject d, object Saturation)
        {
            double v = (double)Saturation;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Alpha
        {
            get { return (double)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }
        private static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(ColorBox),
            new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(UpdateColorHSB), new CoerceValueCallback(AlphaCoerce)));
        private static object AlphaCoerce(DependencyObject d, object Alpha)
        {
            double v = (double)Alpha;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private int A
        {
            get { return (int)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }
        private static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(int), typeof(ColorBox),
            new FrameworkPropertyMetadata(255, new PropertyChangedCallback(UpdateColorRGB), new CoerceValueCallback(RGBCoerce)));


        private int R
        {
            get { return (int)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }
        private static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(int), typeof(ColorBox),
            new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB), new CoerceValueCallback(RGBCoerce)));


        private int G
        {
            get { return (int)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }
        private static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(int), typeof(ColorBox),
            new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB), new CoerceValueCallback(RGBCoerce)));


        private int B
        {
            get { return (int)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }
        private static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(int), typeof(ColorBox),
            new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB), new CoerceValueCallback(RGBCoerce)));
        

        private static object RGBCoerce(DependencyObject d, object value)
        {
            int v = (int)value;
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        #endregion

        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorHSB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox)o;
            Color n = ColorHelper.ColorFromAHSB(c.Alpha, c.Hue, c.Saturation, c.Brightness);

            c._HSBSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c._HSBSetInternally = false;
        }

        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorRGB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox)o;
            Color n = Color.FromArgb((byte)c.A, (byte)c.R, (byte)c.G, (byte)c.B);

            c._RGBSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c._RGBSetInternally = false;
        }

       
        #region ColorChanged Event

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof(ColorChangedEventHandler), typeof(ColorBox));

        public event ColorChangedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        void RaiseColorChangedEvent(Color color)
        {
            ColorChangedEventArgs newEventArgs = new ColorChangedEventArgs(ColorBox.ColorChangedEvent, color);
            RaiseEvent(newEventArgs);
        }

        #endregion

        
        #region BrushChanged Event

        public delegate void BrushChangedEventHandler(object sender, BrushChangedEventArgs e);

        public static readonly RoutedEvent BrushChangedEvent =
            EventManager.RegisterRoutedEvent("BrushChanged", RoutingStrategy.Bubble, typeof(BrushChangedEventHandler), typeof(ColorBox));

        public event BrushChangedEventHandler BrushChanged
        {
            add { AddHandler(BrushChangedEvent, value); }
            remove { RemoveHandler(BrushChangedEvent, value); }
        }

        void RaiseBrushChangedEvent(Brush brush)
        {
            BrushChangedEventArgs newEventArgs = new BrushChangedEventArgs(ColorBox.BrushChangedEvent, brush);
            RaiseEvent(newEventArgs);
        }

        #endregion
        

        private ColorSpectrum CalcColorSpectrum()
        {
            var colors = new ColorSpectrum();
            GradientStop first = new GradientStop(new Color(), 1.0f);
            GradientStop last = new GradientStop(new Color(), 0.0f);
            foreach (var grad in Gradients)
            {
                if (grad.Offset < first.Offset) first = grad;
                if (grad.Offset > last.Offset) last = grad;
                colors.SetColorAt((float)grad.Offset, ColorUtils.MediaColorToDrawingColor(grad.Color));
            }
            colors.SetColorAt(0, ColorUtils.MediaColorToDrawingColor(first.Color));
            colors.SetColorAt(1, ColorUtils.MediaColorToDrawingColor(last.Color));
            return colors;
        }
        internal void SetBrush()
        {
            if (!_UpdateBrush)
                return;

            this._BrushSetInternally = true;


            switch (BrushType)
            {
                //case BrushTypes.None: Brush = null; break;

                case BrushTypes.Solid:
                    
                    Brush = new SolidEffectBrush(new SolidColorBrush(this.Color));

                    break;

                case BrushTypes.Linear:
                    var brush = new LinearEffectBrush(CalcColorSpectrum());
                    brush.SampleWindowSize = this.SampleWindowSize;
                    brush.Angle = Angle;
                    Brush = new LinearEffectBrush(brush);

                    break;

                
                case BrushTypes.Radial:

                    var brush1 = new RadialEffectBrush(CalcColorSpectrum());
                    brush1.SampleWindowSize = this.SampleWindowSize;
                    brush1.Center = new System.Drawing.PointF((float)CenterX, (float)CenterY);
                    Brush = (RadialEffectBrush)brush1.Clone();

                    break;
            }
            /*
            if (this.BrushType != BrushTypes.None)
            {
                this.Brush.Opacity = opacity;  // retain old opacity
                if (tempTG != null)
                    this.Brush.Transform = tempTG;
            }
            */

            this._BrushSetInternally = false;
        }
    }    
}
