using Aurora.Settings.Keycaps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public double? line_break;
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

        public VirtualLight(string text, LEDINT tag, bool? enabled = true, double? linebreak = null, double? fontsize = 12, double? margin_left = 7, double? margin_top = 0, double? width = 30, double? height = 30, int? width_bits = 2, int? height_bits = 2, int? margin_left_bits = 0, int? margin_top_bits = 0)
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

        public bool hasLineBreak()
        {
            return ((this.line_break ?? -1) > 0);
        }
    }

    public class VirtualGroup
    {
        public DeviceLayout parent;

        public string group_tag;

        public VirtualRegion origin_region;

        [JsonProperty("key_conversion")]
        public Dictionary<LEDINT, LEDINT> KeyConversion = null;

        public List<VirtualLight> grouped_keys = new List<VirtualLight>();

        //probably redundant
        public Dictionary<LEDINT, string> KeyText = new Dictionary<LEDINT, string>();

        private RectangleF _region = new RectangleF(0, 0, 0, 0);

        public RectangleF Region { get { return _region; } }

        private Rectangle _region_bitmap = new Rectangle(0, 0, 0, 0);

        public Rectangle BitmapRegion { get { return _region_bitmap; } }

        public Dictionary<LEDINT, BitmapRectangle> BitmapMap = new Dictionary<LEDINT, BitmapRectangle>();

        protected Dictionary<LEDINT, IKeycap> virtualKeyboardMap = new Dictionary<LEDINT, IKeycap>();

        /*protected Grid virtualLayout;

        public Grid VirtualLayout
        {
            get
            {
                if (virtualLayout == null)
                    CreateUserControl();
                return virtualLayout;
            }
        }*/

        public VirtualGroup()
        {

        }

        public VirtualGroup(DeviceLayout layout)
        {
            this.parent = layout;
        }

        public VirtualGroup(DeviceLayout layout, VirtualLight[] keys) : this(layout)
        {
            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            double max_absolute_height = 0;
            double max_absolute_width = 0;

            foreach (var key in keys)
            {
                grouped_keys.Add(key);
                KeyText.Add(key.tag, key.visualName);

                if (key.absolute_location ?? false)
                {
                    max_absolute_height = Math.Max(max_absolute_height, (key.margin_top ?? 0) + (key.height ?? 0));
                    max_absolute_width = Math.Max(max_absolute_width, (key.margin_left ?? 0) + (key.width ?? 0));
                    continue;
                }

                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.hasLineBreak())
                {
                    current_height += key.line_break.Value;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;
            }

            

            _region.Width = (float)Math.Max(layout_width, max_absolute_width);
            _region.Height = (float)Math.Max(layout_height, max_absolute_height);
        }

        public void AddFeature(VirtualLight[] keys, VirtualRegion insertion_region = VirtualRegion.TopLeft)
        {
            double location_x = 0.0D;
            double location_y = 0.0D;

            if (insertion_region == VirtualRegion.TopRight)
            {
                location_x = _region.Width;
            }
            else if (insertion_region == VirtualRegion.BottomLeft)
            {
                location_y = _region.Height;
            }
            else if (insertion_region == VirtualRegion.BottomRight)
            {
                location_x = _region.Width;
                location_y = _region.Height;
            }

            float added_width = 0.0f;
            float added_height = 0.0f;

            foreach (var key in keys)
            {
                key.margin_left += location_x;
                key.margin_top += location_y;

                grouped_keys.Add(key);
                if (KeyText.ContainsKey(key.tag))
                    KeyText.Remove(key.tag);
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
            }

            NormalizeKeys();
        }

        internal void UpdateColors(DeviceColorComposition deviceColours)
        {
            foreach (var clr in deviceColours.deviceColours)
            {
                this.virtualKeyboardMap[clr.Key].SetColor(clr.Value);
            }
        }

        private void NormalizeKeys()
        {
            double x_correction = 0.0D;
            double y_correction = 0.0D;

            foreach (var key in grouped_keys)
            {
                if (!key.absolute_location.Value)
                    continue;

                if (key.margin_left < x_correction)
                    x_correction = key.margin_left.Value;

                if (key.margin_top < y_correction)
                    y_correction = key.margin_top.Value;
            }

            if (grouped_keys.Count > 0)
            {
                grouped_keys[0].margin_top -= y_correction;

                bool previous_linebreak = true;
                foreach (var key in grouped_keys)
                {
                    if (key.absolute_location.Value)
                    {
                        key.margin_top -= y_correction;
                        key.margin_left -= x_correction;
                    }
                    else
                    {
                        if (previous_linebreak && !key.hasLineBreak())
                        {
                            key.margin_left -= x_correction;
                        }

                        previous_linebreak = key.hasLineBreak();
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
                VirtualLight otherKey = keys[key.tag];
                if (key.tag != otherKey.tag)
                    KeyText.Remove(key.tag);
                key.UpdateFromOtherLight(otherKey);
                if (KeyText.ContainsKey(key.tag))
                    KeyText[key.tag] = key.visualName;
                else
                    KeyText.Add(key.tag, key.visualName);
            }
        }

        internal void RemoveKeys(LEDINT[] keys_to_remove)
        {
            var applicable_keys = grouped_keys.RemoveAll(key => keys_to_remove.Contains(key.tag));

            double layout_height = 0;
            double layout_width = 0;
            double current_height = 0;
            double current_width = 0;

            foreach (var key in grouped_keys)
            {
                if (key.width + key.margin_left > 0)
                    current_width += key.width.Value + key.margin_left.Value;

                if (key.margin_top > 0)
                    current_height += key.margin_top.Value;


                if (layout_width < current_width)
                    layout_width = current_width;

                if (key.hasLineBreak())
                {
                    current_height += key.line_break.Value;
                    current_width = 0;
                }

                if (layout_height < current_height)
                    layout_height = current_height;

                KeyText.Remove(key.tag);
            }

            _region.Width = (float)layout_width;
            _region.Height = (float)layout_height;
        }

        public void CalculateBitmap()
        {
            double cur_width = 0;
            double cur_height = 0;
            double width_max = 1;
            double height_max = 1;
            this.BitmapMap.Clear();

            foreach (VirtualLight key in this.grouped_keys)
            {
                if (key.tag.Equals(-1))
                    continue;

                double width = key.width.Value;
                int width_bit = LayoutUtils.PixelToByte(width);
                double height = key.height.Value;
                int height_bit = LayoutUtils.PixelToByte(height);
                double x_offset = key.margin_left.Value;
                double y_offset = key.margin_top.Value;
                double br_x, br_y;

                if (key.absolute_location.Value)
                {
                    this.BitmapMap[key.tag] = new BitmapRectangle(LayoutUtils.PixelToByte(x_offset), LayoutUtils.PixelToByte(y_offset), width_bit, height_bit);
                    br_x = (x_offset + width);
                    br_y = (y_offset + height);
                }
                else
                {
                    double x = x_offset + cur_width;
                    double y = y_offset + cur_height;

                    this.BitmapMap[key.tag] = new BitmapRectangle(LayoutUtils.PixelToByte(x), LayoutUtils.PixelToByte(y), width_bit, height_bit);

                    br_x = (x + width);
                    br_y = (y + height);

                    if (key.hasLineBreak())
                    {
                        cur_height += key.line_break.Value;
                        cur_width = 0;
                    }
                    else
                    {
                        cur_width = br_x;
                        if (y > cur_height)
                            cur_height = y;
                    }
                }
                if (br_x > width_max) width_max = br_x;
                if (br_y > height_max) height_max = br_y;
            }
            //+1 for rounding error, where the bitmap rectangle B(X)+B(Width) > B(X+Width) 
            _region_bitmap.Width = LayoutUtils.PixelToByte(this.Region.Width) + 1;
            _region_bitmap.Height = LayoutUtils.PixelToByte(this.Region.Height) + 1;
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
