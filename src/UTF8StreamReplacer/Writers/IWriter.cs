using System.Collections.Generic;

namespace UTF8StreamReplacer.Writers
{
    public interface IWriter
    {
        void Write(IReadOnlyList<byte> buffer, int offset, int count);
        void Flush();
    }
}
