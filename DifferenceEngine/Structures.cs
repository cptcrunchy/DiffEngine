using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DifferenceEngine
{
    // Interface
    public interface IDiffList
    {
        int Count();
        IComparable GetByIndex(int idx);
    }

    public enum DiffResultSpanStatus
    {
        NoChange,
        Replace,
        DeleteSource,
        AddDestination
    }

    internal enum DiffStatus
    {
        Matched = 1,
        NoMatch = -1,
        Unknown = -2
    }

    internal class DiffState
    {
        // Props
        private const int BAD_INDEX = -1;
        private int _length;

        public int StartIndex { get; private set; }

        public int EndIndex { get { return StartIndex + _length - 1;}}

        public int Length
        {
            get
            {
                int len;
                if (_length > 0)
                    len = _length;
                else
                    len = (_length == 0) ? 1 : 0;
                
                return len;
            }
        }

        // CTOR
        public DiffState()
        {
            SetStatusToUnknown();
        }

        public DiffStatus Status
        {
            get
            {
                DiffStatus status;
               
                if (_length > 0)
                {
                    status = DiffStatus.Matched;
                }
                else
                {

                    if (_length == -1)
                    {
                        status = DiffStatus.NoMatch;
                    }
                    else
                    { 
                        Debug.Assert(_length == -2, "Invalid status: _length < -2");
                        status = DiffStatus.Unknown;
                    }
                }

                return status;
            }
        }

        protected void SetStatusToUnknown()
        {
            StartIndex = BAD_INDEX;
            _length = (int)DiffStatus.Unknown;
        }

        public void SetStatusToMatch(int start, int length)
        {
            Debug.Assert(start >= 0, "Start must be greater than or equal to zero.");
            Debug.Assert(length > 0, "Length must be greater than zero.");

            StartIndex = start;
            _length = length;
        }

        public void SetStatusToNoMatch()
        {
            StartIndex = BAD_INDEX;
            _length = (int)DiffStatus.NoMatch;
        }

        public bool HasValidLength(int start, int end, int maxPossibleLength)
        {
            if (_length > 0 && (maxPossibleLength < _length || StartIndex < start || EndIndex > end)) SetStatusToUnknown();
            return _length != (int)DiffStatus.Unknown;
        }
    }

    internal class DiffStateList
    {
        private readonly DiffState[] _diffStates;

        public DiffStateList(int destinationCount)
        {
            _diffStates = new DiffState[destinationCount];
        }

        public DiffState GetByIndex(int idx)
        {
            DiffState diff = _diffStates[idx];

            if (diff == null) diff = new DiffState(); _diffStates[idx] = diff;

            return diff;
        }

    }

    public class DiffResultSpan : IComparable
    {
        private const int BAD_INDEX = -1;
        private int _length;

        public int DestIndex { get; }
        public int SourceIndex { get; }
        public int Length { get { return _length; } }
        public DiffResultSpanStatus Status { get; }

        protected DiffResultSpan(
            DiffResultSpanStatus status,
            int destIndex,
            int sourceIndex,
            int length)
        {
            Status = status;
            DestIndex = destIndex;
            SourceIndex = sourceIndex;
            _length = length;
        }

        public static DiffResultSpan CreateNoChange(int destIndex, int sourceIndex, int length) => new DiffResultSpan(DiffResultSpanStatus.NoChange, destIndex, sourceIndex, length);
        
        public static DiffResultSpan CreateReplace(int destIndex, int sourceIndex, int length) => new DiffResultSpan(DiffResultSpanStatus.Replace, destIndex, sourceIndex, length);

        public static DiffResultSpan CreateDeleteSource(int sourceIndex, int length) => new DiffResultSpan(DiffResultSpanStatus.DeleteSource, BAD_INDEX, sourceIndex, length);

        public static DiffResultSpan CreateAddDestination(int destIndex, int length) => new DiffResultSpan(DiffResultSpanStatus.AddDestination, destIndex, BAD_INDEX, length);

        public void AddLength(int i)
        {
            _length += i;
        }

        public override string ToString()
        {
            return string.Format("{0} (Dest: {1},Source: {2}) {3}",
                Status.ToString(),
                DestIndex.ToString(),
                SourceIndex.ToString(),
                _length.ToString());
        }

        #region IComparable Members
        public int CompareTo(object obj) => DestIndex.CompareTo(((DiffResultSpan)obj).DestIndex);
        #endregion

    }
}
