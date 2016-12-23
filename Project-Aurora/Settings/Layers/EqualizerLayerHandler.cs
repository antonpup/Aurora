using Aurora.EffectsEngine;
using Aurora.Profiles;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public enum EqualizerType
    {
        Waveform,
        PowerBars
    }

    public class EqualizerLayerHandlerProperties : LayerHandlerProperties<EqualizerLayerHandlerProperties>
    {
        public EqualizerType? _EQType { get; set; }

        [JsonIgnore]
        public EqualizerType EQType { get { return Logic._EQType ?? _EQType ?? EqualizerType.PowerBars; } }

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
            _EQType = EqualizerType.PowerBars;
        }
    }

    public class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties>
    {
        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 1024; // NAudio fft wants powers of two! was 8192

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        private Complex[] _ffts = { };

        float[] previous_freq_results = null;

        public EqualizerLayerHandler()
        {
            _Type = LayerType.Equalizer;

            //PrimaryColor = Utils.ColorUtils.GenerateRandomColor();

            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            waveIn = new WasapiLoopbackCapture();

            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
        }

        protected override UserControl CreateControl()
        {
            return new Control_EqualizerLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            double[] freqs = {125, 250, 500, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 5000, 6000, 7000, 8000, 10000, 12000, 14000, 16000 };
            double[] freq_results = new double[freqs.Length];

            if (previous_freq_results == null)
                previous_freq_results = new float[freqs.Length];

            EffectLayer equalizer_layer = new EffectLayer();

            using (Graphics g = equalizer_layer.GetGraphics())
            {

                switch (Properties.EQType)
                {
                    case EqualizerType.Waveform:
                        for (int x = 0; x < Effects.canvas_width * 2; x += 2)
                        {
                            float fft_val = _ffts[x].X;

                            g.DrawLine(new Pen(Color.Red), x / 2, Effects.canvas_height_center, x / 2, Effects.canvas_height_center - fft_val * 1000.0f);
                        }
                        break;
                    case EqualizerType.PowerBars:

                        //Perform FFT again to get frequencies
                        FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), _ffts);

                        for(int x = 0; x < freqs.Length; x++)
                        {
                            int bin = (int)(freqs[x] / (waveIn.WaveFormat.SampleRate / _ffts.Length));

                            //System.Diagnostics.Debug.WriteLine($"bin = {bin} for Hz = {freqs[x]}");

                            Complex c = _ffts[bin];
                            double intensityDB = Math.Log10(Math.Sqrt(c.X * c.X + c.Y * c.Y));

                            freq_results[x] += intensityDB;
                        }

                        float bar_width = Effects.canvas_width / (float)freqs.Length;

                        for (int f_x = 0; f_x < freq_results.Length; f_x++)
                        {
                            float fft_val = (float)(freq_results[f_x] + 4.5) / 2.0f;

                            if (fft_val > 1.0f) fft_val = 1.0f;

                            if (fft_val <= 0 || previous_freq_results[f_x] - fft_val > 0.10)
                                fft_val = previous_freq_results[f_x] - 0.10f;

                            float x = f_x * bar_width;
                            float y = Effects.canvas_height;
                            float width = bar_width;
                            float height = fft_val * Effects.canvas_height;

                            previous_freq_results[f_x] = fft_val;

                            g.FillRectangle(new SolidBrush(f_x % 2 == 0 ? Color.Red : Color.Green), x, y - height, width, height);
                        }

                        break;
                }
            }

            return equalizer_layer;
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

                for (int index = 0; index < bytesRecorded; index += bufferIncrement)
                {
                    float sample32 = BitConverter.ToSingle(buffer, index);
                    sampleAggregator.Add(sample32);
                }
            }
        }

        void FftCalculated(object sender, FftEventArgs e)
        {
            // Do something with e.result!
            //Global.logger.LogLine($"{e.Result.ToString()}");

            _ffts = e.Result;
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
                    FastFourierTransform.FFT(true, m, fftBuffer);
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
