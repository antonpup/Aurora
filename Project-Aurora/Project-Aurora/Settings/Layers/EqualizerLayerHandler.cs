using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Aurora.Settings.Layers
{
    public enum EqualizerType
    {
        [Description("Power Bars")]
        PowerBars,

        [Description("Waveform")]
        Waveform,

        [Description("Waveform (From bottom)")]
        Waveform_Bottom

    }

    public enum EqualizerPresentationType
    {
        [Description("Solid Color")]
        SolidColor,

        [Description("Alternating Colors")]
        AlternatingColor,

        [Description("Gradient Notched Color")]
        GradientNotched,

        [Description("Gradient Color Shift")]
        GradientColorShift,

        [Description("Gradient (Horizontal)")]
        GradientHorizontal,

        [Description("Gradient (Vertical)")]
        GradientVertical
    }

    public enum EqualizerBackgroundMode
    {
        [Description("Disabled")]
        Disabled,

        [Description("Always on")]
        AlwaysOn,

        [Description("On sound")]
        EnabledOnSound
    }

    public class EqualizerLayerHandlerProperties : LayerHandlerProperties<EqualizerLayerHandlerProperties>
    {
        [LogicOverridable("Secondary Color")]
        public Color? _SecondaryColor { get; set; }
        [JsonIgnore]
        public Color SecondaryColor { get { return Logic._SecondaryColor ?? _SecondaryColor ?? Color.Empty; } }

        [LogicOverridable("Gradient")]
        public EffectBrush _Gradient { get; set; }
        [JsonIgnore]
        public EffectBrush Gradient { get { return Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear); } }

        [LogicOverridable("Equalizer Type")]
        public EqualizerType? _EQType { get; set; }
        [JsonIgnore]
        public EqualizerType EQType { get { return Logic._EQType ?? _EQType ?? EqualizerType.PowerBars; } }

        [LogicOverridable("View Type")]
        public EqualizerPresentationType? _ViewType { get; set; }
        [JsonIgnore]
        public EqualizerPresentationType ViewType { get { return Logic._ViewType ?? _ViewType ?? EqualizerPresentationType.SolidColor; } }

        [LogicOverridable("Background Mode")]
        public EqualizerBackgroundMode? _BackgroundMode { get; set; }
        [JsonIgnore]
        public EqualizerBackgroundMode BackgroundMode { get { return Logic._BackgroundMode ?? _BackgroundMode ?? EqualizerBackgroundMode.Disabled; } }

        [LogicOverridable("Max Amplitude")]
        public float? _MaxAmplitude { get; set; }
        [JsonIgnore]
        public float MaxAmplitude { get { return Logic._MaxAmplitude ?? _MaxAmplitude ?? 20.0f; } }

        [LogicOverridable("Scale with System Volume")]
        public bool? _ScaleWithSystemVolume { get; set; }
        [JsonIgnore]
        public bool ScaleWithSystemVolume { get { return Logic._ScaleWithSystemVolume ?? _ScaleWithSystemVolume ?? false; } }

        [LogicOverridable("Background Color")]
        public Color? _DimColor { get; set; }
        [JsonIgnore]
        public Color DimColor { get { return Logic._DimColor ?? _DimColor ?? Color.Empty; } }

        public SortedSet<float> _Frequencies { get; set; }
        [JsonIgnore]
        public SortedSet<float> Frequencies { get { return Logic._Frequencies ?? _Frequencies ?? new SortedSet<float>(); } }

        public string _DeviceId { get; set; }
        [JsonIgnore] public string DeviceId => Logic?._DeviceId ?? _DeviceId ?? "";

        public EqualizerLayerHandlerProperties() : base()
        {

        }

        public EqualizerLayerHandlerProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
            _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            _SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
            _Gradient = new EffectBrush(ColorSpectrum.RainbowLoop).SetBrushType(EffectBrush.BrushType.Linear);
            _EQType = EqualizerType.PowerBars;
            _ViewType = EqualizerPresentationType.SolidColor;
            _MaxAmplitude = 20.0f;
            _ScaleWithSystemVolume = false;
            _BackgroundMode = EqualizerBackgroundMode.Disabled;
            _DimColor = Color.FromArgb(169, 0, 0, 0);
            _Frequencies = new SortedSet<float>() { 50, 95, 130, 180, 250, 350, 500, 620, 700, 850, 1200, 1600, 2200, 3000, 4100, 5600, 7700, 10000 };
            _DeviceId = "";
        }
    }

    [LayerHandlerMeta(Name = "Audio Visualizer", IsDefault = true)]
    public class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties>
    {
        public event NewLayerRendered NewLayerRender = delegate { };

        private AudioDeviceProxy deviceProxy;
        private AudioDeviceProxy DeviceProxy {
            get {
                if (deviceProxy == null) {
                    deviceProxy = new AudioDeviceProxy(DataFlow.Render);
                    deviceProxy.WaveInDataAvailable += OnDataAvailable;
                }
                return deviceProxy;
            }
        }

        private List<float> flux_array = new List<float>();
        private static int fftLength = 1024; // NAudio fft wants powers of two! was 8192

        // Base rectangle that defines the region that is used to render the audio output
        // Higher values mean a higher initial resolution, but may increase memory usage (looking at you, waveform).
        // KEEP X AND Y AT 0
        private static readonly RectangleF sourceRect = new RectangleF(0, 0, 80, 40);

        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);
        private Complex[] _ffts = { };
        private Complex[] _ffts_prev = { };

        private float[] previous_freq_results = null;

        private bool first = true;

        public EqualizerLayerHandler()
        {
            _ffts = new Complex[fftLength];
            _ffts_prev = new Complex[fftLength];

            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
        }

        protected override UserControl CreateControl()
        {
            return new Control_EqualizerLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            try
            {
                //this initialization has to be done on this method 
                //because of threading issues with the NAudio stuff.
                //it should only be done once, so we set this bool to true right after.
                //should prevent the layer from spamming the log with errors and throwing
                //exceptions left and right if the users has nahimic drivers installed
                if (first)
                {
                    try
                    {
                        DeviceProxy.DeviceId = Properties.DeviceId;
                    }
                    catch (COMException e)
                    {
                        Global.logger.Error("Error binding to audio device in the audio visualizer layer. This is probably caused by an incompatibility with audio software: " + e);
                    }
                    first = false;
                }

                EffectLayer equalizer_layer = new EffectLayer();

                if (deviceProxy is null)
                    return equalizer_layer;

                // Update device ID. If it has changed, it will re-assign itself to the new device
                DeviceProxy.DeviceId = Properties.DeviceId;

                // The system sound as a value between 0.0 and 1.0
                float system_sound_normalized = DeviceProxy.Device?.AudioMeterInformation?.MasterPeakValue ?? 1f;

                // Scale the Maximum amplitude with the system sound if enabled, so that at 100% volume the max_amp is unchanged.
                // Replaces all Properties.MaxAmplitude calls with the scaled value
                float scaled_max_amplitude = Properties.MaxAmplitude * (Properties.ScaleWithSystemVolume ? system_sound_normalized : 1);

                float[] freqs = Properties.Frequencies.ToArray(); //Defined Frequencies

                double[] freq_results = new double[freqs.Length];

                if (previous_freq_results == null || previous_freq_results.Length < freqs.Length)
                    previous_freq_results = new float[freqs.Length];

                //Maintain local copies of fft, to prevent data overwrite
                Complex[] _local_fft = new List<Complex>(_ffts).ToArray();
                Complex[] _local_fft_previous = new List<Complex>(_ffts_prev).ToArray();

                bool BgEnabled = false;
                switch (Properties.BackgroundMode)
                {
                    case EqualizerBackgroundMode.EnabledOnSound:
                        foreach (var bin in _local_fft)
                        {
                            if (bin.X > 0.0005 || bin.X < -0.0005)
                            {
                                BgEnabled = true;
                                break;
                            }
                        }
                        break;
                    case EqualizerBackgroundMode.AlwaysOn:
                        BgEnabled = true;
                        break;
                }


                // Use the new transform render method to draw the equalizer layer
                equalizer_layer.DrawTransformed(Properties.Sequence, g => {
                    // Here we draw the equalizer relative to our source rectangle and the DrawTransformed method handles sizing and positioning it correctly for us

                    // Draw a rectangle background over the entire source rect if bg is enabled
                    if (BgEnabled)
                        g.FillRectangle(new SolidBrush(Properties.DimColor), sourceRect);

                    g.CompositingMode = CompositingMode.SourceCopy;
                    
                    int wave_step_amount = _local_fft.Length / (int)sourceRect.Width;

                    switch (Properties.EQType) {
                        case EqualizerType.Waveform:
                            var halfHeight = sourceRect.Height / 2f;
                            for (int x = 0; x < (int)sourceRect.Width; x++) {
                                float fft_val = _local_fft.Length > x * wave_step_amount ? _local_fft[x * wave_step_amount].X : 0.0f;
                                Brush brush = GetBrush(fft_val, x, sourceRect.Width);
                                var yOff = -Math.Max(Math.Min(fft_val / scaled_max_amplitude * 1000.0f, halfHeight), -halfHeight);
                                g.DrawLine(new Pen(brush), x, halfHeight, x, halfHeight + yOff);
                            }
                            break;

                        case EqualizerType.Waveform_Bottom:
                            for (int x = 0; x < (int)sourceRect.Width; x++) {
                                float fft_val = _local_fft.Length > x * wave_step_amount ? _local_fft[x * wave_step_amount].X : 0.0f;
                                Brush brush = GetBrush(fft_val, x, sourceRect.Width);
                                g.DrawLine(new Pen(brush), x, sourceRect.Height, x, sourceRect.Height - Math.Min(Math.Abs(fft_val / scaled_max_amplitude) * 1000.0f, sourceRect.Height));
                            }
                            break;

                        case EqualizerType.PowerBars:
                            //Perform FFT again to get frequencies
                            FastFourierTransform.FFT(false, (int)Math.Log(fftLength, 2.0), _local_fft);

                            while (flux_array.Count < freqs.Length)
                                flux_array.Add(0.0f);

                            int startF = 0;
                            int endF = 0;

                            float threshhold = 300.0f;

                            for (int x = 0; x < freqs.Length - 1; x++)
                            {
                                startF = freqToBin(freqs[x]);
                                endF = freqToBin(freqs[x + 1]);

                                float flux = 0.0f;

                                for (int j = startF; j <= endF; j++)
                                {
                                    float curr_fft = (float)Math.Sqrt(_local_fft[j].X * _local_fft[j].X + _local_fft[j].Y * _local_fft[j].Y);
                                    float prev_fft = (float)Math.Sqrt(_local_fft_previous[j].X * _local_fft_previous[j].X + _local_fft_previous[j].Y * _local_fft_previous[j].Y);

                                    float value = curr_fft - prev_fft;
                                    float flux_calc = (value + Math.Abs(value)) / 2;
                                    if (flux < flux_calc)
                                        flux = flux_calc;

                                    flux = flux > threshhold ? 0.0f : flux;
                                }

                                flux_array[x] = flux;
                            }

                            //System.Diagnostics.Debug.WriteLine($"flux max: {flux_array.Max()}");

                            float bar_width = sourceRect.Width / (float)(freqs.Length - 1);

                            for (int f_x = 0; f_x < freq_results.Length - 1; f_x++)
                            {
                                float fft_val = flux_array[f_x] / scaled_max_amplitude;

                                fft_val = Math.Min(1.0f, fft_val);

                                if (previous_freq_results[f_x] - fft_val > 0.10)
                                    fft_val = previous_freq_results[f_x] - 0.15f;

                                float x = f_x * bar_width;
                                float y = sourceRect.Height;
                                float height = fft_val * sourceRect.Height;

                                previous_freq_results[f_x] = fft_val;

                                Brush brush = GetBrush(-(f_x % 2), f_x, freq_results.Length - 1);

                                g.FillRectangle(brush, x, y - height, bar_width, height);
                            }
                            break;
                    }

                }, sourceRect);
                

                var hander = NewLayerRender;
                if (hander != null)
                    hander.Invoke(equalizer_layer.GetBitmap());
                return equalizer_layer;

            }
            catch (Exception exc)
            {
                Global.logger.Error("Error encountered in the Equalizer layer. Exception: " + exc.ToString());
                return new EffectLayer();
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (false/*this.InvokeRequired*/)
            {
                //this.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                byte[] buffer = e.Buffer;
                int bytesRecorded = e.BytesRecorded;
                int bufferIncrement = DeviceProxy.WaveIn.WaveFormat.BlockAlign;

                // 4 bytes per channel, bufferIncrement is numChannels * 4
                for (int index = 0; index < bytesRecorded; index += bufferIncrement) // Loop over the bytes, respecting the channel grouping
                {
                    if (DeviceProxy.WaveIn.WaveFormat.Channels == 2)
                        // If recording has two channels, take the largest value and add that to the sampleAggregator.
                        sampleAggregator.Add(Math.Max(BitConverter.ToSingle(buffer, index), BitConverter.ToSingle(buffer, index + 4)));
                    else
                        // Fallback to the original if there's only one channel
                        sampleAggregator.Add(BitConverter.ToSingle(buffer, index));
                }
            }
        }

        void FftCalculated(object sender, FftEventArgs e)
        {
            // Do something with e.result!
            //Global.logger.LogLine($"{e.Result.ToString()}");

            _ffts_prev = new List<Complex>(_ffts).ToArray();
            _ffts = new List<Complex>(e.Result).ToArray();
        }

        private int freqToBin(float freq)
        {
            return (int)(freq / (44000 / _ffts.Length));
        }

        private Brush GetBrush(float value, float position, float max_position)
        {
            if (Properties.ViewType == EqualizerPresentationType.AlternatingColor)
            {
                if (value >= 0)
                    return new SolidBrush(Properties.PrimaryColor);
                else
                    return new SolidBrush(Properties.SecondaryColor);
            }
            else if (Properties.ViewType == EqualizerPresentationType.GradientNotched)
                return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(position, max_position));
            else if (Properties.ViewType == EqualizerPresentationType.GradientHorizontal)
            {
                EffectBrush e_brush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                    start = PointF.Empty,
                    end = new PointF(sourceRect.Width, 0)
                };

                return e_brush.GetDrawingBrush();
            }
            else if (Properties.ViewType == EqualizerPresentationType.GradientColorShift)
                return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(Utils.Time.GetMilliSeconds(), 1000));
            else if (Properties.ViewType == EqualizerPresentationType.GradientVertical)
            {
                EffectBrush e_brush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                    start = new PointF(0, sourceRect.Height),
                    end = PointF.Empty
                };

                return e_brush.GetDrawingBrush();
            }
            else
                return new SolidBrush(Properties.PrimaryColor);
        }

        public override void Dispose()
        {
            deviceProxy?.Dispose();
            deviceProxy = null;
        }
    }

    public class SampleAggregator
    {
        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }

        // This Complex is NAudio's own! 
        private Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private int fftLength;
        private int m;

        public SampleAggregator(int fftLength)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;
            this.fftBuffer = new Complex[fftLength];
            this.fftArgs = new FftEventArgs(fftBuffer);
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public void Add(float value)
        {
            if (PerformFFT && FftCalculated != null)
            {
                // Remember the window function! There are many others as well.
                fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HannWindow(fftPos, fftLength));
                fftBuffer[fftPos].Y = 0; // This is always zero with audio.
                fftPos++;
                if (fftPos >= fftLength)
                {
                    fftPos = 0;
                    //FastFourierTransform.FFT(true, m, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }
        }
    }

    public class FftEventArgs : EventArgs
    {
        public FftEventArgs(Complex[] result)
        {
            this.Result = result;
        }
        public Complex[] Result { get; private set; }
    }
}
