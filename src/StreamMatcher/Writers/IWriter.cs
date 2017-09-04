using System.Collections.Generic;

namespace StreamMatcher.Writers
{
    public interface IWriter
    {
        void Write(IReadOnlyList<byte> buffer, int offset, int count);
    }
}
