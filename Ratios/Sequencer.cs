using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Note
{
    public float frequency;
    public float startTime; // in beats
    public float duration; // in beats
    public bool selected;
    public float volume;
    public string sampleName = null;
    public Note(float _frequency, float _startTime, float _duration, float _volume, string _sampleName)
    {
        volume = 0.1f;
        selected = false;
        frequency = _frequency;
        startTime = _startTime;
        duration = _duration;
        volume = _volume;
        sampleName = _sampleName;
    }
    public void setSelected(bool _selected)
    {
        selected = _selected;
    }
}

namespace Ratios
{
    class Sequencer
    {
        public float bpm;
        Synthesizer synthesizer = new Synthesizer();
        public List<Note> notes = new List<Note>();
        FileLoader fileLoader;
        public void attachFileLoader(FileLoader _fileLoader)
        {
            fileLoader = _fileLoader;
        }
        public void addNote(float _frequency, float _startTime, float _duration, float _volume, string _sampleName)
        {
            notes.Add(new Note(_frequency, _startTime, _duration, _volume, _sampleName));
        }
        public void removeNote(int _index)
        {
            notes.RemoveAt(_index);
        }
        public void setSelected(int _index, bool _selected)
        {
            notes[_index].setSelected(_selected);
        }
        public void deselectAll()
        {
            for (int i = 0; i < notes.Count; i++)
            {
                setSelected(i, false);
            }
        }

        public float getDisplacement(bool _channel, double _time)
        {
            //_time is in seconds
            //convert to beat:
            float beatTime = (float)((bpm/60)*_time);
            double output = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                if (beatTime >= notes[i].startTime)
                {
                    if (beatTime < notes[i].startTime + notes[i].duration)
                    {
                        //adds the displacement from the note at the current time to the final sequencer output
                        if (notes[i].sampleName != null)
                        {
                            //if (_channel)
                            //{
                                output += fileLoader.readSample(notes[i].sampleName, (float)((44100 * (beatTime - notes[i].startTime) * 60 / bpm) * notes[i].frequency / 261.63));
                            //}
                            //else
                            //{
                                //output += fileLoader.readSample(notes[i].sampleName, (int)((44100 * (beatTime - notes[i].startTime) * 60 / bpm) * notes[i].frequency / 261.63));
                            //}
                        } else
                        {
                            output += synthesizer.sinGenerator(beatTime, notes[i].frequency) * notes[i].volume;
                        }
                    }
                }
            }
            return (float)output;
        }
    }
}