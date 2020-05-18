using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Note
{
    public float frequency;
    public int startTime;
    public int duration;
    public bool selected;
    public float volume;
    public Note(float _frequency, int _startTime, int _duration)
    {
        volume = 0.1f;
        selected = false;
        frequency = _frequency;
        startTime = _startTime;
        duration = _duration;
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
        Synthesizer synthesizer = new Synthesizer();
        public List<Note> notes = new List<Note>();
        public void addNote(float _frequency, int _startTime, int _duration)
        {
            notes.Add(new Note(_frequency, _startTime, _duration));
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

        public double getDisplacement(bool _channel, double _time)
        {
            double output = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                //deletes a note if it's already been played
                if (_time > notes[i].duration + notes[i].startTime)
                {
                    //notes.erase(notes.begin() + i);
                    //i--;
                }
                else if (_time >= notes[i].startTime)
                {
                    if (_time < notes[i].startTime + notes[i].duration)
                    {
                        //adds the displacement from the note at the current time to the final sequencer output
                        output += synthesizer.sinGenerator(_time, notes[i].frequency) * notes[i].volume;
                    }
                }
            }
            return output;
        }

    }
}