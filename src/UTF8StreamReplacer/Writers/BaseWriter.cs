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

        /// <summary>
        /// Each byte is considered in turn.
        /// If a byte is not part of a match, add it to MemoryStream.
        /// If a byte is part of a match, add it to PotentialStream.
        /// 
        /// If a match is complete (PotentialStream fulfilled), perform any replace logic on PotentialStream & write it to MemoryStream.
        /// If a match is aborted, write the contents of PotentialStream to MemoryStream.
        /// Clear PotentialStream after use.
        /// 
        /// PotentialStream is used to record potential matches across buffer boundaries
        /// (i.e. the match may start at the end of one buffer and flow into the next)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
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
