using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StreamMatcher
{
    using StringReplacer = Replacer<string>;
    using ByteReplacer = Replacer<byte[]>;

    public delegate T Replacer<T>(T input);

    public class UTf8StreamReplacer : Stream
    {
        private Stream _stream;
        private byte[] _startDelimiter;
        private byte[] _endDelimiter;
        private Replacer<string> _stringReplacer;
        private Replacer<byte[]> _byteReplacer;

        private readonly byte[] _match;
        private readonly byte[] _replace;

        private readonly List<byte> _memoryStream = new List<byte>();
        private readonly List<byte> _potentialStream = new List<byte>();

        public UTf8StreamReplacer(Stream stream, string match, string replace)
            : this(stream, Encoding.UTF8.GetBytes(match), Encoding.UTF8.GetBytes(replace))
        { }

        public UTf8StreamReplacer(Stream stream, byte[] match, byte[] replace)
        {
            _stream = stream;
            _match = match;
            _replace = replace;
        }

        public UTf8StreamReplacer(Stream stream, StringReplacer replacer, string delimiter)
            : this(stream, replacer, delimiter, delimiter)
        { }

        public UTf8StreamReplacer(Stream stream, ByteReplacer replacer, byte[] delimiter)
            : this(stream, replacer, delimiter, delimiter)
        { }

        public UTf8StreamReplacer(Stream stream, StringReplacer replacer, string startDelimiter, string endDelimiter)
        {
            Init(stream, replacer, Encoding.UTF8.GetBytes(startDelimiter), Encoding.UTF8.GetBytes(endDelimiter));
        }

        public UTf8StreamReplacer(Stream stream, ByteReplacer replacer, byte[] startDelimiter, byte[] endDelimiter)
        {
            Init(stream, replacer, startDelimiter, endDelimiter);
        }

        private void Init<T>(Stream stream, Replacer<T> replacer, byte[] startDelimiter, byte[] endDelimiter)
        {
            _stream = stream;
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;

            if (replacer is StringReplacer)
            {
                _stringReplacer = replacer as StringReplacer;
            }
            if (replacer is ByteReplacer)
            {
                _byteReplacer = replacer as ByteReplacer;
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin direction) => _stream.Seek(offset, direction);

        public override void SetLength(long length) => _stream.SetLength(length);

        public override void Close() => _stream.Close();

        public override void Flush()
        {
            if (_potentialStream.Any())
            {
                _stream.Write(_potentialStream.ToArray(), 0, _potentialStream.Count);
            }
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_startDelimiter != null && _endDelimiter != null)
            {
                WriteDelimited(buffer, offset, count);
            }

            if (_match != null && _replace != null)
            {
                WriteReplace(buffer, offset, count);
            }
        }

        public void WriteReplace(byte[] buffer, int offset, int count)
        {
            Func<int, bool> isPotential = (currentBufferIndex) =>
            {
                for (var i = 0; i < _match.Length; i++)
                {
                    if (_potentialStream.Count == i && _match[i] == buffer[currentBufferIndex])
                    {
                        return true;
                    }
                }
                return false;
            };

            for (var i = offset; i < offset + count; i++)
            {
                if (isPotential(i))
                {
                    _potentialStream.Add(buffer[i]);
                }
                else
                {// save potential, or normal
                    if (_potentialStream.Any())
                    {
                        if (_potentialStream.SequenceEqual(_match))
                        {// if matched
                            _memoryStream.AddRange(_replace);
                        }
                        else
                        {// didnt match, add what we got
                            _memoryStream.AddRange(_potentialStream);
                        }

                        _potentialStream.Clear();
                    }

                    _memoryStream.Add(buffer[i]);
                }
            }

            _stream.Write(_memoryStream.ToArray(), 0, _memoryStream.Count);
            _memoryStream.Clear();
        }

        public void WriteDelimited(byte[] buffer, int offset, int count)
        {
            Func<int, bool> isPotential = (currentBufferIndex) =>
            {
                for (var i = 0; i < _startDelimiter.Length; i++)
                {
                    if (_potentialStream.Count == i && _startDelimiter[i] == buffer[currentBufferIndex])
                    {
                        return true;
                    }
                }
                return false;
            };

            Func<int, bool> isClosed = (currentBufferIndex) =>
            {
                if (_potentialStream.Count < _endDelimiter.Length)
                {
                    return false;
                }

                var eCount = _endDelimiter.Length - 1;
                var pCount = _potentialStream.Count - 1;

                for (var i = 0; i < _endDelimiter.Length; i++)
                {
                    if (_endDelimiter[eCount - i] != _potentialStream[pCount - i])
                    {
                        return false;
                    }
                }
                return true;
            };

            for (var i = offset; i < offset + count; i++)
            {
                if (_potentialStream.Count >= _startDelimiter.Length)
                {// in segment
                    _potentialStream.Add(buffer[i]);

                    if (isClosed(i))
                    {// end of segment

                        var replacement = default(byte[]);
                        if (_stringReplacer != null)
                        {
                            var potentialStr = Encoding.UTF8.GetString(_potentialStream.ToArray());
                            var replacementStr = _stringReplacer(potentialStr);
                            replacement = Encoding.UTF8.GetBytes(replacementStr);
                        }

                        if (_byteReplacer != null)
                        {
                            replacement = _byteReplacer(_potentialStream.ToArray());
                        }

                        _memoryStream.AddRange(replacement);
                        _potentialStream.Clear();
                    }
                }
                else if (isPotential(i))
                {
                    _potentialStream.Add(buffer[i]);
                }
                else
                {// cancel potential, or normal
                    if (_potentialStream.Any())
                    {
                        _memoryStream.AddRange(_potentialStream);
                        _potentialStream.Clear();
                    }

                    _memoryStream.Add(buffer[i]);
                }
            }

            _stream.Write(_memoryStream.ToArray(), 0, _memoryStream.Count);
            _memoryStream.Clear();
        }
    }
}
