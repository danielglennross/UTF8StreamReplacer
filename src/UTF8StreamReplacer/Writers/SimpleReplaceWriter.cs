using System;
using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.Writers
{
    internal class SimpleReplaceWriter : BaseWriter
    {
        private readonly byte[] _match;
        private readonly byte[] _replace;

        public SimpleReplaceWriter(Stream stream, byte[] match, byte[] replace)
            : base(stream)
        {
            _match = match;
            _replace = replace;
        }

        protected override void ProcessByte(byte current)
        {
            Func<bool> isPotential = () =>
            {
                return _match.Where((t, i) => PotentialStream.Count == i && t == current).Any();
            };

            // if we match part of seg
            if (isPotential())
            {
                PotentialStream.Add(current);
                return;
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

            MemoryStream.Add(current);            
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
            base.Flush();
        }
    }
}
