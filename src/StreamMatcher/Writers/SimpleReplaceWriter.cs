using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamMatcher.Writers
{
    public class SimpleReplaceWriter : BaseWriter
    {
        private readonly byte[] _match;
        private readonly byte[] _replace;

        private readonly List<byte> _memoryStream = new List<byte>();

        public SimpleReplaceWriter(Stream stream, List<byte> potentialStream, byte[] match, byte[] replace)
            : base(stream, potentialStream)
        {
            _match = match;
            _replace = replace;
        }

        public override void Write(IReadOnlyList<byte> buffer, int offset, int count)
        {
            Func<int, bool> isPotential = (currentBufferIndex) =>
            {
                return _match.Where((t, i) => PotentialStream.Count == i && t == buffer[currentBufferIndex]).Any();
            };

            for (var i = offset; i < offset + count; i++)
            {
                // if we match part of seg
                if (isPotential(i))
                {
                    PotentialStream.Add(buffer[i]);
                    continue;
                }

                // save potential, or normal
                if (PotentialStream.Any())
                {
                    if (PotentialStream.SequenceEqual(_match))
                    {// if matched
                        _memoryStream.AddRange(_replace);
                    }
                    else
                    {// didnt match, add what we got
                        _memoryStream.AddRange(PotentialStream);
                    }

                    PotentialStream.Clear();
                }

                _memoryStream.Add(buffer[i]);
            }

            Stream.Write(_memoryStream.ToArray(), 0, _memoryStream.Count);
            _memoryStream.Clear();
        }
    }
}
