using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratios
{
    class Synthesizer
    {
        public double sinGenerator(double _time, float _frequency)
        {
            return Math.Sin(_time * 2 * Math.PI * _frequency);
        }
    }
}
