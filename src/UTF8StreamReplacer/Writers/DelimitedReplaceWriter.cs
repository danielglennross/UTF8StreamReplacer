using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.Writers
{
    public class DelimitedReplaceWriter : BaseWriter
    {
        private readonly byte[] _startDelimiter;
        private readonly byte[] _endDelimiter;
        private readonly Replacer<byte[]> _byteReplacer;

        public DelimitedReplaceWriter(Stream stream, byte[] startDelimiter, byte[] endDelimiter, Replacer<byte[]> byteReplacer)
            : base(stream)
        {
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;
            _byteReplacer = byteReplacer;
        }

        public override void Write(IReadOnlyList<byte> buffer, int offset, int count)
        {
            Func<int, bool> isPotential = (currentBufferIndex) =>
            {
                return _startDelimiter.Where((t, i) => PotentialStream.Count == i && t == buffer[currentBufferIndex]).Any();
            };

            Func<int, bool> isClosed = (currentBufferIndex) =>
            {
                if (PotentialStream.Count < _endDelimiter.Length)
                {
                    return false;
                }

                var eCount = _endDelimiter.Length - 1;
                var pCount = PotentialStream.Count - 1;

                return !_endDelimiter.Where((t, i) => _endDelimiter[eCount - i] != PotentialStream[pCount - i]).Any();
            };

            for (var i = offset; i < offset + count; i++)
            {
                // if in segment
                if (PotentialStream.Count >= _startDelimiter.Length)
                {
                    PotentialStream.Add(buffer[i]);
                    if (isClosed(i))
                    {
                        // end segment
                        // remove delimiters
                        PotentialStream.RemoveRange(0, _startDelimiter.Length);
                        PotentialStream.RemoveRange(PotentialStream.Count - _endDelimiter.Length, _endDelimiter.Length);

                        var replacement = _byteReplacer(PotentialStream.ToArray());

                        MemoryStream.AddRange(replacement);
                        PotentialStream.Clear();
                    }
                    continue;
                }

                // if we match part of seg
                if (isPotential(i))
                {
                    PotentialStream.Add(buffer[i]);
                    continue;
                }

                // cancel potential if exists
                if (PotentialStream.Any())
                {
                    MemoryStream.AddRange(PotentialStream);
                    PotentialStream.Clear();
                }

                MemoryStream.Add(buffer[i]);
            }

            Stream.Write(MemoryStream.ToArray(), 0, MemoryStream.Count);
            MemoryStream.Clear();
        }

        public override void Flush()
        {
            // do we have waiting ponential stream at end
            if (PotentialStream.Any())
            {
                Stream.Write(PotentialStream.ToArray(), 0, PotentialStream.Count);   
            }
            Stream.Flush();
        }
    }
}
