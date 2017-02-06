using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".cueprofile";
            dlg.Filter = "CUE Profile Files (*.cueprofile)|*.cueprofile";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            string filePath = "";

            // Get the selected file name and display in a TextBox 
            if (result == true)
                filePath = dlg.FileName;


            if (filePath.EndsWith(".cueprofile"))
            {
                XElement rootElement = XElement.Load(filePath);

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
                            var polyID = profProperty.Element("polymorphic_id");


                            if ((polyName != null && "AdvancedLightingsProperty::Proxy".Equals(polyName.Value)) ||
                                (polyID != null && "14".Equals(polyID.Value))
                                )
                            {
                                var layers = profProperty.Element("ptr_wrapper").Element("data").Element("properties").Element("value1").Element("value").Element("ptr_wrapper").Element("data").Element("base").Element("layers");

                                ProfileManager.Settings.Layers.Clear();

                                uint _basePolyID = 2147483648;
                                Dictionary<uint, string> _definedPolyIDS = new Dictionary<uint, string>();

                                foreach (XElement layer in layers.Elements())
                                {
                                    var keysAndStuff = layer.Element("ptr_wrapper").Element("data").Element("base");

                                    string layerName = keysAndStuff.Element("name").Value;
                                    bool layerEnabled = bool.Parse(keysAndStuff.Element("enabled").Value);
                                    int repeatTimes = Math.Max(int.Parse(keysAndStuff.Element("executionHints").Element("stopAfterTimes").Value), 0);
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
                                                case "Led_KeyboardLogo":
                                                    keyValue = CUE.NET.Devices.Generic.Enums.CorsairLedId.Logo;
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
                                    var layerPolyName = lightingInfo.Element("polymorphic_name")?.Value;

                                    if (String.IsNullOrWhiteSpace(layerPolyName))
                                    {
                                        if (_definedPolyIDS.ContainsKey(uint.Parse(layerPolyId.Value)))
                                            layerPolyName = _definedPolyIDS[uint.Parse(layerPolyId.Value)];
                                    }
                                    else
                                        _definedPolyIDS.Add(uint.Parse(layerPolyId.Value) - _basePolyID, layerPolyName);


                                    if ("StaticLighting".Equals(layerPolyName))
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
                                    else if ("GradientLighting".Equals(layerPolyName))
                                    {
                                        float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);
                                        AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                        Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                        foreach (XElement transition in lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions").Elements())
                                        {
                                            try
                                            {
                                                float time = float.Parse(transition.Element("time").Value);
                                                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                transitions.Add(time * (duration / 1000.0f), color);
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }

                                        for (int x = 0; x + 1 < transitions.Count; x += 2)
                                        {
                                            float transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                            animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration));
                                        }

                                        ProfileManager.Settings.Layers.Add(new Layers.Layer()
                                        {
                                            Name = layerName,
                                            Enabled = layerEnabled,
                                            Type = Layers.LayerType.Animation,
                                            Handler = new Layers.AnimationLayerHandler()
                                            {
                                                Properties = new Layers.AnimationLayerHandlerProperties()
                                                {
                                                    _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                    _Sequence = affected_keys,
                                                    _forceKeySequence = true,
                                                    _AnimationDuration = (duration / 1000.0f),
                                                    _AnimationRepeat = repeatTimes
                                                }
                                            }
                                        });
                                    }
                                    else if ("SolidLighting".Equals(layerPolyName))
                                    {
                                        float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);
                                        AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                        Dictionary<float, System.Drawing.Color> transitions = new Dictionary<float, System.Drawing.Color>();

                                        foreach (XElement transition in lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions").Elements())
                                        {
                                            try
                                            {
                                                float time = float.Parse(transition.Element("time").Value);
                                                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                if (transitions.ContainsKey(time * (duration / 1000.0f)))
                                                    transitions.Add(time * (duration / 1000.0f) + 0.000001f, color);
                                                else
                                                    transitions.Add(time * (duration / 1000.0f), color);
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }

                                        for (int x = 0; x < transitions.Count; x += 2)
                                        {
                                            float transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                            animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration).SetTransitionType(AnimationFrameTransitionType.None));
                                        }

                                        ProfileManager.Settings.Layers.Add(new Layers.Layer()
                                        {
                                            Name = layerName,
                                            Enabled = layerEnabled,
                                            Type = Layers.LayerType.Animation,
                                            Handler = new Layers.AnimationLayerHandler()
                                            {
                                                Properties = new Layers.AnimationLayerHandlerProperties()
                                                {
                                                    _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                    _Sequence = affected_keys,
                                                    _forceKeySequence = true,
                                                    _AnimationDuration = (duration / 1000.0f),
                                                    _AnimationRepeat = repeatTimes
                                                }
                                            }
                                        });
                                    }
                                    else if ("WaveLighting".Equals(layerPolyName))
                                    {
                                        float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);

                                        List<AnimationTrack> animTracks = new List<AnimationTrack>();

                                        EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                        float smallest = 0.5f;
                                        float largest = 0.5f;

                                        foreach (XElement transition in lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions").Elements())
                                        {
                                            try
                                            {
                                                float time = float.Parse(transition.Element("time").Value);
                                                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                transitions.SetColorAt(time, color);

                                                if (time < smallest)
                                                    smallest = time;
                                                else if (time > largest)
                                                    largest = time;
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }

                                        if (smallest > 0.0f)
                                        {
                                            transitions.SetColorAt(0.0f, System.Drawing.Color.Transparent);
                                            transitions.SetColorAt(smallest - 0.001f, System.Drawing.Color.Transparent);
                                        }

                                        if (largest < 1.0f)
                                        {
                                            transitions.SetColorAt(1.0f, System.Drawing.Color.Transparent);
                                            transitions.SetColorAt(largest + 0.001f, System.Drawing.Color.Transparent);
                                        }

                                        transitions.Flip();

                                        float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value) / 10.0f;
                                        float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value) / 10.0f;
                                        bool isDoubleSided = bool.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("isDoublesided").Value);
                                        float angle = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("angle").Value);

                                        width *= 3.0f;

                                        angle %= 360; //Get angle within our range

                                        if (!isDoubleSided)
                                        {
                                            AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                            float terminalTime = Effects.canvas_width / (velocity * (3.0f * 0.7f));

                                            if (angle >= 315 || angle <= 45)
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width + width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                            }
                                            else if (angle > 45 && angle < 135)
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, Effects.canvas_height + width / 2, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(0, (Effects.canvas_height + width / 2) - (Effects.canvas_width + width), width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                            }
                                            else if (angle >= 135 && angle <= 225)
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width + width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                            }
                                            else if (angle > 225 && angle < 315)
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(0, -width / 2, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(0, (-width / 2) + (Effects.canvas_width + width), width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                            }

                                            animTracks.Add(animTrack);
                                        }
                                        else
                                        {
                                            AnimationTrack animTrack = new AnimationTrack(layerName + " - Side 1", duration / 1000.0f);
                                            AnimationTrack animTrack2 = new AnimationTrack(layerName + " - Side 2", duration / 1000.0f);

                                            float widthTime = width / (velocity * (3.0f * 0.7f)) / 2;
                                            float terminalTime = Effects.canvas_width / (velocity * (3.0f * 0.7f)) / 2;

                                            if ((angle >= 315 || angle <= 45) || (angle >= 135 && angle <= 225))
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, 0, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(widthTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width + width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, 0, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));

                                                animTrack2.SetFrame(widthTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));

                                                animTrack2.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-width, 0, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));
                                            }
                                            else if ((angle > 45 && angle < 135) || (angle > 225 && angle < 315))
                                            {
                                                animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(widthTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, Effects.canvas_height_center, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, (Effects.canvas_height + width / 2) - (Effects.canvas_width + width), width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));

                                                animTrack2.SetFrame(widthTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, Effects.canvas_height_center, width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));

                                                animTrack2.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, (-width / 2) + (Effects.canvas_width + width), width, Effects.canvas_biggest * 2, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle + 180));
                                            }

                                            animTracks.Add(animTrack);
                                            animTracks.Add(animTrack2);
                                        }

                                        ProfileManager.Settings.Layers.Add(new Layers.Layer()
                                        {
                                            Name = layerName,
                                            Enabled = layerEnabled,
                                            Type = Layers.LayerType.Animation,
                                            Handler = new Layers.AnimationLayerHandler()
                                            {
                                                Properties = new Layers.AnimationLayerHandlerProperties()
                                                {
                                                    _AnimationMix = new AnimationMix(animTracks.ToArray()),
                                                    _Sequence = affected_keys,
                                                    _forceKeySequence = true,
                                                    _AnimationDuration = (duration / 1000.0f),
                                                    _AnimationRepeat = repeatTimes,
                                                    _scaleToKeySequenceBounds = true
                                                }
                                            }
                                        });
                                    }
                                    else if ("RippleLighting".Equals(layerPolyName))
                                    {
                                        float duration = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("base").Element("duration").Value);

                                        EffectsEngine.ColorSpectrum transitions = new EffectsEngine.ColorSpectrum();

                                        float smallest = 0.5f;
                                        float largest = 0.5f;

                                        foreach (XElement transition in lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions").Elements())
                                        {
                                            try
                                            {
                                                float time = float.Parse(transition.Element("time").Value);
                                                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(transition.Element("color").Value);

                                                transitions.SetColorAt(time, color);

                                                if (time < smallest)
                                                    smallest = time;
                                                else if (time > largest)
                                                    largest = time;
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }

                                        if (smallest > 0.0f)
                                        {
                                            transitions.SetColorAt(0.0f, System.Drawing.Color.Transparent);
                                            transitions.SetColorAt(smallest - 0.001f, System.Drawing.Color.Transparent);
                                        }

                                        if (largest < 1.0f)
                                        {
                                            transitions.SetColorAt(1.0f, System.Drawing.Color.Transparent);
                                            transitions.SetColorAt(largest + 0.001f, System.Drawing.Color.Transparent);
                                        }

                                        transitions.Flip();

                                        float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value) / 10.0f;
                                        float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value) / 10.0f;

                                        width *= 3.0f;

                                        AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                        float terminalTime = Effects.canvas_width / (velocity * (3.0f * 0.7f)) / 2;

                                        animTrack.SetFrame(0.0f, new AnimationGradientCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));

                                        animTrack.SetFrame(terminalTime, new AnimationGradientCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, new EffectsEngine.EffectBrush(transitions).SetBrushType(EffectsEngine.EffectBrush.BrushType.Radial), (int)width));

                                        ProfileManager.Settings.Layers.Add(new Layers.Layer()
                                        {
                                            Name = layerName,
                                            Enabled = layerEnabled,
                                            Type = Layers.LayerType.Animation,
                                            Handler = new Layers.AnimationLayerHandler()
                                            {
                                                Properties = new Layers.AnimationLayerHandlerProperties()
                                                {
                                                    _AnimationMix = new AnimationMix().AddTrack(animTrack),
                                                    _Sequence = affected_keys,
                                                    _forceKeySequence = true,
                                                    _AnimationDuration = (duration / 1000.0f),
                                                    _AnimationRepeat = repeatTimes,
                                                    _scaleToKeySequenceBounds = true
                                                }
                                            }
                                        });
                                    }
                                    else
                                    {
                                        //Null, it's unknown.
                                        Global.logger.LogLine("Unknown CUE Layer Type");
                                    }
                                }

                                break;
                            }
                        }

                        break;
                    }
                }
            }

            //Global.logger.LogLine(rootElement.ToString());
        }
    }
}
