using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            if (PotentialStream.Any())
            {
                Stream.Write(PotentialStream.ToArray(), 0, PotentialStream.Count);
            }
        }

        protected abstract void ProcessByte(byte current);

        protected bool IsAccumulativeMatch(byte[] input, byte current)
        {
            // get our latest PotentialStream count - use this to get the corresponding input byte
            // if this input byte matches our current one, we're still accumulating
            return input.Where((b, i) => PotentialStream.Count == i && b == current).Any();
        }

        protected bool IsAccumulativeMatchComplete(byte[] input, byte current)
        {
            // we're still accumulating if PotentialStream isn't long enough to match
            if (PotentialStream.Count < input.Length)
            {
                return false;
            }

            var eCount = input.Length - 1;
            var pCount = PotentialStream.Count - 1;

            // trace backwards down our input - if we arrive at a byte that doesn't match 
            // the corresponding PotentialStream byte we're still accumulating
            return !input.Where((_, i) => input[eCount - i] != PotentialStream[pCount - i]).Any();
        }
    }
}
