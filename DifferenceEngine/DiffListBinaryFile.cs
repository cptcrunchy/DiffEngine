using System;
using System.IO;

namespace DifferenceEngine
{
    class DiffListBinaryFile
    {
        private readonly byte[] _byteList;

        public DiffListBinaryFile(string fileName)
        {
            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int fileLength = (int)fs.Length;
            using BinaryReader br = new BinaryReader(fs);

            _byteList = br.ReadBytes(fileLength);
        }

        public int Count() => _byteList.Length;

        public IComparable GetByIndex(int idx) => _byteList[idx];
    }
}
