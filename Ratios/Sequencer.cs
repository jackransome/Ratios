using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

struct Note
{
    public float frequency;
    public int startTime;
    public int duration;
    public Note(float _frequency, int _startTime, int _duration)
    {
        frequency = _frequency;
        startTime = _startTime;
        duration = _duration;
    }
}

namespace Ratios
{
    class Sequencer
    {
        public List<Note> notes = new List<Note>();
        public void addNote(float _frequency, int _startTime, int _duration)
        {
            notes.Add(new Note(_frequency, _startTime, _duration));
        }
        public void removeNote(int _index)
        {
            notes.RemoveAt(_index);
        }
    }
}
