using System;
using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.Writers
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
            Func<bool> isPotential = () =>
            {
                return _startDelimiter.Where((t, i) => PotentialStream.Count == i && t == current).Any();
            };

            Func<bool> isClosed = () =>
            {
                if (PotentialStream.Count < _endDelimiter.Length)
                {
                    return false;
                }

                var eCount = _endDelimiter.Length - 1;
                var pCount = PotentialStream.Count - 1;

                return !_endDelimiter.Where((t, i) => _endDelimiter[eCount - i] != PotentialStream[pCount - i]).Any();
            };
  
            // if in segment
            if (PotentialStream.Count >= _startDelimiter.Length)
            {
                PotentialStream.Add(current);
                if (isClosed())
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
            if (isPotential())
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

        public override void Flush()
        {
            // do we have waiting ponential stream at end
            if (PotentialStream.Any())
            {
                Stream.Write(PotentialStream.ToArray(), 0, PotentialStream.Count);   
            }
            base.Flush();
        }
    }
}
