using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.lib.Writers
{
    internal class DelimitedReplaceWriter : BaseWriter
    {
        private readonly byte[] _startDelimiter;
        private readonly byte[] _endDelimiter;
        private readonly ByteArrayReplacer _byteArrayReplacer;

        public DelimitedReplaceWriter(Stream stream, byte[] startDelimiter, byte[] endDelimiter, ByteArrayReplacer byteArrayReplacer)
            : base(stream)
        {
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;
            _byteArrayReplacer = byteArrayReplacer;
        }

        protected override void ProcessByte(byte current)
        {  
            // if in segment
            if (PotentialStream.Count >= _startDelimiter.Length)
            {
                PotentialStream.Add(current);
                if (IsAccumulativeMatchComplete(_endDelimiter, current))
                {
                    // end segment
                    // remove delimiters
                    PotentialStream.RemoveRange(0, _startDelimiter.Length);
                    PotentialStream.RemoveRange(PotentialStream.Count - _endDelimiter.Length, _endDelimiter.Length);

                    var replacement = _byteArrayReplacer(PotentialStream.ToArray());

                    MemoryStream.AddRange(replacement);
                    PotentialStream.Clear();
                }
                return;
            }

            // if we match part of seg
            if (IsAccumulativeMatch(_startDelimiter, current))
            {
                PotentialStream.Add(current);
                return;
            }

            // cancel potential if exists
            if (PotentialStream.Any())
            {
                MemoryStream.AddRange(PotentialStream);
                PotentialStream.Clear();
            }

            MemoryStream.Add(current);
        }
    }
}
