using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_ProfileManager.xaml
    /// </summary>
    public partial class Control_ProfileManager : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register("ProfileManager", typeof(ProfileManager), typeof(UserControl));

        public ProfileManager ProfileManager
        {
            get
            {
                return (ProfileManager)GetValue(ProfileManagerProperty);
            }
            set
            {
                SetValue(ProfileManagerProperty, value);

                this.profiles_combobox.Items.Clear();
                foreach (var kvp in value.Profiles)
                    this.profiles_combobox.Items.Add(kvp.Key);

                this.load_profile_button.IsEnabled = value.Profiles.Count > 0;
            }
        }

        public Control_ProfileManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void load_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if (this.profiles_combobox.SelectedItem != null && this.profiles_combobox.SelectedItem is string && !string.IsNullOrWhiteSpace(this.profiles_combobox.SelectedItem as string))
                {
                    ProfileManager.SwitchToProfile(this.profiles_combobox.SelectedItem as string);
                }
                else
                {
                    MessageBox.Show("Please either select an existing profile from the dropbox.");
                }

            }
        }

        private void save_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if (this.profiles_combobox.Text != null && !string.IsNullOrWhiteSpace(this.profiles_combobox.Text))
                {
                    ProfileManager.SaveDefaultProfile(this.profiles_combobox.Text as string);

                    this.profiles_combobox.Items.Clear();
                    foreach (var kvp in ProfileManager.Profiles)
                        this.profiles_combobox.Items.Add(kvp.Key);

                    this.load_profile_button.IsEnabled = ProfileManager.Profiles.Count > 0;
                }
                else
                {
                    MessageBox.Show("Please either select an existing profile or\r\ntype a new profile name in the dropbox.");
                }
            }
        }

        private void reset_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                ProfileManager.ResetProfile();

                this.profiles_combobox.Items.Clear();
                foreach (var kvp in ProfileManager.Profiles)
                    this.profiles_combobox.Items.Add(kvp.Key);

                this.load_profile_button.IsEnabled = ProfileManager.Profiles.Count > 0;
            }
        }

        private void view_folder_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                System.Diagnostics.Process.Start(ProfileManager.GetProfileFolderPath());
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void import_profile_button_Click(object sender, RoutedEventArgs e)
        {
            XElement rootElement = XElement.Load("medic.cueprofile");

            //var elements = rootElement.Elements();

            XElement value0Element = rootElement.Element("value0");

            XElement profileElement = value0Element.Element("profile");

            string profileName = profileElement.Element("name").Value;

            foreach (XElement property in profileElement.Element("properties").Elements())
            {
                if ("Keyboard".Equals(property.Element("key").Value))
                {
                    foreach (XElement profProperty in property.Element("value").Element("properties").Elements())
                    {
                        var polyName = profProperty.Element("polymorphic_name");

                        if (polyName != null && "AdvancedLightingsProperty::Proxy".Equals(polyName.Value))
                        {
                            var layers = profProperty.Element("ptr_wrapper").Element("data").Element("properties").Element("value1").Element("value").Element("ptr_wrapper").Element("data").Element("base").Element("layers");

                            ProfileManager.Settings.Layers.Clear();

                            foreach (XElement layer in layers.Elements())
                            {
                                var keysAndStuff = layer.Element("ptr_wrapper").Element("data").Element("base");

                                string layerName = keysAndStuff.Element("name").Value;
                                bool layerEnabled = bool.Parse(keysAndStuff.Element("enabled").Value);
                                KeySequence affected_keys = new KeySequence();

                                foreach (XElement key in keysAndStuff.Element("keys").Elements())
                                {
                                    try
                                    {
                                        CUE.NET.Devices.Generic.Enums.CorsairLedId keyValue;

                                        switch (key.Value)
                                        {
                                            case "0":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D0;
                                                break;
                                            case "1":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D1;
                                                break;
                                            case "2":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D2;
                                                break;
                                            case "3":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D3;
                                                break;
                                            case "4":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D4;
                                                break;
                                            case "5":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D5;
                                                break;
                                            case "6":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D6;
                                                break;
                                            case "7":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D7;
                                                break;
                                            case "8":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D8;
                                                break;
                                            case "9":
                                                keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.D9;
                                                break;
                                            default:
                                                keyValue = (CUE.NET.Devices.Generic.Enums.CorsairLedId)Enum.Parse(typeof(CUE.NET.Devices.Generic.Enums.CorsairLedId), key.Value);
                                                break;
                                        }
                                         
                                        if (Enum.IsDefined(typeof(CUE.NET.Devices.Generic.Enums.CorsairLedId), keyValue) | keyValue.ToString().Contains(","))
                                        {
                                            Devices.DeviceKeys deviceKey = Utils.KeyUtils.ToDeviceKeys(keyValue);

                                            if (deviceKey != Devices.DeviceKeys.NONE)
                                                affected_keys.keys.Add(deviceKey);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }

                                var lightingInfo = layer.Element("ptr_wrapper").Element("data").Element("lighting");

                                var layerPolyId = lightingInfo.Element("polymorphic_id");

                                if (layerPolyId != null && ("30".Equals(layerPolyId.Value) || "2147483678".Equals(layerPolyId.Value)))
                                {
                                    ProfileManager.Settings.Layers.Add(new Layers.Layer()
                                    {
                                        Name = layerName,
                                        Enabled = layerEnabled,
                                        Type = Layers.LayerType.Solid,
                                        Handler = new Layers.SolidColorLayerHandler()
                                        {
                                            Properties = new Layers.LayerHandlerProperties()
                                            {
                                                _Sequence = affected_keys,
                                                _PrimaryColor = System.Drawing.ColorTranslator.FromHtml(lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions").Element("value0").Element("color").Value)
                                            },
                                            Opacity = int.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("opacity").Value) / 255.0f
                                        }
                                    });
                                }
                            }

                            break;
                        }
                    }

                    break;
                }
            }


            Global.logger.LogLine(rootElement.ToString());
        }
    }
}
