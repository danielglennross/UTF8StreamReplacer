using System.Collections.Generic;

namespace UTF8StreamReplacer.lib.Writers
{
    internal interface IWriter
    {
        void Write(IReadOnlyList<byte> buffer, int offset, int count);
        void Flush();
    }
}
