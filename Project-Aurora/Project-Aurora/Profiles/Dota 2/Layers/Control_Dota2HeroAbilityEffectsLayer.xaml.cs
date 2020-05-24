using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Utils;
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

namespace Aurora.Profiles.Dota_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Dota2HeroAbilityEffectsLayer.xaml
    /// </summary>
    public partial class Control_Dota2HeroAbilityEffectsLayer : UserControl
    {
        private bool settingsset = false;

        public Control_Dota2HeroAbilityEffectsLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2HeroAbilityEffectsLayer(Dota2HeroAbilityEffectsLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2HeroAbilityEffectsLayerHandler && !settingsset)
            {
                //Settings are set here...

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AnimationMix sandking_epicenter_mix = new AnimationMix();
            AnimationTrack sandking_epicenter_wave0 = new AnimationTrack("Sandsking Epicenter Wave0", 0.5f);
            sandking_epicenter_wave0.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(115, 255, 0), 4)
                );
            sandking_epicenter_wave0.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(115, 255, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave1 = new AnimationTrack("Sandsking Epicenter Wave1", 0.5f, 2.0f);
            sandking_epicenter_wave1.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave1.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave2 = new AnimationTrack("Sandsking Epicenter Wave2", 0.5f, 2.16f);
            sandking_epicenter_wave2.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave2.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave3 = new AnimationTrack("Sandsking Epicenter Wave3", 0.5f, 2.32f);
            sandking_epicenter_wave3.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave3.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave4 = new AnimationTrack("Sandsking Epicenter Wave4", 0.5f, 2.48f);
            sandking_epicenter_wave4.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave4.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave5 = new AnimationTrack("Sandsking Epicenter Wave5", 0.5f, 2.64f);
            sandking_epicenter_wave5.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave5.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave6 = new AnimationTrack("Sandsking Epicenter Wave6", 0.5f, 2.8f);
            sandking_epicenter_wave6.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave6.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave7 = new AnimationTrack("Sandsking Epicenter Wave7", 0.5f, 2.96f);
            sandking_epicenter_wave7.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave7.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave8 = new AnimationTrack("Sandsking Epicenter Wave8", 0.5f, 3.12f);
            sandking_epicenter_wave8.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave8.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave9 = new AnimationTrack("Sandsking Epicenter Wave9", 0.5f, 3.28f);
            sandking_epicenter_wave9.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave9.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave10 = new AnimationTrack("Sandsking Epicenter Wave10", 0.5f, 3.44f);
            sandking_epicenter_wave10.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave10.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave11 = new AnimationTrack("Sandsking Epicenter Wave11", 0.5f, 3.6f);
            sandking_epicenter_wave11.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave11.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            AnimationTrack sandking_epicenter_wave12 = new AnimationTrack("Sandsking Epicenter Wave12", 0.5f, 3.76f);
            sandking_epicenter_wave12.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave12.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave13 = new AnimationTrack("Sandsking Epicenter Wave13", 0.5f, 3.92f);
            sandking_epicenter_wave13.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave13.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave14 = new AnimationTrack("Sandsking Epicenter Wave14", 0.5f, 4.08f);
            sandking_epicenter_wave14.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave14.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave15 = new AnimationTrack("Sandsking Epicenter Wave15", 0.5f, 4.24f);
            sandking_epicenter_wave15.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave15.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave16 = new AnimationTrack("Sandsking Epicenter Wave16", 0.5f, 4.4f);
            sandking_epicenter_wave16.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave16.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave17 = new AnimationTrack("Sandsking Epicenter Wave17", 0.5f, 4.56f);
            sandking_epicenter_wave17.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave17.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave18 = new AnimationTrack("Sandsking Epicenter Wave18", 0.5f, 4.72f);
            sandking_epicenter_wave18.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave18.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave19 = new AnimationTrack("Sandsking Epicenter Wave19", 0.5f, 4.88f);
            sandking_epicenter_wave19.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave19.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave20 = new AnimationTrack("Sandsking Epicenter Wave20", 0.5f, 5f);
            sandking_epicenter_wave20.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave20.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, System.Drawing.Color.FromArgb(255, 160, 0), 4)
                );

            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave0);
            
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave1);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave2);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave3);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave4);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave5);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave6);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave7);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave8);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave9);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave10);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave11);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave12);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave13);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave14);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave15);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave16);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave17);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave18);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave19);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave20);
            

            Window win = new Window();
            win.Content = new Controls.Control_AnimationEditor() { AnimationMix = sandking_epicenter_mix };
            win.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            win.Show();
        }
    }
}
