﻿using Aurora.Devices;
using System;
using System.Collections.Generic;
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

namespace Aurora.Settings.DeviceLayoutViewer.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_ColorizedKeycapBlank.xaml
    /// </summary>
    public partial class Control_ColorizedKeycapBlank : KeycapViewer
    {
        private Color current_color = Color.FromArgb(0, 0, 0, 0);

        public Control_ColorizedKeycapBlank()
        {
            InitializeComponent();
        }

        public Control_ColorizedKeycapBlank(DeviceKeyConfiguration key) : base(key)
        {
            InitializeComponent();

            keyBorder.IsEnabled = key.Enabled.Value;

            if (!key.Enabled.Value)
            {
                ToolTipService.SetShowOnDisabled(keyBorder, true);
                keyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            }

            keyCap.Text = Config.Key.VisualName;
            //keyCap.Tag = associatedKey.Tag;
            if (Config.FontSize != null)
                keyCap.FontSize = Config.FontSize.Value;
        }

        public override void SetColor(Color key_color)
        {
            if (!current_color.Equals(key_color))
            {
                keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                keyBorder.BorderBrush = new SolidColorBrush(key_color);
                current_color = key_color;
            }

            if (IsSelected)
                keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)(Math.Min(Math.Pow(Math.Cos((double)(Utils.Time.GetMilliSeconds() / 1000.0) * Math.PI) + 0.05, 2.0), 1.0) * 255), (byte)0));
            else
            {
                if (keyBorder.IsEnabled)
                {

                    keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                }
                else
                {
                    keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
                    keyBorder.BorderThickness = new Thickness(0);
                }
            }
        }
    }
}
