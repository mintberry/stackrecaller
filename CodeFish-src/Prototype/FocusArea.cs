using System;

namespace Prototype
{
	public class FocusArea
	{
        public delegate void FocusChangedDelegate(FocusArea focus);

        //Fields
        private int _start, _end, _center;

        //Constructors
		public FocusArea(int center, int start, int end)
		{
			this._center = center;
			this._start = start;
			this._end = end;
		}

        //Properties
        public int End { get { return _end; } }
        public int Start { get { return _start; } }
		public int Center { get { return _center; } }
		public int Lines { get { return _end - _start + 1; } }
		
		//Methods
        public bool IsInside(int line)
        {
            return Start <= line && line <= End;
        }
        
        public bool Before(int i)
        {
        	return i < _start;
        }
        
        public bool After(int i)
        {
        	return i > _end;
        }
        
        public int Distance(int i)
        {
        	if (Before(i))
        		return _start - i;
        	else if (After(i))
        		return i - _end;
        	else
        		return 0;
        }
        
        public int DistanceToCenter(int i)
        {
        	return Math.Abs(_center - i);
        }
        
        public FocusArea Clip(int size)
        {
        	int start = (_start < 0) ? 0 : _start;
        	int	end = (_end >= size) ? size - 1 : _end;
        	int center = _center;
        	
        	if (center > end) center = end;
        	if (center < start) center = start;
        	        	
        	return new FocusArea(center, start, end);
        }
	}
}
