using System;
using System.Collections;
using System.IO;

namespace DifferenceEngine
{

    public class TextLine : IComparable
    {
        public string Line;
        public int Hash;

        public TextLine(string line)
        {
            Line = line.Replace("\t", "    ");
            Hash = line.GetHashCode();
        }

        public int CompareTo(object o) => Hash.CompareTo(((TextLine)o).Hash);
    }

    public class DiffListTextFile : IDiffList
    {
        private const int MaxLineLength = 1024;
        private readonly ArrayList _lines;

        public DiffListTextFile(string fileName)
        {
            _lines = new ArrayList();

            using StreamReader sr = new StreamReader(fileName);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > MaxLineLength) throw new InvalidOperationException(string.Format("File contains a line greater than {0} characters.", MaxLineLength.ToString()));

                _lines.Add(new TextLine(line));
            }
        }

        public int Count() => _lines.Count;

        public IComparable GetByIndex(int idx) => (TextLine)_lines[idx];
    }
}
