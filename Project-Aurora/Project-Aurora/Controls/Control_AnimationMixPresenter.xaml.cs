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

                foreach (var track in value.GetTracks())
                {
                    Control_AnimationTrackPresenter newTrack = new Control_AnimationTrackPresenter() { ContextTrack = track.Value };
                    newTrack.AnimationTrackUpdated += NewTrack_AnimationTrackUpdated;
                    newTrack.AnimationFrameItemSelected += NewTrack_AnimationFrameItemSelected;

                    stkPanelTracks.Children.Add(newTrack);
                    //stkPanelTracks.Children.Add(new Separator());
                }

                AnimationMixUpdated?.Invoke(this, ContextMix);
            }
        }

        public event Control_AnimationFrameItem.AnimationFrameItemArgs AnimationFrameItemSelected;

        private void NewTrack_AnimationFrameItemSelected(object sender, AnimationFrame track)
        {
            AnimationFrameItemSelected?.Invoke(sender, track);
        }

        private void NewTrack_AnimationTrackUpdated(object sender, AnimationTrack track)
        {
            if (track == null)
                stkPanelTracks.Children.Remove(sender as Control_AnimationTrackPresenter);

            ContextMix.Clear();

            foreach (var child in stkPanelTracks.Children)
            {
                if (child is Control_AnimationTrackPresenter)
                {
                    Control_AnimationTrackPresenter item = (child as Control_AnimationTrackPresenter);

                    ContextMix.AddTrack(item.ContextTrack);
                }
            }

            AnimationMixUpdated?.Invoke(this, ContextMix);

            UpdatePlaybackTime();
        }

        public delegate void AnimationMixRenderedDelegate(object sender);

        public event AnimationMixRenderedDelegate AnimationMixRendered;

        public delegate void AnimationMixArgs(object sender, AnimationMix mix);

        public event AnimationMixArgs AnimationMixUpdated;

        public Bitmap RenderedBitmap;

        public float AnimationScale = 1.0f;

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

                    gridScrubber.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100.0, 0, 0, 0);

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
            double oldMargin = gridScrubber.Margin.Left;
            double newMargin = oldMargin + e.HorizontalChange;

            if (newMargin >= 100)
            {
                gridScrubber.Margin = new Thickness(newMargin, 0, 0, 0);
                _currentPlaybackTime = ConvertToTime(newMargin - 100);
            }
            else
            {
                gridScrubber.Margin = new Thickness(100, 0, 0, 0);
                _currentPlaybackTime = ConvertToTime(0);
            }

            UpdatePlaybackTime();
        }

        public void UpdatePlaybackTime()
        {
            int seconds = (int)_currentPlaybackTime;
            int milliseconds = (int)((_currentPlaybackTime - seconds) * 1000.0);

            this.txtblkCurrentTime.Text = $"{seconds};{milliseconds}";

            Bitmap newBitmap = new Bitmap((int)(Effects.canvas_width * AnimationScale), (int)(Effects.canvas_height * AnimationScale));

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(System.Drawing.Color.Black);

                ContextMix.Draw(g, _currentPlaybackTime, AnimationScale);
            }

            RenderedBitmap = newBitmap;

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
            if (!(sender as CheckBox).IsChecked.Value)
                Global.effengine.ForceImageRender(null);
        }

        private void btnAddTrack_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private string GetAvailableTrackName(string DefaultTrackName)
        {
            int trackCopy = 1;
            string trackName = DefaultTrackName;

            if (ContextMix.ContainsTrack(trackName))
            {
                while (ContextMix.ContainsTrack($"{trackName} ({trackCopy})"))
                {
                    trackCopy++;
                }

                trackName = $"{trackName} ({trackCopy})";
            }

            return trackName;
        }

        private void menuitemAddCircleTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newCircleTrack = new AnimationTrack(GetAvailableTrackName("Circle Track"), 0.0f);
            newCircleTrack.SetFrame(0.0f, new AnimationCircle());

            ContextMix = ContextMix.AddTrack(newCircleTrack);
        }

        private void menuitemAddFilledCircleTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newFilledCircleTrack = new AnimationTrack(GetAvailableTrackName("Filled Circle Track"), 0.0f);
            newFilledCircleTrack.SetFrame(0.0f, new AnimationFilledCircle());

            ContextMix = ContextMix.AddTrack(newFilledCircleTrack);
        }

        private void menuitemAddRectangleTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newRectangleTrack = new AnimationTrack(GetAvailableTrackName("Rectangle Track"), 0.0f);
            newRectangleTrack.SetFrame(0.0f, new AnimationRectangle());

            ContextMix = ContextMix.AddTrack(newRectangleTrack);
        }

        private void menuitemAddFilledRectangleTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newFilledRectangleTrack = new AnimationTrack(GetAvailableTrackName("Filled Rectangle Track"), 0.0f);
            newFilledRectangleTrack.SetFrame(0.0f, new AnimationFilledRectangle());

            ContextMix = ContextMix.AddTrack(newFilledRectangleTrack);
        }

        private void menuitemAddLineTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newLineTrack = new AnimationTrack(GetAvailableTrackName("Line Track"), 0.0f);
            newLineTrack.SetFrame(0.0f, new AnimationLine());

            ContextMix = ContextMix.AddTrack(newLineTrack);
        }

        private void menuitemAddManualColorTrack_Click(object sender, RoutedEventArgs e)
        {
            AnimationTrack newManualColorTrack = new AnimationTrack(GetAvailableTrackName("Manual Color Track"), 0.0f);
            newManualColorTrack.SetFrame(0.0f, new AnimationManualColorFrame());

            ContextMix = ContextMix.AddTrack(newManualColorTrack);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Global.effengine.ForceImageRender(null);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            float deltaT = 0.001f;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                deltaT = 0.01f;

            if (e.Key == Key.Left)
            {
                e.Handled = true;

                if (_currentPlaybackTime - deltaT < 0)
                    _currentPlaybackTime = 0.0f;
                else
                    _currentPlaybackTime -= deltaT;

                gridScrubber.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100, 0, 0, 0);

                UpdatePlaybackTime();
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;

                _currentPlaybackTime += deltaT;

                gridScrubber.Margin = new Thickness(ConvertToLocation(_currentPlaybackTime) + 100, 0, 0, 0);

                UpdatePlaybackTime();
            }
        }
    }
}
