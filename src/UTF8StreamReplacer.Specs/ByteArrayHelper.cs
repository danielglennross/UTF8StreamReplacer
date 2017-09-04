using System.Collections.Generic;
using System.Linq;

namespace UTF8StreamReplacer.Specs
{
    internal static class ByteArrayHelper
    {
        public static IEnumerable<byte[]> Split(this byte[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }
    }
}
