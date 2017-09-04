using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.Writers
{
    public class SimpleReplaceWriter : BaseWriter
    {
        private readonly byte[] _match;
        private readonly byte[] _replace;

        public SimpleReplaceWriter(Stream stream, byte[] match, byte[] replace)
            : base(stream)
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
                        MemoryStream.AddRange(_replace);
                    }
                    else
                    {// didnt match, add what we got
                        MemoryStream.AddRange(PotentialStream);
                    }

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
                // is potential stream a match
                if (PotentialStream.SequenceEqual(_match))
                {
                    Stream.Write(_replace.ToArray(), 0, _replace.Length);
                }
                else
                {
                    Stream.Write(PotentialStream.ToArray(), 0, PotentialStream.Count);
                }         
            }
            Stream.Flush();
        }
    }
}
