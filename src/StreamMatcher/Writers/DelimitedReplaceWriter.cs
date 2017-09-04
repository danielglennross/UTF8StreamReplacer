using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamMatcher.Writers
{
    public class DelimitedReplaceWriter : BaseWriter
    {
        private readonly byte[] _startDelimiter;
        private readonly byte[] _endDelimiter;
        private readonly Replacer<byte[]> _byteReplacer;

        private readonly List<byte> _memoryStream = new List<byte>();

        public DelimitedReplaceWriter(Stream stream, List<byte> potentialStream, byte[] startDelimiter, byte[] endDelimiter, Replacer<byte[]> byteReplacer)
            : base(stream, potentialStream)
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

                        _memoryStream.AddRange(replacement);
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
                    _memoryStream.AddRange(PotentialStream);
                    PotentialStream.Clear();
                }

                _memoryStream.Add(buffer[i]);
            }

            Stream.Write(_memoryStream.ToArray(), 0, _memoryStream.Count);
            _memoryStream.Clear();
        }
    }
}
