using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratios
{
    class Sample
    {
        public string name;
        public double[] left;
        public double[] right;
    }
    class FileLoader
    {
        public List<Sample> samples = new List<Sample>();

        public void loadSample(string _name, string _fileName)
        {
            samples.Add(new Sample());
            openWav(_fileName, out samples[samples.Count - 1].left, out samples[samples.Count - 1].right);
            samples[samples.Count - 1].name = _name;
        }

        public double readSample(string _name, int _sampleTime)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i].name == _name)
                {
                    if (_sampleTime < samples[i].left.Length)
                    {
                        return samples[i].left[_sampleTime];
                    }
                    else
                    {
                        return 0;
                        //tried to read past end of sample
                    }
                }
            }
            //no sample found with this name
            return 0;
        }

        public double readSample(string _name, float _sampleTime, bool _loop)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i].name == _name)
                {
                    //looping:
                    if (_loop)
                    {
                        while (_sampleTime >= samples[i].left.Length - 1)
                        {
                            _sampleTime -= samples[i].left.Length - 1;
                        }
                    }
                    if (_sampleTime < samples[i].left.Length-1)
                    {
                        if (_sampleTime != Math.Floor(_sampleTime))
                        {
                            return samples[i].left[(int)Math.Floor(_sampleTime)] * (_sampleTime - Math.Floor(_sampleTime)) + samples[i].left[(int)Math.Ceiling(_sampleTime)] * (Math.Ceiling(_sampleTime) - _sampleTime);
                        } else
                        {
                            return samples[i].left[(int)_sampleTime];
                        }
                    }
                    else
                    {
                        return 0;
                        //tried to read past end of sample
                    }
                }
            }
            //no sample found with this name
            return 0;
        }

        // from https://stackoverflow.com/questions/8754111/how-to-read-the-data-in-a-wav-file-to-an-array

        // convert two bytes to one double in the range -1 to 1
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0;
        }

        // Returns left and right double arrays. right and left will be the same if the wav file is mono.
        public void openWav(string filename, out double[] left, out double[] right)
        {
            byte[] wav = System.IO.File.ReadAllBytes(filename);

            // Determine if mono or stereo
            int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            left = new double[samples];
            right = new double[samples];

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                left[i] = bytesToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channels == 2)
                {
                    right[i] = bytesToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                else
                {
                    right[i] = left[i];
                }
                i++;
            }
        }
    }
}
