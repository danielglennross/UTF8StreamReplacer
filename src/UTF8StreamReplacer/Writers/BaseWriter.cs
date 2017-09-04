using System.Collections.Generic;
using System.IO;

namespace UTF8StreamReplacer.Writers
{
    public abstract class BaseWriter : IWriter
    {
        protected readonly Stream Stream;
        protected readonly List<byte> PotentialStream;
        protected readonly List<byte> MemoryStream;

        protected BaseWriter(Stream stream)
        {
            Stream = stream;
            PotentialStream = new List<byte>();
            MemoryStream = new List<byte>();
        }

        public abstract void Write(IReadOnlyList<byte> buffer, int offset, int count);
        public abstract void Flush();
    }
}
