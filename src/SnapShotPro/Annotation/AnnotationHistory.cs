using System;
using System.Collections.Generic;
using System.Drawing;

namespace SnapShotPro.Annotation
{
    class AnnotationHistory
    {
        const int MaxSteps = 20;
        readonly List<Bitmap> _stack = new List<Bitmap>();
        int _pos = -1;

        public bool CanUndo => _pos > 0;
        public bool CanRedo => _pos < _stack.Count - 1;

        public void Push(Bitmap state)
        {
            // discard redo branch
            for (int i = _stack.Count - 1; i > _pos; i--)
            {
                _stack[i].Dispose();
                _stack.RemoveAt(i);
            }

            _stack.Add(Clone(state));
            _pos = _stack.Count - 1;

            // trim oldest
            if (_stack.Count > MaxSteps)
            {
                _stack[0].Dispose();
                _stack.RemoveAt(0);
                _pos--;
            }
        }

        public Bitmap Undo()
        {
            if (!CanUndo) return null;
            _pos--;
            return Clone(_stack[_pos]);
        }

        public Bitmap Redo()
        {
            if (!CanRedo) return null;
            _pos++;
            return Clone(_stack[_pos]);
        }

        public void Clear()
        {
            foreach (var b in _stack) b.Dispose();
            _stack.Clear();
            _pos = -1;
        }

        static Bitmap Clone(Bitmap src)
        {
            return new Bitmap(src);
        }
    }
}
