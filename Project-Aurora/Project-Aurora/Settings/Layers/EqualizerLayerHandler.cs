using Aurora.EffectsEngine;
using Aurora.Profiles;
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
using System.Linq;
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

    public class EqualizerLayerHandlerProperties : LayerHandlerProperties<EqualizerLayerHandlerProperties>
    {
        public Color? _SecondaryColor { get; set; }

        [JsonIgnore]
        public Color SecondaryColor { get { return Logic._SecondaryColor ?? _SecondaryColor ?? Color.Empty; } }

        public EffectBrush _Gradient { get; set; }

        [JsonIgnore]
        public EffectBrush Gradient { get { return Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear); } }

        public EqualizerType? _EQType { get; set; }

        [JsonIgnore]
        public EqualizerType EQType { get { return Logic._EQType ?? _EQType ?? EqualizerType.PowerBars; } }

        public EqualizerPresentationType? _ViewType { get; set; }

        [JsonIgnore]
        public EqualizerPresentationType ViewType { get { return Logic._ViewType ?? _ViewType ?? EqualizerPresentationType.SolidColor; } }

        public float? _MaxAmplitude { get; set; }

        [JsonIgnore]
        public float MaxAmplitude { get { return Logic._MaxAmplitude ?? _MaxAmplitude ?? 20.0f; } }

        public bool? _ScaleWithSystemVolume { get; set; }

        [JsonIgnore]
        public bool ScaleWithSystemVolume { get { return Logic._ScaleWithSystemVolume ?? _ScaleWithSystemVolume ?? false; } }

        public bool? _DimBackgroundOnSound { get; set; }

        [JsonIgnore]
        public bool DimBackgroundOnSound { get { return Logic._DimBackgroundOnSound ?? _DimBackgroundOnSound ?? false; } }

        public Color? _DimColor { get; set; }

        [JsonIgnore]
        public Color DimColor { get { return Logic._DimColor ?? _DimColor ?? Color.Empty; } }

        public SortedSet<float> _Frequencies { get; set; }

        [JsonIgnore]
        public SortedSet<float> Frequencies { get { return Logic._Frequencies ?? _Frequencies ?? new SortedSet<float>(); } }

        public EqualizerLayerHandlerProperties() : base()
        {

        }

        public EqualizerLayerHandlerProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
            _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            _SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
            _Gradient = new EffectBrush(ColorSpectrum.RainbowLoop).SetBrushType(EffectBrush.BrushType.Linear);
            _EQType = EqualizerType.PowerBars;
            _ViewType = EqualizerPresentationType.SolidColor;
            _MaxAmplitude = 20.0f;
            _DimBackgroundOnSound = false;
            _DimColor = Color.FromArgb(169, 0, 0, 0);
            _Frequencies = new SortedSet<float>() { 60, 170, 310, 600, 1000, 2000, 3000, 4000, 5000 };
        }
    }

    public class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties>
    {
        public event NewLayerRendered NewLayerRender = delegate { };

        MMDeviceEnumerator audio_device_enumerator = new MMDeviceEnumerator();
        MMDevice default_device = null;

        private List<float> flux_array = new List<float>();

        private IWaveIn waveIn;
        private static int fftLength = 1024; // NAudio fft wants powers of two! was 8192

        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);
        private Complex[] _ffts = { };
        private Complex[] _ffts_prev = { };

        private float[] previous_freq_results = null;

        public EqualizerLayerHandler()
        {
            _ID = "Equalizer";

            _ffts = new Complex[fftLength];
            _ffts_prev = new Complex[fftLength];

            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
        }

        protected override UserControl CreateControl()
        {
            return new Control_EqualizerLayer(this);
        }

        long startTime;
        private void UpdateAudioCapture(MMDevice defaultDevice)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
            }

            default_device?.Dispose();
            default_device = defaultDevice;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            waveIn = new WasapiLoopbackCapture(default_device);
            
            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
            startTime = Time.GetSecondsSinceEpoch();
        }

        private void CheckForDeviceChange()
        {
            MMDevice current_device = audio_device_enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (((WasapiLoopbackCapture)waveIn)?.CaptureState == CaptureState.Stopped
                || default_device == null
                || default_device.ID != current_device.ID
                || (((WasapiLoopbackCapture)waveIn)?.CaptureState != CaptureState.Capturing && (Time.GetSecondsSinceEpoch() - startTime) > 20)) //Check if it has taken over 20 seconds to start the capture which may indicate that there has been an issue
            {
                Global.logger.LogLine($"CaptureState is {((WasapiLoopbackCapture)waveIn)?.CaptureState}");
                UpdateAudioCapture(current_device);
            }
            else
                current_device.Dispose();

            current_device = null;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            try
            {
                //if (current_device != null)
                //current_device.Dispose();
                CheckForDeviceChange();

                // The system sound as a value between 0.0 and 1.0
                float system_sound_normalized = default_device.AudioEndpointVolume.MasterVolumeLevelScalar;

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

                EffectLayer equalizer_layer = new EffectLayer();

                if (Properties.DimBackgroundOnSound)
                {
                    bool hasSound = false;
                    foreach (var bin in _local_fft)
                    {
                        if (bin.X > 0.0005 || bin.X < -0.0005)
                        {
                            hasSound = true;
                            break;
                        }
                    }

                    if (hasSound)
                        equalizer_layer.Fill(Properties.DimColor);
                }

                using (Graphics g = equalizer_layer.GetGraphics())
                {
                    int wave_step_amount = _local_fft.Length / Effects.canvas_width;

                    switch (Properties.EQType)
                    {
                        case EqualizerType.Waveform:
                            for (int x = 0; x < Effects.canvas_width; x++)
                            {
                                float fft_val = _local_fft.Length > x * wave_step_amount ? _local_fft[x * wave_step_amount].X : 0.0f;

                                Brush brush = GetBrush(fft_val, x, Effects.canvas_width);

                                g.DrawLine(new Pen(brush), x, Effects.canvas_height_center, x, Effects.canvas_height_center - fft_val / scaled_max_amplitude * 500.0f);
                            }
                            break;
                        case EqualizerType.Waveform_Bottom:
                            for (int x = 0; x < Effects.canvas_width; x++)
                            {
                                float fft_val = _local_fft.Length > x * wave_step_amount ? _local_fft[x * wave_step_amount].X : 0.0f;

                                Brush brush = GetBrush(fft_val, x, Effects.canvas_width);

                                g.DrawLine(new Pen(brush), x, Effects.canvas_height, x, Effects.canvas_height - Math.Abs(fft_val / scaled_max_amplitude) * 1000.0f);
                            }
                            break;
                        case EqualizerType.PowerBars:

                            //Perform FFT again to get frequencies
                            FastFourierTransform.FFT(false, (int)Math.Log(fftLength, 2.0), _local_fft);

                            while (flux_array.Count < freqs.Length)
                            {
                                flux_array.Add(0.0f);
                            }

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

                            float bar_width = Effects.canvas_width / (float)(freqs.Length - 1);

                            for (int f_x = 0; f_x < freq_results.Length - 1; f_x++)
                            {
                                float fft_val = flux_array[f_x] / scaled_max_amplitude;

                                fft_val = Math.Min(1.0f, fft_val);

                                if (previous_freq_results[f_x] - fft_val > 0.10)
                                    fft_val = previous_freq_results[f_x] - 0.15f;

                                float x = f_x * bar_width;
                                float y = Effects.canvas_height;
                                float width = bar_width;
                                float height = fft_val * Effects.canvas_height;

                                previous_freq_results[f_x] = fft_val;

                                Brush brush = GetBrush(-(f_x % 2), f_x, freq_results.Length - 1);

                                g.FillRectangle(brush, x, y - height, width, height);
                            }

                            break;
                    }
                }

                var hander = NewLayerRender;
                if (hander != null)
                    hander.Invoke(equalizer_layer.GetBitmap());
                return equalizer_layer;

            }
            catch(Exception exc)
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
                int bufferIncrement = waveIn.WaveFormat.BlockAlign;

                // 4 bytes per channel, bufferIncrement is numChannels * 4
                for (int index = 0; index < bytesRecorded; index += bufferIncrement) // Loop over the bytes, respecting the channel grouping
                {
                    if (waveIn.WaveFormat.Channels == 2)
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
                EffectBrush e_brush = new EffectBrush(Properties.Gradient.GetColorSpectrum());
                e_brush.start = new PointF(0, 0);
                e_brush.end = new PointF(Effects.canvas_width, 0);

                return e_brush.GetDrawingBrush();
            }
            else if (Properties.ViewType == EqualizerPresentationType.GradientColorShift)
                return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(Utils.Time.GetMilliSeconds(), 1000));
            else if (Properties.ViewType == EqualizerPresentationType.GradientVertical)
            {
                EffectBrush e_brush = new EffectBrush(Properties.Gradient.GetColorSpectrum());
                e_brush.start = new PointF(0, Effects.canvas_height);
                e_brush.end = new PointF(0, 0);

                return e_brush.GetDrawingBrush();
            }
            else
                return new SolidBrush(Properties.PrimaryColor);
        }

        public override void Dispose()
        {
            waveIn?.Dispose();
            waveIn = null;
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
                fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
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
