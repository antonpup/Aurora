using Aurora.Profiles.ETS2.GSI;
using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.ETS2 {
    /// <summary>
    /// Interaction logic for Control_ETS2.xaml
    /// </summary>
    public partial class Control_ETS2 : UserControl {

        private Application profile_manager;

        BlinkerComboBoxStates selectedBlinkerMode = BlinkerComboBoxStates.None;
        private Timer blinkerTimer = new Timer(500);

        public Control_ETS2(Application profile) {
            profile_manager = profile;

            InitializeComponent();
            
            SetSettings();

            PopulateComboBoxes();

            blinkerTimer.Elapsed += BlinkerTimer_Elapsed;
            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private GameState_ETS2 gameState => (GameState_ETS2)profile_manager?.Config.Event._game_state;

        // --------------------- //
        // --- Overview Tab --- //
        // ------------------- //
        private void Profile_manager_ProfileChanged(object sender, EventArgs e) {
            SetSettings();
        }

        private void SetSettings() {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        /// <summary>
        /// Installs either the 32-bit or 64-bit version of the Telemetry Server DLL.
        /// </summary>
        /// <param name="x64">Install 64-bit (true) or 32-bit (false)?</param>
        private bool InstallDLL(bool x64) {
            string gamePath = Utils.SteamUtils.GetGamePath(227300);
            if (String.IsNullOrWhiteSpace(gamePath))
                return false;
            
            string installPath = System.IO.Path.Combine(gamePath, "bin", x64 ? "win_x64" : "win_x86", "plugins", "ets2-telemetry-server.dll");

            if (!File.Exists(installPath))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(installPath));

            using (FileStream cfg_stream = File.Create(installPath)) {
                var sourceDll = x64 ? Properties.Resources.ets2_telemetry_server_x64 : Properties.Resources.ets2_telemetry_server_x86;
                cfg_stream.Write(sourceDll, 0, sourceDll.Length);
            }

            return true;
        }

        private void install_button_Click(object sender, RoutedEventArgs e) {
            if (!InstallDLL(true)) {
                MessageBox.Show("64-bit ETS2 Telemetry Server DLL installed failed.");
            } else if (!InstallDLL(false)) {
                MessageBox.Show("32-bit ETS2 Telemetry Server DLL installed failed.");
            } else {
                MessageBox.Show("ETS2 Telemetry Server DLLs installed successfully.");
            }
        }

        private void uninstall_button_Click(object sender, RoutedEventArgs e) {
            string gamePath = Utils.SteamUtils.GetGamePath(227300);
            if (String.IsNullOrWhiteSpace(gamePath)) return;
            string x86Path = System.IO.Path.Combine(gamePath, "bin", "win_x86", "plugins", "ets2-telemetry-server.dll");
            string x64Path = System.IO.Path.Combine(gamePath, "bin", "win_x64", "plugins", "ets2-telemetry-server.dll");
            if (File.Exists(x64Path))
                File.Delete(x64Path);
            if (File.Exists(x86Path))
                File.Delete(x86Path);
            MessageBox.Show("ETS2 Telemetry Server DLLs uninstalled successfully.");
        }

        private void visit_ets2ts_button_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Funbit/ets2-telemetry-server");
        }


        // -------------------- //
        // --- Preview Tab --- //
        // ------------------ //
        private void PopulateComboBoxes() {
            this.lights.Items.Add(LightComboBoxStates.Off);
            this.lights.Items.Add(LightComboBoxStates.ParkingLights);
            this.lights.Items.Add(LightComboBoxStates.LowBeam);
            this.lights.Items.Add(LightComboBoxStates.HighBeam);

            this.blinkers.Items.Add(BlinkerComboBoxStates.None);
            this.blinkers.Items.Add(BlinkerComboBoxStates.Left);
            this.blinkers.Items.Add(BlinkerComboBoxStates.Right);
            this.blinkers.Items.Add(BlinkerComboBoxStates.Hazard);
        }

        private void PreviewTextInput_Numeric(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            // https://stackoverflow.com/a/1268648/1305670
            e.Handled = new System.Text.RegularExpressions.Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void speed_TextChanged(object sender, TextChangedEventArgs e) {
            float.TryParse((sender as TextBox).Text, out gameState._memdat.value.speed);
        }

        private void speedLimit_TextChanged(object sender, TextChangedEventArgs e) {
            float.TryParse((sender as TextBox).Text, out gameState._memdat.value.navigationSpeedLimit);
        }

        private void lights_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            LightComboBoxStates selected = (LightComboBoxStates)(sender as ComboBox).SelectedItem;
            gameState._memdat.value.lightsParking = boolToByte(selected == LightComboBoxStates.ParkingLights); // Parking light only comes on when the parking lights are on
            gameState._memdat.value.lightsBeamLow = boolToByte(selected == LightComboBoxStates.LowBeam || selected == LightComboBoxStates.HighBeam); // Low beam light is on when the low beam is on OR when high beam is on
            gameState._memdat.value.lightsBeamHigh = boolToByte(selected == LightComboBoxStates.HighBeam); // Highbeam only on when high beam is on
        }

        private void frontAux_Checked(object sender, RoutedEventArgs e) {
            gameState._memdat.value.lightsAuxFront = boolToByte((sender as CheckBox).IsChecked);
        }

        private void roofAux_Checked(object sender, RoutedEventArgs e) {
            gameState._memdat.value.lightsAuxRoof = boolToByte((sender as CheckBox).IsChecked);
        }

        private void beacon_Checked(object sender, RoutedEventArgs e) {
            gameState._memdat.value.lightsBeacon = boolToByte((sender as CheckBox).IsChecked);
        }

        private void blinkers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            selectedBlinkerMode = (BlinkerComboBoxStates)(sender as ComboBox).SelectedItem;
            if (selectedBlinkerMode == BlinkerComboBoxStates.None) {
                blinkerTimer.Stop();
                setLeftBlinker(false);
                setRightBlinker(false);
            } else {
                blinkerTimer.Stop(); // Stop then start to reset the timer (so if we change from left to right for example, we don't have a partial phase)
                blinkerTimer.Start();
                // Immediately start the blinkers (as happens in the actual game and real life)
                setLeftBlinker(selectedBlinkerMode == BlinkerComboBoxStates.Left || selectedBlinkerMode == BlinkerComboBoxStates.Hazard);
                setRightBlinker(selectedBlinkerMode == BlinkerComboBoxStates.Right || selectedBlinkerMode == BlinkerComboBoxStates.Hazard);
            }
        }

        private void BlinkerTimer_Elapsed(object sender, ElapsedEventArgs e) {
            // When the timer ticks, toggle the hazard lights based on the selected blinker mode
            if (selectedBlinkerMode == BlinkerComboBoxStates.Left || selectedBlinkerMode == BlinkerComboBoxStates.Hazard)
                setLeftBlinker();
            if (selectedBlinkerMode == BlinkerComboBoxStates.Right || selectedBlinkerMode == BlinkerComboBoxStates.Hazard)
                setRightBlinker();
        }

        /// <summary>Sets or toggles the left blinker flag. Set v to null to toggle or a boolean to set to that value.</summary>
        private void setLeftBlinker(bool? v=null) {
            bool newState = v.HasValue ? v.Value : gameState._memdat.value.blinkerLeftOn == 0;
            gameState._memdat.value.blinkerLeftOn = (byte)(newState ? 1 : 0);
        }

        /// <summary>Sets or toggles the left blinker flag. Set v to null to toggle or a boolean to set to that value.</summary>
        private void setRightBlinker(bool? v=null) {
            bool newState = v.HasValue ? v.Value : gameState._memdat.value.blinkerRightOn == 0;
            gameState._memdat.value.blinkerRightOn = (byte)(newState ? 1 : 0);
        }

        private void throttleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            gameState._memdat.value.gameThrottle = (float)(sender as Slider).Value;
        }

        private void brakeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            gameState._memdat.value.gameBrake = (float)(sender as Slider).Value;
        }

        private void engineRpmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            gameState._memdat.value.engineRpm = (float)(sender as Slider).Value;
            gameState._memdat.value.engineRpmMax = 1f;
        }

        private void fuelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            gameState._memdat.value.fuel = (float)(sender as Slider).Value;
            gameState._memdat.value.fuelCapacity = 1;
        }

        private void airSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            gameState._memdat.value.airPressure = (float)(sender as Slider).Value;
        }

        private byte boolToByte(bool? v) {
            return (byte)(v.HasValue && v.Value ? 1 : 0);
        }
    }

    enum LightComboBoxStates {
        Off, ParkingLights, LowBeam, HighBeam
    }

    enum BlinkerComboBoxStates {
        None, Left, Right, Hazard
    }
}
