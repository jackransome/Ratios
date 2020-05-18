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
    public Note(float _frequency, int _startTime, int _duration)
    {
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
    }
}
