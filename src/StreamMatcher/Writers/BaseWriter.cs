using System.Collections.Generic;
using System.IO;

namespace StreamMatcher.Writers
{
    public abstract class BaseWriter : IWriter
    {
        protected readonly Stream Stream;
        protected readonly List<byte> PotentialStream;

        protected BaseWriter(Stream stream, List<byte> potentialStream)
        {
            Stream = stream;
            PotentialStream = potentialStream;
        }

        public abstract void Write(IReadOnlyList<byte> buffer, int offset, int count);
    }
}
