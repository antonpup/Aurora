using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_AnimationMixPresenter.xaml
    /// </summary>
    public partial class Control_AnimationMixPresenter : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ContextMixProperty = DependencyProperty.Register("ContextMix", typeof(AnimationMix), typeof(Control_AnimationMixPresenter));

        public AnimationMix ContextMix
        {
            get
            {
                return (AnimationMix)GetValue(ContextMixProperty);
            }
            set
            {
                SetValue(ContextMixProperty, value);

                stkPanelTracks.Children.Clear();

                foreach(var track in value.GetTracks())
                {
                    Control_AnimationTrackPresenter newTrack = new Control_AnimationTrackPresenter() { ContextTrack = track.Value };
                    newTrack.AnimationTrackUpdated += NewTrack_AnimationTrackUpdated;

                    stkPanelTracks.Children.Add(newTrack);
                    stkPanelTracks.Children.Add(new Separator());
                }
            }
        }

        private void NewTrack_AnimationTrackUpdated(object sender, AnimationTrack track)
        {
            AnimationMix newTrackMix = new AnimationMix();

            foreach (var child in stkPanelTracks.Children)
            {
                if (child is Control_AnimationTrackPresenter)
                {
                    Control_AnimationTrackPresenter item = (child as Control_AnimationTrackPresenter);

                    newTrackMix.AddTrack(item.ContextTrack);
                }
            }

            ContextMix = newTrackMix;

            //AnimationTrackUpdated?.Invoke(this, newTrack);
        }

        public delegate void AnimationMixRenderedDelegate(object sender);

        public event AnimationMixRenderedDelegate AnimationMixRendered;

        public Bitmap RenderedBitmap;

        private float _currentPlaybackTime = 0.0f;

        private Timer _playbackTimer = new Timer(33);

        public Control_AnimationMixPresenter()
        {
            InitializeComponent();

            RenderedBitmap = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            _playbackTimer.Elapsed += _playbackTimer_Elapsed;
        }

        private void _playbackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    _currentPlaybackTime += 0.033f;

                    if (!ContextMix.AnyActiveTracksAt(_currentPlaybackTime))
                    {
                        _currentPlaybackTime = 0.0f;
                        _playbackTimer.Stop();
                    }

                    grdsplitrScrubber.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100.0, 0, 0, 0);

                    UpdatePlaybackTime();
                });
        }

        private void btnPlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackTimer.Enabled)
                _playbackTimer.Stop();
            else
                _playbackTimer.Start();
        }

        private void grdsplitrScrubber_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double oldMargin = (sender as GridSplitter).Margin.Left;
            double newMargin = oldMargin + e.HorizontalChange;

            if (newMargin >= 100)
            {
                (sender as GridSplitter).Margin = new Thickness(newMargin, 0, 0, 0);
                _currentPlaybackTime = ConvertToTime(newMargin - 100);
            }
            else
            {
                (sender as GridSplitter).Margin = new Thickness(100, 0, 0, 0);
                _currentPlaybackTime = ConvertToTime(0);
            }

            UpdatePlaybackTime();
        }

        private void UpdatePlaybackTime()
        {
            int seconds = (int)_currentPlaybackTime;
            int milliseconds = (int)((_currentPlaybackTime - seconds) * 1000.0);

            this.txtblkCurrentTime.Text = $"{seconds};{milliseconds}";

            using (Graphics g = Graphics.FromImage(RenderedBitmap))
            {
                g.Clear(System.Drawing.Color.Black);

                ContextMix.Draw(g, _currentPlaybackTime);
            }

            if (chkbxDrawToDevices.IsChecked.Value)
                Global.effengine.ForceImageRender(RenderedBitmap);

            AnimationMixRendered?.Invoke(this);
        }

        private double ConvertToLocation(float time)
        {
            return time * 50.0;
        }

        private float ConvertToTime(double loc)
        {
            return (float)(loc / 50.0f);
        }

        private void chkbxDrawToDevices_Checked(object sender, RoutedEventArgs e)
        {
            if(!(sender as CheckBox).IsChecked.Value)
                Global.effengine.ForceImageRender(null);
        }
    }
}
