using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratios
{
    class Synthesizer
    {
        public double WaveGenerator(double _time, float _frequency, int _waveForm)
        {
            double phase = (_time * _frequency) - Math.Floor(_time * _frequency);
            switch (_waveForm)
            {
                case 0:
                    return SinWave(PhaseModulator(phase));
                case 1:
                    return SawWave(PhaseModulator(phase));
                case 2:
                    return SquareWave(PhaseModulator(phase));
                case 3:
                    return TriangleWave(PhaseModulator(phase));
                default:
                    return SinWave(phase);
            }
        }
        private double SinWave(double _phase)
        {
            return Math.Sin(_phase * 2 * Math.PI);
        }
        private double SawWave(double _phase)
        {
            return 1 - _phase * 2;
        }
        private double SquareWave(double _phase)
        {
            if (_phase < 0.5)
            {
                return 1;
            } else
            {
                return -1;
            }
        }
        private double TriangleWave(double _phase)
        {
            if (_phase < 0.5)
            {
                return -1 + 4 * _phase;
            }
            else
            {
                return 1 - (_phase - 0.5) * 4;
            }
        }
        private double PhaseModulator(double _phase)
        {
            return Math.Pow(_phase, 2);
        }
    }
}