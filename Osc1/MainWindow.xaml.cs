using System;
using System.Windows;

using NAudio.Wave;

namespace Osc1
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private DirectSoundOut output = null;
        private BlockAlignReductionStream stream = null;

        private WaveTone tone;

        private void button_Click(object sender, RoutedEventArgs e)
            //hallo
        {
            tone = new WaveTone(1000, 0.5);
            stream = new BlockAlignReductionStream(tone);

            output = new DirectSoundOut(40);
            output.Init(stream);
            output.Play();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                output.Stop();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (output != null)
            {
                output.Dispose();
                output = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
            if (tone != null)
            {
                tone.Dispose();
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tone != null)
                tone.frequency = sliderFrequency.Value;
        }

        private void sliderAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tone != null)
                tone.amplitude = sliderAmplitude.Value;
        }

        private void sliderTone_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tone != null)
            {
                tone.tone = (double)(sliderTone.Value / 100);
                label.Content = tone.lastCount.ToString();
            }
        }
    }

    public class WaveTone : WaveStream
    {
        public double frequency;
        public double amplitude;
        private double posInWaveTable;
        private short[] sineTable;
        private short[] triTable;
        private short[] sawTable;
        public double tone;
        public int lastCount;

        public WaveTone(double f, double a)
        {
            this.posInWaveTable = 0;
            this.frequency = f;
            this.amplitude = a;
            this.tone = 0;
            this.sineTable = new short[48000];
            this.triTable = new short[48000];
            this.sawTable = new short[48000];
            for (int i = 0; i < 48000; i++)
            {
                sineTable[i] = (short)Math.Round(Math.Sin(Math.PI * 2.0 * i / 48000.0) * (Math.Pow(2,15)-1));
                if (i < 24000)
                  triTable[i] = (short)Math.Round((i / 24000.0) * (Math.Pow(2, 15) - 1));
                else
                    triTable[i] = (short)Math.Round(((48000-i) / 24000.0) * (Math.Pow(2, 15) - 1));
                sawTable[i] = (short)Math.Round((i / 48000.0) * (Math.Pow(2, 15) - 1));
            }
        }

        public override long Position
        {
            get;
            set;
        }

        public override long Length
        {
            get { return long.MaxValue; }
        }

        public override WaveFormat WaveFormat
        {
            get { return new WaveFormat(48000, 16, 1); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int samples = count / 2;
            for (int i = 0; i < samples; i++)
            {
                posInWaveTable += frequency;
                if (posInWaveTable >= 48000)
                    posInWaveTable -= 48000;
                short o = (short)(triTable[(int)Math.Truncate(posInWaveTable)] * this.tone + sineTable[(int)Math.Truncate(posInWaveTable)] * (1-this.tone));
                o = (short)(o * amplitude);
                buffer[i * 2] = (byte)(o & 0x00ff);
                buffer[i * 2 + 1] = (byte)((o & 0xff00) >> 8);

            }
            lastCount = count;
            return count;
        }
    }
}
