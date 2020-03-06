using System;

namespace DifferenceEngine
{
    class DiffListCharData
    {
        private readonly char[] _charList;

        public DiffListCharData(string charData)
        {
            _charList = charData.ToCharArray();
        }

        public int Count() => _charList.Length;
        public IComparable GetByIndex(int idx) => _charList[idx];

    }
}
