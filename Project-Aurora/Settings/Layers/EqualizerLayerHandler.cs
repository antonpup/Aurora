using Aurora.EffectsEngine;
using Aurora.Profiles;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class EqualizerLayerHandler : LayerHandler
    {
        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 8192; // NAudio fft wants powers of two!

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private SampleAggregator sampleAggregator = new SampleAggregator(fftLength);

        private Complex[] _ffts = { };

        public Color PrimaryColor = Utils.ColorUtils.GenerateRandomColor();

        public EqualizerLayerHandler()
        {
            _Control = new Control_EqualizerLayer(this);

            _Type = LayerType.Equalizer;


            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            waveIn = new WasapiLoopbackCapture();

            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer equalizer_layer = new EffectLayer();

            using (Graphics g = equalizer_layer.GetGraphics())
            {
                int increments = _ffts.Length / Effects.canvas_width;

                for(int x = 0; x < _ffts.Length; x++)
                {
                    g.DrawLine(new Pen(Color.Red), x, Effects.canvas_height, x, Effects.canvas_height - _ffts[x].X * 2000.0f);
                    //Global.logger.LogLine($"_ffts[x].X = {_ffts[x].X}");
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
