using System.Collections.Generic;
using System.Linq;

namespace UTF8StreamReplacer.Specs
{
    public static class Helpers
    {
        public static IEnumerable<T[]> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }
    }
}
