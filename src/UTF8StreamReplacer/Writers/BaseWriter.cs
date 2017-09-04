using System;
using System.Collections.Generic;
using System.IO;

namespace UTF8StreamReplacer.Writers
{
    internal abstract class BaseWriter : IWriter
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

        public void Write(IReadOnlyList<byte> buffer, int offset, int count)
        {
            var length = offset + count;

            if (buffer.Count < length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (var i = offset; i < length; i++)
            {
                ProcessByte(buffer[i]);
            }

            Stream.Write(MemoryStream.ToArray(), 0, MemoryStream.Count);
            MemoryStream.Clear();
        }

        public virtual void Flush()
        {
            Stream.Flush();   
        }

        protected abstract void ProcessByte(byte current);
    }
}
