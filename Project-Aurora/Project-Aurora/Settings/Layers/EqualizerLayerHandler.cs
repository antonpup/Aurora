using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;

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
        public Color SecondaryColor => Logic._SecondaryColor ?? _SecondaryColor ?? Color.Empty;

        [LogicOverridable("Gradient")]
        public EffectBrush _Gradient { get; set; }
        [JsonIgnore]
        public EffectBrush Gradient => Logic._Gradient ?? _Gradient ?? new EffectBrush().SetBrushType(EffectBrush.BrushType.Linear);

        [LogicOverridable("Equalizer Type")]
        public EqualizerType? _EQType { get; set; }
        [JsonIgnore]
        public EqualizerType EQType => Logic._EQType ?? _EQType ?? EqualizerType.PowerBars;

        [LogicOverridable("View Type")]
        public EqualizerPresentationType? _ViewType { get; set; }
        [JsonIgnore]
        public EqualizerPresentationType ViewType => Logic._ViewType ?? _ViewType ?? EqualizerPresentationType.SolidColor;

        [LogicOverridable("Background Mode")]
        public EqualizerBackgroundMode? _BackgroundMode { get; set; }
        [JsonIgnore]
        public EqualizerBackgroundMode BackgroundMode => Logic._BackgroundMode ?? _BackgroundMode ?? EqualizerBackgroundMode.Disabled;

        [LogicOverridable("Max Amplitude")]
        public float? _MaxAmplitude { get; set; }
        [JsonIgnore]
        public float MaxAmplitude => Logic._MaxAmplitude ?? _MaxAmplitude ?? 20.0f;

        [LogicOverridable("Scale with System Volume")]
        public bool? _ScaleWithSystemVolume { get; set; }
        [JsonIgnore]
        public bool ScaleWithSystemVolume => Logic._ScaleWithSystemVolume ?? _ScaleWithSystemVolume ?? false;

        [LogicOverridable("Background Color")]
        public Color? _DimColor { get; set; }
        [JsonIgnore]
        public Color DimColor => Logic._DimColor ?? _DimColor ?? Color.Empty;

        public SortedSet<float> _Frequencies { get; set; }
        [JsonIgnore]
        public SortedSet<float> Frequencies => Logic._Frequencies ?? _Frequencies ?? new SortedSet<float>();

        public string _DeviceId { get; set; }
        [JsonIgnore] public string DeviceId => Logic?._DeviceId ?? _DeviceId ?? "";

        public EqualizerLayerHandlerProperties()
        {

        }

        public EqualizerLayerHandlerProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
            _PrimaryColor = ColorUtils.GenerateRandomColor();
            _SecondaryColor = ColorUtils.GenerateRandomColor();
            _Gradient = new EffectBrush(ColorSpectrum.RainbowLoop).SetBrushType(EffectBrush.BrushType.Linear);
            _EQType = EqualizerType.PowerBars;
            _ViewType = EqualizerPresentationType.SolidColor;
            _MaxAmplitude = 20.0f;
            _ScaleWithSystemVolume = false;
            _BackgroundMode = EqualizerBackgroundMode.Disabled;
            _DimColor = Color.FromArgb(169, 0, 0, 0);
            _Frequencies = new SortedSet<float> { 50, 95, 130, 180, 250, 350, 500, 620, 700, 850, 1200, 1600, 2200, 3000, 4100, 5600, 7700, 10000 };
            _DeviceId = "";
        }
    }

    [LayerHandlerMeta(Name = "Audio Visualizer", IsDefault = true)]
    public class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties>
    {
        public event NewLayerRendered NewLayerRender = delegate { };

        private AudioDeviceProxy _deviceProxy;
        private int _channels;
        private int _bufferIncrement;
        private AudioDeviceProxy DeviceProxy {  //_deviceProxy.WaveIn.WaveFormat is a very expensive line
            get {
                if (_deviceProxy != null) return _deviceProxy;
                _deviceProxy = new AudioDeviceProxy(DataFlow.Render);
                _channels = _deviceProxy.WaveIn.WaveFormat.Channels;
                _bufferIncrement = _deviceProxy.WaveIn.WaveFormat.BlockAlign;
                _deviceProxy.WaveInDataAvailable += OnDataAvailable;
                return _deviceProxy;
            }
        }

        private readonly List<float> _fluxArray = new();
        private const int FftLength = 1024; // NAudio fft wants powers of two! was 8192

        // Base rectangle that defines the region that is used to render the audio output
        // Higher values mean a higher initial resolution, but may increase memory usage (looking at you, waveform).
        // KEEP X AND Y AT 0
        private static readonly RectangleF SourceRect = new(0, 0, 80, 40);

        private readonly SampleAggregator _sampleAggregator = new(FftLength);
        private Complex[] _ffts;
        private Complex[] _fftsPrev;

        private float[] _previousFreqResults;

        private bool _first = true;
        public EqualizerLayerHandler(): base("EqualizerLayer")
        {
            _ffts = new Complex[FftLength];
            _fftsPrev = new Complex[FftLength];

            _sampleAggregator.FftCalculated += FftCalculated;
            _sampleAggregator.PerformFft = true;
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
                if (_first)
                {
                    try
                    {
                        DeviceProxy.DeviceId = Properties.DeviceId;
                    }
                    catch (COMException e)
                    {
                        Global.logger.Error("Error binding to audio device in the audio visualizer layer. This is probably caused by an incompatibility with audio software: " + e);
                    }
                    _first = false;
                }


                if (_deviceProxy is null)
                    return EffectLayer;

                // Update device ID. If it has changed, it will re-assign itself to the new device
                DeviceProxy.DeviceId = Properties.DeviceId;

                // The system sound as a value between 0.0 and 1.0
                var systemSoundNormalized = DeviceProxy.Device?.AudioMeterInformation?.MasterPeakValue ?? 1f;

                // Scale the Maximum amplitude with the system sound if enabled, so that at 100% volume the max_amp is unchanged.
                // Replaces all Properties.MaxAmplitude calls with the scaled value
                var scaledMaxAmplitude = Properties.MaxAmplitude * (Properties.ScaleWithSystemVolume ? systemSoundNormalized : 1);

                var freqs = Properties.Frequencies.ToArray(); //Defined Frequencies

                var freqResults = new double[freqs.Length];

                if (_previousFreqResults == null || _previousFreqResults.Length < freqs.Length)
                    _previousFreqResults = new float[freqs.Length];

                //Maintain local copies of fft, to prevent data overwrite
                var localFft = new List<Complex>(_ffts).ToArray();
                var localFftPrevious = new List<Complex>(_fftsPrev).ToArray();

                var bgEnabled = false;
                switch (Properties.BackgroundMode)
                {
                    case EqualizerBackgroundMode.EnabledOnSound:
                        if (localFft.Any(bin => bin.X > 0.0005 || bin.X < -0.0005))
                        {
                            bgEnabled = true;
                        }
                        else
                        {
                            EffectLayer.Clear();
                        }
                        break;
                    case EqualizerBackgroundMode.AlwaysOn:
                        bgEnabled = true;
                        break;
                    case EqualizerBackgroundMode.Disabled:
                    default:
                        EffectLayer.Clear();
                        break;
                }


                // Use the new transform render method to draw the equalizer layer
                EffectLayer.DrawTransformed(Properties.Sequence, g => {
                    // Here we draw the equalizer relative to our source rectangle and the DrawTransformed method handles sizing and positioning it correctly for us

                    g.CompositingMode = CompositingMode.SourceCopy;
                    // Draw a rectangle background over the entire source rect if bg is enabled
                    if (bgEnabled)
                        g.FillRectangle(new SolidBrush(Properties.DimColor), SourceRect);

                    
                    var waveStepAmount = localFft.Length / (int)SourceRect.Width;

                    switch (Properties.EQType) {
                        case EqualizerType.Waveform:
                            var halfHeight = SourceRect.Height / 2f;
                            for (var x = 0; x < (int)SourceRect.Width; x++) {
                                var fftVal = localFft.Length > x * waveStepAmount ? localFft[x * waveStepAmount].X : 0.0f;
                                var brush = GetBrush(fftVal, x, SourceRect.Width);
                                var yOff = -Math.Max(Math.Min(fftVal / scaledMaxAmplitude * 1000.0f, halfHeight), -halfHeight);
                                g.DrawLine(new Pen(brush), x, halfHeight, x, halfHeight + yOff);
                            }
                            break;

                        case EqualizerType.Waveform_Bottom:
                            for (var x = 0; x < (int)SourceRect.Width; x++) {
                                var fftVal = localFft.Length > x * waveStepAmount ? localFft[x * waveStepAmount].X : 0.0f;
                                var brush = GetBrush(fftVal, x, SourceRect.Width);
                                g.DrawLine(new Pen(brush), x, SourceRect.Height, x, SourceRect.Height - Math.Min(Math.Abs(fftVal / scaledMaxAmplitude) * 1000.0f, SourceRect.Height));
                            }
                            break;

                        case EqualizerType.PowerBars:
                            //Perform FFT again to get frequencies
                            FastFourierTransform.FFT(false, (int)Math.Log(FftLength, 2.0), localFft);

                            while (_fluxArray.Count < freqs.Length)
                                _fluxArray.Add(0.0f);

                            const float threshold = 300.0f;

                            for (var x = 0; x < freqs.Length - 1; x++)
                            {
                                var startF = FreqToBin(freqs[x]);
                                var endF = FreqToBin(freqs[x + 1]);
                                var flux = 0.0f;

                                for (var j = startF; j <= endF; j++)
                                {
                                    var currFft = (float)Math.Sqrt(localFft[j].X * localFft[j].X + localFft[j].Y * localFft[j].Y);
                                    var prevFft = (float)Math.Sqrt(localFftPrevious[j].X * localFftPrevious[j].X + localFftPrevious[j].Y * localFftPrevious[j].Y);

                                    var value = currFft - prevFft;
                                    var fluxCalc = (value + Math.Abs(value)) / 2;
                                    if (flux < fluxCalc)
                                        flux = fluxCalc;

                                    flux = flux > threshold ? 0.0f : flux;
                                }

                                _fluxArray[x] = flux;
                            }

                            var barWidth = SourceRect.Width / (freqs.Length - 1);

                            for (var fX = 0; fX < freqResults.Length - 1; fX++)
                            {
                                var fftVal = _fluxArray[fX] / scaledMaxAmplitude;

                                fftVal = Math.Min(1.0f, fftVal);

                                if (_previousFreqResults[fX] - fftVal > 0.10)
                                    fftVal = _previousFreqResults[fX] - 0.15f;

                                var x = fX * barWidth;
                                var y = SourceRect.Height;
                                var height = fftVal * SourceRect.Height;

                                _previousFreqResults[fX] = fftVal;

                                var brush = GetBrush(-(fX % 2), fX, freqResults.Length - 1);

                                g.FillRectangle(brush, x, y - height, barWidth, height);
                            }
                            break;
                    }
                }, SourceRect);

                NewLayerRender?.Invoke(EffectLayer.GetBitmap());
                return EffectLayer;
            }
            catch (Exception exc)
            {
                Global.logger.Error("Error encountered in the Equalizer layer. Exception: " + exc);
                return EffectLayer.EmptyLayer;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var buffer = e.Buffer;
            var bytesRecorded = e.BytesRecorded;

            // 4 bytes per channel, bufferIncrement is numChannels * 4
            for (var index = 0; index < bytesRecorded; index += _bufferIncrement) // Loop over the bytes, respecting the channel grouping
            {
                _sampleAggregator.Add(_channels == 2
                    ? Math.Max(BitConverter.ToSingle(buffer, index), BitConverter.ToSingle(buffer, index + 4))
                    : BitConverter.ToSingle(buffer, index));
            }
        }

        private void FftCalculated(object sender, FftEventArgs e)
        {
            _fftsPrev = _ffts;
            _ffts = new List<Complex>(e.Result).ToArray();
        }

        private int FreqToBin(float freq)
        {
            return (int)(freq / (44000 / _ffts.Length));
        }

        private Brush GetBrush(float value, float position, float maxPosition)
        {
            switch (Properties.ViewType)
            {
                case EqualizerPresentationType.AlternatingColor:
                    return value >= 0 ? new SolidBrush(Properties.PrimaryColor) : new SolidBrush(Properties.SecondaryColor);
                case EqualizerPresentationType.GradientNotched:
                    return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(position, maxPosition));
                case EqualizerPresentationType.GradientHorizontal:
                {
                    var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                        start = PointF.Empty,
                        end = new PointF(SourceRect.Width, 0)
                    };

                    return eBrush.GetDrawingBrush();
                }
                case EqualizerPresentationType.GradientColorShift:
                    return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(Time.GetMilliSeconds(), 1000));
                case EqualizerPresentationType.GradientVertical:
                {
                    var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                        start = new PointF(0, SourceRect.Height),
                        end = PointF.Empty
                    };

                    return eBrush.GetDrawingBrush();
                }
                case EqualizerPresentationType.SolidColor:
                default:
                    return new SolidBrush(Properties.PrimaryColor);
            }
        }

        public override void Dispose()
        {
            if (_deviceProxy != null)
            {
                _deviceProxy.WaveInDataAvailable -= OnDataAvailable;
                _deviceProxy.Dispose();
                _deviceProxy = null;
            }
        }
    }

    public class SampleAggregator
    {
        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFft { get; set; }

        // This Complex is NAudio's own! 
        private readonly Complex[] _fftBuffer;
        private readonly FftEventArgs _fftArgs;
        private int _fftPos;
        private readonly int _fftLength;

        public SampleAggregator(int fftLength)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }

            _fftLength = fftLength;
            _fftBuffer = new Complex[fftLength];
            _fftArgs = new FftEventArgs(_fftBuffer);
        }

        private bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public void Add(float value)
        {
            if (!PerformFft || FftCalculated == null) return;
            // Remember the window function! There are many others as well.
            _fftBuffer[_fftPos].X = (float)(value * FastFourierTransform.HannWindow(_fftPos, _fftLength));
            _fftBuffer[_fftPos].Y = 0; // This is always zero with audio.
            _fftPos++;
            if (_fftPos < _fftLength) return;
            _fftPos = 0;
            //FastFourierTransform.FFT(true, m, fftBuffer);
            FftCalculated(this, _fftArgs);
        }
    }

    public class FftEventArgs : EventArgs
    {
        public FftEventArgs(Complex[] result)
        {
            Result = result;
        }
        public Complex[] Result { get; }
    }
}
