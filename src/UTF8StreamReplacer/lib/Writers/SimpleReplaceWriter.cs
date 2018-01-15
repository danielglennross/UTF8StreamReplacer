using System.IO;
using System.Linq;

namespace UTF8StreamReplacer.lib.Writers
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
            // if we match part of seg
            if (IsAccumulativeMatch(_match, current))
            {
                PotentialStream.Add(current);
                if (IsAccumulativeMatchComplete(_match, current))
                {
                    MemoryStream.AddRange(_replace);
                    PotentialStream.Clear();
                }
                return;
            }

            // save potential, or normal
            if (PotentialStream.Any())
            {
                // didnt match, add what we got
                MemoryStream.AddRange(PotentialStream);
                PotentialStream.Clear();
            }

            MemoryStream.Add(current);            
        }
    }
}
