using Aurora.Settings;
using Aurora.Settings.Keycaps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LEDINT = System.Int16;


namespace Aurora.Devices.Layout
{
    public enum VirtualRegion
    {
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }

    public class VirtualLight
    {
        public string visualName;
        public LEDINT tag;
        public bool? line_break;
        public double? margin_left;
        public double? margin_top;
        public double? width;
        public double? height;
        public double? font_size;
        public bool? enabled = true;
        public bool? absolute_location = false;
        public string image = "";

        public VirtualLight() : this("", -1)
        {
        }

        public VirtualLight(string text, LEDINT tag, bool? enabled = true, bool? linebreak = false, double? fontsize = 12, double? margin_left = 7, double? margin_top = 0, double? width = 30, double? height = 30, int? width_bits = 2, int? height_bits = 2, int? margin_left_bits = 0, int? margin_top_bits = 0)
        {
            this.visualName = text;
            this.tag = tag;
            this.line_break = linebreak;
            this.width = width;
            this.height = height;
            this.font_size = fontsize;
            this.margin_left = margin_left;
            this.margin_top = margin_top;
            this.enabled = enabled;
        }

        public VirtualLight UpdateFromOtherLight(VirtualLight otherKey)
        {
            if (otherKey != null)
            {
                if (otherKey.visualName != null) this.visualName = otherKey.visualName;
                this.tag = otherKey.tag;
                if (otherKey.line_break != null) this.line_break = otherKey.line_break;
                if (otherKey.width != null) this.width = otherKey.width;
                if (otherKey.height != null) this.height = otherKey.height;
                if (otherKey.font_size != null) this.font_size = otherKey.font_size;
                if (otherKey.margin_left != null) this.margin_left = otherKey.margin_left;
                if (otherKey.margin_top != null) this.margin_top = otherKey.margin_top;
                if (otherKey.enabled != null) this.enabled = otherKey.enabled;
            }
            return this;
        }
    }

    public class VirtualGroup
    {
        public string group_tag;

        public VirtualRegion origin_region;

        public List<VirtualLight> grouped_keys = new List<VirtualLight>();

        //probably redundant
        public Dictionary<LEDINT, string> KeyText = new Dictionary<LEDINT, string>();

        private RectangleF _region = new RectangleF(0, 0, 0, 0);

        public RectangleF Region { get { return _region; } }

        private Rectangle _region_bitmap = new Rectangle(0, 0, 0, 0);

        public Rectangle BitmapRegion { get { return _region_bitmap; } }

        public Dictionary<LEDINT, BitmapRectangle> BitmapMap = new Dictionary<LEDINT, BitmapRectangle>();

        protected Dictionary<LEDINT, IKeycap> virtualKeyboardMap = new Dictionary<LEDINT, IKeycap>();

        protected Grid virtualLayout;

        public Grid VirtualLayout
        {
            get
            {
                return virtualLayout;
            }
        }

        public VirtualGroup()
        {

        }

        public VirtualGroup(VirtualLight[] keys)
        {
            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 1;
            int height_bit_max = 1;

            foreach (var key in keys)
            {
                grouped_keys.Add(key);
                KeyText.Add(key.tag, key.visualName);

                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.line_break.Value)
                {
                    current_height += 37;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;


                int key_tly = LayoutUtils.PixelToByte(key.margin_top.Value) + height_bit;
                int key_tlx = LayoutUtils.PixelToByte(key.margin_left.Value) + width_bit;

                int key_bry = key_tly + LayoutUtils.PixelToByte(key.height.Value);
                int key_brx = key_tlx + LayoutUtils.PixelToByte(key.width.Value);

                if (width_bit_max < key_brx) width_bit_max = key_brx;
                if (height_bit_max < key_bry) height_bit_max = key_bry;


                if (key.line_break.Value)
                {
                    height_bit += 3;
                    width_bit = 0;
                }
                else
                {
                    width_bit = key_brx;
                    height_bit = key_tly;
                }

            }

            _region.Width = (float)layout_width;
            _region.Height = (float)layout_height;

            _region_bitmap.Width = width_bit_max;
            _region_bitmap.Height = height_bit_max;

            //NormalizeKeys();
        }

        public void AddFeature(VirtualLight[] keys, VirtualRegion insertion_region = VirtualRegion.TopLeft)
        {
            double location_x = 0.0D;
            double location_y = 0.0D;
            int location_x_bit = 0;
            int location_y_bit = 0;

            if (insertion_region == VirtualRegion.TopRight)
            {
                location_x = _region.Width;
                location_x_bit = _region_bitmap.Width;
            }
            else if (insertion_region == VirtualRegion.BottomLeft)
            {
                location_y = _region.Height;
                location_y_bit = _region_bitmap.Height;
            }
            else if (insertion_region == VirtualRegion.BottomRight)
            {
                location_x = _region.Width;
                location_y = _region.Height;
                location_x_bit = _region_bitmap.Width;
                location_y_bit = _region_bitmap.Height;
            }

            float added_width = 0.0f;
            float added_height = 0.0f;
            int added_width_bits = 0;
            int added_height_bits = 0;

            foreach (var key in keys)
            {
                key.margin_left += location_x;
                key.margin_top += location_y;

                //key.margin_left_bits += location_x_bit;
                //key.margin_top_bits += location_y_bit;

                grouped_keys.Add(key);
                KeyText.Add(key.tag, key.visualName);

                if (key.width + key.margin_left > _region.Width)
                    _region.Width += (float)(key.width + key.margin_left - location_x);
                else if (key.margin_left + added_width < 0)
                {
                    added_width = -(float)(key.margin_left);
                    _region.Width -= (float)(key.margin_left);
                }

                if (key.height + key.margin_top > _region.Height)
                    _region.Height += (float)(key.height + key.margin_top - location_y);
                else if (key.margin_top + added_height < 0)
                {
                    added_height = -(float)(key.margin_top);
                    _region.Height -= (float)(key.margin_top);
                }


                if (LayoutUtils.PixelToByte(key.width.Value) + LayoutUtils.PixelToByte(key.margin_left.Value) > _region_bitmap.Width)
                    _region_bitmap.Width += LayoutUtils.PixelToByte(key.width.Value) + LayoutUtils.PixelToByte(key.margin_left.Value) - location_x_bit;
                else if (LayoutUtils.PixelToByte(key.margin_left.Value) + added_width_bits < 0)
                {
                    added_width_bits = -LayoutUtils.PixelToByte(key.margin_left.Value);
                    _region_bitmap.Width -= LayoutUtils.PixelToByte(key.margin_left.Value);
                }

                if (LayoutUtils.PixelToByte(key.height.Value) + LayoutUtils.PixelToByte(key.margin_top.Value) > _region_bitmap.Height)
                    _region_bitmap.Height += LayoutUtils.PixelToByte(key.height.Value) + LayoutUtils.PixelToByte(key.margin_top.Value) - location_y_bit;
                else if (LayoutUtils.PixelToByte(key.margin_top.Value) + added_height_bits < 0)
                {
                    added_height_bits = -LayoutUtils.PixelToByte(key.margin_top.Value);
                    _region_bitmap.Height -= LayoutUtils.PixelToByte(key.margin_top.Value);
                }

            }

            NormalizeKeys();
        }

        private void NormalizeKeys()
        {
            double x_correction = 0.0D;
            double y_correction = 0.0D;

            //int x_correction_bit = 0;
            //int y_correction_bit = 0;

            foreach (var key in grouped_keys)
            {
                if (!key.absolute_location.Value)
                    continue;

                if (key.margin_left < x_correction)
                    x_correction = key.margin_left.Value;

                if (key.margin_top < y_correction)
                    y_correction = key.margin_top.Value;

                /*if (key.margin_left_bits < x_correction_bit)
                    x_correction_bit = key.margin_left_bits.Value;

                if (key.margin_top_bits < y_correction_bit)
                    y_correction_bit = key.margin_top_bits.Value;*/
            }

            if (grouped_keys.Count > 0)
            {
                grouped_keys[0].margin_top -= y_correction;
                //grouped_keys[0].margin_top_bits -= y_correction_bit;

                bool previous_linebreak = true;
                foreach (var key in grouped_keys)
                {
                    if (key.absolute_location.Value)
                    {
                        key.margin_top -= y_correction;
                        key.margin_left -= x_correction;
                        /*key.margin_top_bits -= y_correction_bit;
                        key.margin_left_bits -= x_correction_bit;*/
                    }
                    else
                    {
                        if (previous_linebreak && !key.line_break.Value)
                        {
                            key.margin_left -= x_correction;
                            //key.margin_left_bits -= x_correction_bit;
                        }

                        previous_linebreak = key.line_break.Value;
                    }
                }

            }
        }

        public void Clear()
        {
            _region = new RectangleF(0, 0, 0, 0);
            _region_bitmap = new Rectangle(0, 0, 0, 0);
            BitmapMap.Clear();
            grouped_keys.Clear();
        }

        internal void AdjustKeys(Dictionary<LEDINT, VirtualLight> keys)
        {
            var applicable_keys = grouped_keys.FindAll(key => keys.ContainsKey(key.tag));

            foreach (var key in applicable_keys)
            {
                if (KeyText.ContainsKey(key.tag))
                    KeyText[key.tag] = key.visualName;
                key.UpdateFromOtherLight(keys[key.tag]);
            }
        }

        internal void RemoveKeys(LEDINT[] keys_to_remove)
        {
            var applicable_keys = grouped_keys.RemoveAll(key => keys_to_remove.Contains(key.tag));

            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 1;
            int height_bit_max = 1;

            foreach (var key in grouped_keys)
            {
                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.line_break.Value)
                {
                    current_height += 37;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;


                int key_tly = LayoutUtils.PixelToByte(key.margin_top.Value) + height_bit;
                int key_tlx = LayoutUtils.PixelToByte(key.margin_left.Value) + width_bit;

                int key_bry = key_tly + LayoutUtils.PixelToByte(key.height.Value);
                int key_brx = key_tlx + LayoutUtils.PixelToByte(key.width.Value);

                if (width_bit_max < key_brx) width_bit_max = key_brx;
                if (height_bit_max < key_bry) height_bit_max = key_bry;


                if (key.line_break.Value)
                {
                    height_bit += 3;
                    width_bit = 0;
                }
                else
                {
                    width_bit = key_brx;
                    height_bit = key_tly;
                }

            }

            _region.Width = (float)layout_width;
            _region.Height = (float)layout_height;

            _region_bitmap.Width = width_bit_max;
            _region_bitmap.Height = height_bit_max;

        }

        public void CalculateBitmap()
        {
            int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 1;
            int height_bit_max = 1;
            BitmapMap.Clear();

            foreach (VirtualLight key in this.grouped_keys)
            {
                int key_tly = LayoutUtils.PixelToByte(key.margin_top.Value) + height_bit;
                int key_tlx = LayoutUtils.PixelToByte(key.margin_left.Value) + width_bit;

                int key_bry = key_tly + LayoutUtils.PixelToByte(key.height.Value);
                int key_brx = key_tlx + LayoutUtils.PixelToByte(key.width.Value);

                if (key.absolute_location.Value)
                    this.BitmapMap[key.tag] = new BitmapRectangle(LayoutUtils.PixelToByte(key.margin_left.Value), LayoutUtils.PixelToByte(key.margin_top.Value), key_brx - key_tlx, key_bry - key_tly);
                else
                    this.BitmapMap[key.tag] = new BitmapRectangle(key_tlx, key_tly, key_brx - key_tlx, key_bry - key_tly);

                if (!key.absolute_location.Value)
                {
                    if (width_bit_max < key_brx) width_bit_max = key_brx;
                    if (height_bit_max < key_bry) height_bit_max = key_bry;


                    if (key.line_break.Value)
                    {
                        //TODO: rework so it isn't fixed
                        height_bit += LayoutUtils.PixelToByte(37);
                        width_bit = 0;
                    }
                    else
                    {
                        width_bit = key_brx;
                        if (key_tly > height_bit)
                            height_bit = key_tly;
                    }
                }
            }

        }

        private Grid CreateUserControl(bool abstractKeycaps = false)
        {
            //Abstract keycaps are used for stuff like the animation editor
            if (!abstractKeycaps)
                virtualKeyboardMap.Clear();

            Grid new_virtual_keyboard = new Grid();

            new_virtual_keyboard.HorizontalAlignment = HorizontalAlignment.Left;
            new_virtual_keyboard.VerticalAlignment = VerticalAlignment.Top;

            //Bind Margin
            Binding bind = new Binding();
            bind.Source = this;
            bind.Path = new System.Windows.PropertyPath("Location");
            bind.Mode = abstractKeycaps ? BindingMode.OneWay : BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bind.Converter = new PointToMarginConverter();
            BindingOperations.SetBinding(new_virtual_keyboard, Grid.MarginProperty, bind);

            double layout_height = 0;
            double layout_width = 0;

            double current_height = 0;
            double current_width = 0;

            string images_path = Path.Combine(layoutsPath, "Extra Features", "images");

            foreach (VirtualLight key in this.grouped_keys)
            {
                double keyMargin_Left = key.margin_left.Value;
                double keyMargin_Top = key.margin_top.Value;

                string image_path = "";

                if (!string.IsNullOrWhiteSpace(key.image))
                    image_path = Path.Combine(images_path, key.image);

                UserControl keycap;

                //Ghost keycap is used for abstract representation of keys
                if (abstractKeycaps)
                    keycap = new Control_GhostKeycap(this, key, image_path);
                else
                {
                    switch (Global.Configuration.virtualkeyboard_keycap_type)
                    {
                        case KeycapType.Default_backglow:
                            keycap = new Control_DefaultKeycapBackglow(this, key, image_path);
                            break;
                        case KeycapType.Default_backglow_only:
                            keycap = new Control_DefaultKeycapBackglowOnly(this, key, image_path);
                            break;
                        case KeycapType.Colorized:
                            keycap = new Control_ColorizedKeycap(this, key, image_path);
                            break;
                        case KeycapType.Colorized_blank:
                            keycap = new Control_ColorizedKeycapBlank(this, key, image_path);
                            break;
                        default:
                            keycap = new Control_DefaultKeycap(this, key, image_path);
                            break;
                    }
                }

                new_virtual_keyboard.Children.Add(keycap);

                if (key.tag != -1 && !virtualKeyboardMap.ContainsKey(key.tag) && keycap is IKeycap && !abstractKeycaps)
                    virtualKeyboardMap.Add(key.tag, keycap as IKeycap);

                if (key.absolute_location.Value)
                    keycap.Margin = new Thickness(key.margin_left.Value, key.margin_top.Value, 0, 0);
                else
                    keycap.Margin = new Thickness(current_width + key.margin_left.Value, current_height + key.margin_top.Value, 0, 0);

                if (!key.absolute_location.Value)
                {
                    if (key.width + keyMargin_Left > 0)
                        current_width += key.width.Value + keyMargin_Left;

                    if (keyMargin_Top > 0)
                        current_height += keyMargin_Top;


                    if (layout_width < current_width)
                        layout_width = current_width;

                    if (key.line_break.Value)
                    {
                        current_height += 37;
                        current_width = 0;
                        //isFirstInRow = true;
                    }

                    if (layout_height < current_height)
                        layout_height = current_height;
                }
            }

            //TODO: DEAL WITH THIS
            if (this.grouped_keys.Count == 0)
            {
                //No items, display error
                Label error_message = new Label();

                DockPanel info_panel = new DockPanel();

                TextBlock info_message = new TextBlock()
                {
                    Text = "No keyboard selected\r\nPlease select your keyboard in the settings",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                };

                DockPanel.SetDock(info_message, Dock.Top);
                info_panel.Children.Add(info_message);

                DockPanel info_instruction = new DockPanel();

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = "Press (",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(@"Resources/settings_icon.png", UriKind.Relative)),
                    Stretch = Stretch.Uniform,
                    Height = 40.0,
                    VerticalAlignment = VerticalAlignment.Center
                });

                info_instruction.Children.Add(new TextBlock()
                {
                    Text = ") and go into \"Devices & Wrappers\" tab",
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                DockPanel.SetDock(info_instruction, Dock.Bottom);
                info_panel.Children.Add(info_instruction);

                error_message.Content = info_panel;

                error_message.FontSize = 16.0;
                error_message.FontWeight = FontWeights.Bold;
                error_message.HorizontalContentAlignment = HorizontalAlignment.Center;
                error_message.VerticalContentAlignment = VerticalAlignment.Center;

                new_virtual_keyboard.Children.Add(error_message);

                //Update size
                new_virtual_keyboard.Width = 850;
                new_virtual_keyboard.Height = 200;
            }
            else
            {
                //Update size
                new_virtual_keyboard.Width = this.Region.Width;
                new_virtual_keyboard.Height = this.Region.Height;
            }

            if (!abstractKeycaps)
            {
                virtualLayout.Children.Clear();
                virtualLayout = new_virtual_keyboard;
            }

            return new_virtual_keyboard;
        }
    }

    public class VirtualGroupConfiguration
    {
        public LEDINT[] keys_to_remove = new LEDINT[] { };

        public Dictionary<LEDINT, VirtualLight> key_modifications = new Dictionary<LEDINT, VirtualLight>();

        [JsonProperty("key_conversion")]
        public Dictionary<LEDINT, LEDINT> KeyConversion = null;

        /// <summary>
        /// A list of paths for each included group json
        /// </summary>
        public string[] included_features = new string[] { };

        public VirtualGroupConfiguration()
        {

        }
    }
}
