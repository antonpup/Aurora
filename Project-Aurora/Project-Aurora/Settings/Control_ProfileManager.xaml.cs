using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Settings.Layers;
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
using System.Xml.Linq;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_SubProfileManager.xaml
    /// </summary>
    public partial class Control_ProfileManager : UserControl
    {
        public delegate void ProfileSelectedHandler(ApplicationProfile profile);

        public event ProfileSelectedHandler ProfileSelected;

        public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(Control_ProfileManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Dictionary<Profiles.Application, ApplicationProfile> LastSelectedProfile = new Dictionary<Profiles.Application, ApplicationProfile>();

        public Profiles.Application FocusedApplication
        {
            get { return (Profiles.Application)GetValue(FocusedApplicationProperty); }
            set
            {
                SetValue(FocusedApplicationProperty, value);
            }
        }

        public Control_ProfileManager()
        {
            InitializeComponent();

            lstProfiles.SelectionMode = SelectionMode.Single;
            lstProfiles.SelectionChanged += lstProfiles_SelectionChanged;
        }

        public static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Control_ProfileManager self = source as Control_ProfileManager;
            if (e.OldValue != null)
            {
                Profiles.Application prof = ((Profiles.Application)e.OldValue);
                prof.ProfileChanged -= self.UpdateProfiles;
                //prof.SaveProfiles();

                if (self.LastSelectedProfile.ContainsKey(prof))
                    self.LastSelectedProfile.Remove(prof);

                self.LastSelectedProfile.Add(prof, self.lstProfiles.SelectedItem as ApplicationProfile);
            }
            self.UpdateProfiles();
            if (e.NewValue != null)
            {
                Profiles.Application profile = ((Profiles.Application)e.NewValue);

                profile.ProfileChanged += self.UpdateProfiles;

                if (self.LastSelectedProfile.ContainsKey(profile))
                    self.lstProfiles.SelectedItem = self.LastSelectedProfile[profile];
            }
        }

        public void UpdateProfiles()
        {
            this.UpdateProfiles(null, null);
        }

        public void UpdateProfiles(object sender, EventArgs e)
        {
            this.lstProfiles.ItemsSource = this.FocusedApplication?.Profiles;
            lstProfiles.Items.SortDescriptions.Add(
            new System.ComponentModel.SortDescription("ProfileName",
            System.ComponentModel.ListSortDirection.Ascending));
            this.lstProfiles.SelectedItem = this.FocusedApplication?.Profiles.First((profile) => System.IO.Path.GetFileNameWithoutExtension(profile.ProfileFilepath).Equals(this.FocusedApplication?.Settings.SelectedProfile));
        }

        private void lstProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (lstProfiles.SelectedItem != null)
                {
                    if (!(lstProfiles.SelectedItem is ApplicationProfile))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'ProfileSettings', not '{lstProfiles.SelectedItem.GetType()}'");

                    this.FocusedApplication?.SwitchToProfile(lstProfiles.SelectedItem as ApplicationProfile);

                    ProfileSelected?.Invoke(lstProfiles.SelectedItem as ApplicationProfile);
                    this.btnDeleteProfile.IsEnabled = true;
                }
                else
                    this.btnDeleteProfile.IsEnabled = false;
            }
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            this.FocusedApplication?.SaveDefaultProfile();

            this.lstProfiles.SelectedIndex = this.lstProfiles.Items.Count - 1;
        }

        private void buttonDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstProfiles.SelectedIndex > -1)
            {
                if (this.FocusedApplication.Profiles.Count == 1)
                {
                    MessageBox.Show("You cannot delete the last profile!");
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete Profile '{((ApplicationProfile)lstProfiles.SelectedItem).ProfileName}'", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    int index = this.lstProfiles.SelectedIndex;
                    ApplicationProfile profile = (ApplicationProfile)this.lstProfiles.SelectedItem;

                    this.FocusedApplication.DeleteProfile(profile);

                    //this.lstProfiles.SelectedIndex = Math.Max(0, index - 1);
                }
            }
        }

        private void btnProfilePath_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedApplication != null)
            {
                System.Diagnostics.Process.Start(FocusedApplication.GetProfileFolderPath());
            }
        }


        private void btnProfileReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to reset the \"{this.FocusedApplication.Profile.ProfileName}\" profile?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                this.FocusedApplication?.ResetProfile();
        }

        private void lstProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                    Global.Clipboard = (this.lstProfiles.SelectedItem as ApplicationProfile)?.Clone();
                else if (e.Key == Key.V && Global.Clipboard is ApplicationProfile)
                {

                    ApplicationProfile prof = (ApplicationProfile)((ApplicationProfile)Global.Clipboard)?.Clone();
                    prof.ProfileName += " - Copy";

                    FocusedApplication.AddProfile(prof);

                    FocusedApplication.SaveProfiles();
                }
            }
            else if (e.Key == Key.Delete)
            {
                this.buttonDeleteProfile_Click(null, null);
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 80)
            {
                this.textblockDownload.Visibility = Visibility.Collapsed;
                this.borderBottom.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.textblockDownload.Visibility = Visibility.Visible;
                this.borderBottom.Visibility = Visibility.Visible;
            }
        }

        private void btnImportProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".cueprofile";
                dlg.Filter = "CUE Profile Files (*.cueprofile;*.cuefolder)|*.cueprofile;*.cuefolder";


                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();
                string filePath = "";

                // Get the selected file name and display in a TextBox 
                if (result == true)
                    filePath = dlg.FileName;


                if (filePath.EndsWith(".cueprofile") || filePath.EndsWith(".cuefolder"))
                {
                    {
                        XElement rootElement = XElement.Load(filePath);

                        XElement valueElement;
                        if (filePath.EndsWith(".cueprofile"))
                        {
                            valueElement = rootElement;
                        }
                        else
                        {
                            valueElement = rootElement.Element("profile_folder").Element("profiles");
                        }
                        foreach (XElement value in valueElement.Elements())
                        {
                            XElement profileElement = value.Element("profile");
                            if (profileElement != null)
                            {
                                String profileName = profileElement.Element("name").Value;
                                this.FocusedApplication?.AddNewProfile(profileName);

                                foreach (XElement property in profileElement.Element("properties").Elements())
                                {
                                    if ("Keyboard".Equals(property.Element("key").Value))
                                    {
                                        foreach (XElement profProperty in property.Element("value").Element("properties").Descendants())
                                        {
                                            if (profProperty.Name.ToString().Equals("keys"))
                                            {
                                                var hasValue = profProperty.Element("value0");
                                                if (hasValue != null)
                                                {
                                                    var layers = profProperty.Parent.Parent.Parent.Parent.Parent; Global.logger.Debug("Layers: " + layers.Name);
                                                    FocusedApplication.Profile.Layers.Clear();

                                                    uint _basePolyID = 2147483648;
                                                    Dictionary<uint, string> _definedPolyIDS = new Dictionary<uint, string>();

                                                    foreach (XElement layer in layers.Elements())
                                                    {
                                                        var keysAndStuff = layer.Element("ptr_wrapper").Element("data").Element("base");

                                                        string layerName = keysAndStuff.Element("name").Value; Global.logger.Debug("layerName: " + layerName);
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
                                                                        if (key.Value.StartsWith("Led_Top"))
                                                                            key.Value = "G18";
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
                                                                Global.logger.Debug("Exception in profile");
                                                                //break;
                                                            }
                                                        }

                                                        var lightingInfo = layer.Element("ptr_wrapper").Element("data").Element("lighting");
                                                        var transitionInfo = lightingInfo.Element("ptr_wrapper").Element("data").Element("transitions");
                                                        if (transitionInfo == null)
                                                        {
                                                            transitionInfo = lightingInfo.Element("ptr_wrapper").Element("data").Element("base").Element("transitions");
                                                        }
                                                        var layerPolyId = lightingInfo.Element("polymorphic_id");
                                                        var layerPolyName = lightingInfo.Element("polymorphic_name")?.Value;

                                                        if (String.IsNullOrWhiteSpace(layerPolyName))
                                                        {
                                                            if (_definedPolyIDS.ContainsKey(uint.Parse(layerPolyId.Value)))
                                                                layerPolyName = _definedPolyIDS[uint.Parse(layerPolyId.Value)];
                                                            var waveCheck = lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity");
                                                            var rippleCheck = lightingInfo.Element("ptr_wrapper").Element("data").Element("waveSpread");

                                                            if (rippleCheck != null)
                                                            {
                                                                layerPolyName = "RippleLighting";
                                                            }
                                                            else if (waveCheck != null)
                                                            {
                                                                layerPolyName = "WaveLighting";

                                                            }
                                                            else
                                                            {
                                                                var gradientCheck = transitionInfo.Element("value1");
                                                                if (gradientCheck != null)
                                                                {
                                                                    layerPolyName = "GradientLighting";
                                                                }
                                                                else
                                                                    layerPolyName = "StaticLighting";
                                                            }
                                                        }
                                                        else
                                                            _definedPolyIDS.Add(uint.Parse(layerPolyId.Value) - _basePolyID, layerPolyName);

                                                        Global.logger.Debug("Animation: " + layerPolyName);
                                                        if ("StaticLighting".Equals(layerPolyName))
                                                        {
                                                            FocusedApplication.Profile.Layers.Add(new Layers.Layer()
                                                            {
                                                                Name = layerName,
                                                                Enabled = layerEnabled,
                                                                Handler = new Layers.SolidColorLayerHandler()
                                                                {
                                                                    Properties = new Layers.LayerHandlerProperties()
                                                                    {
                                                                        _Sequence = affected_keys,
                                                                        _PrimaryColor = System.Drawing.ColorTranslator.FromHtml(transitionInfo.Element("value0").Element("color").Value)
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

                                                            foreach (XElement transition in transitionInfo.Elements())
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
                                                                float transitionDuration = 0.0f;

                                                                if (x + 1 != transitions.Count)
                                                                    transitionDuration = transitions.Keys.ElementAt(x + 1) - transitions.Keys.ElementAt(x);

                                                                animTrack.SetFrame(transitions.Keys.ElementAt(x), new AnimationFill(transitions[transitions.Keys.ElementAt(x)], transitionDuration));
                                                            }

                                                            FocusedApplication.Profile.Layers.Add(new Layers.Layer()
                                                            {
                                                                Name = layerName,
                                                                Enabled = layerEnabled,
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

                                                            foreach (XElement transition in transitionInfo.Elements())
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

                                                            FocusedApplication.Profile.Layers.Add(new Layers.Layer()
                                                            {
                                                                Name = layerName,
                                                                Enabled = layerEnabled,
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

                                                            foreach (XElement transition in transitionInfo.Elements())
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
                                                                    Global.logger.Debug("Wave Ex");
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

                                                            //transitions.Flip();

                                                            float velocity = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("velocity").Value) / 10.0f;
                                                            float width = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("tailLength").Value) / 10.0f;
                                                            bool isDoubleSided = bool.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("isDoublesided").Value);
                                                            float angle = float.Parse(lightingInfo.Element("ptr_wrapper").Element("data").Element("angle").Value);

                                                            width *= 2.1f;

                                                            angle %= 360; //Get angle within our range
                                                            if (angle < 0) angle += 360;

                                                            float _widthFillTime = width / (velocity * 2.1f);
                                                            float _terminalTime = duration / 1000.0f;


                                                            float _terminalOffset = velocity * _terminalTime * 2.1f;

                                                            if (!isDoubleSided)
                                                            {
                                                                AnimationTrack animTrack = new AnimationTrack(layerName, duration / 1000.0f);

                                                                float terminalTime = (Effects.canvas_width + width) / (velocity * (3.0f * 0.7f));

                                                                if (angle >= 315 || angle <= 45)
                                                                {
                                                                    float _angleOffset = (width / 2.0f) * (float)Math.Cos((double)angle * (Math.PI / 180.0));
                                                                    _angleOffset = (width / 2.0f) - _angleOffset;

                                                                    terminalTime = (Effects.canvas_width + width + 2.0f * _angleOffset) / (velocity * (3.0f * 0.7f));

                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-width - _angleOffset, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 5, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                                    animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width + _angleOffset, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 5, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                                }
                                                                else if (angle > 45 && angle < 135)
                                                                {
                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, Effects.canvas_height + width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                                    animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, (Effects.canvas_height + width / 2) - (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                                }
                                                                else if (angle >= 135 && angle <= 225)
                                                                {
                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width + width, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 5, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                                    animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-width, -Effects.canvas_height * 2.0f, width, Effects.canvas_height * 5, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                                }
                                                                else if (angle > 225 && angle < 315)
                                                                {
                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, -width / 2, width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));

                                                                    animTrack.SetFrame(terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width * 2.0f, (-width / 2) + (Effects.canvas_width + width), width, Effects.canvas_width * 10, new EffectsEngine.EffectBrush(transitions)).SetAngle(angle));
                                                                }

                                                                animTracks.Add(animTrack);
                                                            }
                                                            else
                                                            {
                                                                AnimationTrack animTrack = new AnimationTrack(layerName + " - Side 1", duration / 1000.0f);
                                                                AnimationTrack animTrack2 = new AnimationTrack(layerName + " - Side 2", duration / 1000.0f);

                                                                float widthTime = width / (velocity * (3.0f * 0.7f)) / 2;
                                                                _terminalTime = (Effects.canvas_width + width) / (velocity * 2.1f);

                                                                if ((angle >= 315 || angle <= 45) || (angle >= 135 && angle <= 225))
                                                                {
                                                                    //Right Side
                                                                    EffectsEngine.EffectBrush _initialBrushRight = new EffectsEngine.EffectBrush(transitions);
                                                                    _initialBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center, 0);
                                                                    _initialBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center - width, 0);

                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, 0, Effects.canvas_height * 3, _initialBrushRight).SetAngle(angle));

                                                                    if (_widthFillTime < _terminalTime)
                                                                    {
                                                                        EffectsEngine.EffectBrush _fillBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                                        _fillBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);
                                                                        _fillBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                                        animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, width, Effects.canvas_height * 3, _fillBrushRight).SetAngle(angle));

                                                                        EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_fillBrushRight);
                                                                        _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                                        _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                                        animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center + _terminalOffset - width, -Effects.canvas_height, width, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));

                                                                    }
                                                                    else
                                                                    {
                                                                        EffectsEngine.EffectBrush _terminalBrushRight = new EffectsEngine.EffectBrush(_initialBrushRight);
                                                                        _terminalBrushRight.start = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset, 0);
                                                                        _terminalBrushRight.end = new System.Drawing.PointF(Effects.canvas_width_center + _terminalOffset - width, 0);

                                                                        animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, _terminalOffset, Effects.canvas_height * 3, _terminalBrushRight).SetAngle(angle));
                                                                    }

                                                                    //Left Side
                                                                    EffectsEngine.EffectBrush _initialBrushLeft = new EffectsEngine.EffectBrush(transitions);
                                                                    _initialBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center, 0);
                                                                    _initialBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center + width, 0);

                                                                    animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(Effects.canvas_width_center, -Effects.canvas_height, 0, Effects.canvas_height * 3, _initialBrushLeft).SetAngle(angle));

                                                                    if (_widthFillTime < _terminalTime)
                                                                    {
                                                                        EffectsEngine.EffectBrush _fillBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                                        _fillBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - width, 0);
                                                                        _fillBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center, 0);

                                                                        animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - width, -Effects.canvas_height, width, Effects.canvas_height * 3, _fillBrushLeft).SetAngle(angle));

                                                                        EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                                        _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                                        _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                                        animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, -Effects.canvas_height, width, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                                    }
                                                                    else
                                                                    {
                                                                        EffectsEngine.EffectBrush _terminalBrushLeft = new EffectsEngine.EffectBrush(_initialBrushLeft);
                                                                        _terminalBrushLeft.start = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset, 0);
                                                                        _terminalBrushLeft.end = new System.Drawing.PointF(Effects.canvas_width_center - _terminalOffset + width, 0);

                                                                        animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(Effects.canvas_width_center - _terminalOffset, -Effects.canvas_height, _terminalOffset, Effects.canvas_height * 3, _terminalBrushLeft).SetAngle(angle));
                                                                    }
                                                                }
                                                                else if ((angle > 45 && angle < 135) || (angle > 225 && angle < 315))
                                                                {
                                                                    angle -= 90;

                                                                    //Bottom Side
                                                                    EffectsEngine.EffectBrush _initialBrushBottom = new EffectsEngine.EffectBrush(transitions);
                                                                    _initialBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                                    _initialBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center - width);

                                                                    animTrack.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushBottom).SetAngle(angle));

                                                                    if (_widthFillTime < _terminalTime)
                                                                    {
                                                                        EffectsEngine.EffectBrush _fillBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                                        _fillBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + width);
                                                                        _fillBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center);


                                                                        animTrack.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, width, _fillBrushBottom).SetAngle(angle));

                                                                        EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_fillBrushBottom);
                                                                        _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                                        _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                                        animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center + _terminalOffset - width, Effects.canvas_width * 3, width, _terminalBrushBottom).SetAngle(angle));
                                                                    }
                                                                    else
                                                                    {
                                                                        EffectsEngine.EffectBrush _terminalBrushBottom = new EffectsEngine.EffectBrush(_initialBrushBottom);
                                                                        _terminalBrushBottom.start = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset);
                                                                        _terminalBrushBottom.end = new System.Drawing.PointF(0, Effects.canvas_height_center + _terminalOffset - width);

                                                                        animTrack.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, _terminalOffset, _terminalBrushBottom).SetAngle(angle));
                                                                    }

                                                                    //Top Side
                                                                    EffectsEngine.EffectBrush _initialBrushtTop = new EffectsEngine.EffectBrush(transitions);
                                                                    _initialBrushtTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center);
                                                                    _initialBrushtTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center + width);

                                                                    animTrack2.SetFrame(0.0f, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center, Effects.canvas_width * 3, 0, _initialBrushtTop).SetAngle(angle));

                                                                    if (_widthFillTime < _terminalTime)
                                                                    {
                                                                        EffectsEngine.EffectBrush _fillBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                                        _fillBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - width);
                                                                        _fillBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center);

                                                                        animTrack2.SetFrame(_widthFillTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - width, Effects.canvas_width * 3, width, _fillBrushTop).SetAngle(angle));

                                                                        EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                                        _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                                        _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);
                                                                        animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, width, _terminalBrushTop).SetAngle(angle));
                                                                    }
                                                                    else
                                                                    {
                                                                        EffectsEngine.EffectBrush _terminalBrushTop = new EffectsEngine.EffectBrush(_initialBrushtTop);
                                                                        _terminalBrushTop.start = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset);
                                                                        _terminalBrushTop.end = new System.Drawing.PointF(0, Effects.canvas_height_center - _terminalOffset + width);

                                                                        animTrack2.SetFrame(_terminalTime, new AnimationFilledGradientRectangle(-Effects.canvas_width, Effects.canvas_height_center - _terminalOffset, Effects.canvas_width * 3, _terminalOffset, _terminalBrushTop).SetAngle(angle));
                                                                    }
                                                                }

                                                                animTracks.Add(animTrack);
                                                                animTracks.Add(animTrack2);
                                                            }

                                                            FocusedApplication.Profile.Layers.Add(new Layers.Layer()
                                                            {
                                                                Name = layerName,
                                                                Enabled = layerEnabled,
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

                                                            foreach (XElement transition in transitionInfo.Elements())
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

                                                            FocusedApplication.Profile.Layers.Add(new Layers.Layer()
                                                            {
                                                                Name = layerName,
                                                                Enabled = layerEnabled,
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
                                                            Global.logger.Warn("Unknown CUE Layer Type");
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception exception)
            {
                Global.logger.Error("Exception Found: " + exception.ToString());
            }
        }
    }
}