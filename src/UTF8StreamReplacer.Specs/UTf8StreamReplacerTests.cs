using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UTF8StreamReplacer.Specs
{
    [TestClass]
    public class UTf8StreamReplacerTests
    {
        [TestMethod]
        public void UTf8StreamReplacer_SimpleReplaceTest_Offset()
        {
            var cases = new[]
            {
                new { Offset = 0, Count = 23, Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "123456789Daniel123456789" },
                new { Offset = 0, Count = 12, Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "123456789Hel" },
                new { Offset = 5, Count = 12, Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "6789Daniel123" },
                new { Offset = 8, Count = 6,  Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "9Daniel" },
            }
            .ToList();

            cases.ForEach(val =>
            {
                var bytes = Encoding.UTF8.GetBytes(val.Input);
                var memoryStream = new MemoryStream();
                var testStream = new UTf8StreamReplacer(memoryStream, val.Match, val.Replace);
   
                testStream.Write(bytes, val.Offset, val.Count);
                testStream.Flush();

                var result = Encoding.UTF8.GetString(memoryStream.ToArray());

                Assert.AreEqual(val.Expected, result);
            });
        }

        [TestMethod]
        public void UTf8StreamReplacer_DelimitedReplaceTest_Offset()
        {
            var cases = new[]
            {
                new { Offset = 0, Count = 25, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456789{Hello}123456789", Expected = "123456789Daniel123456789" },
                new { Offset = 0, Count = 12, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456789{Hello}123456789", Expected = "123456789{He" },
                new { Offset = 5, Count = 12, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456789{Hello}123456789", Expected = "6789Daniel1" },
                new { Offset = 8, Count = 8,  StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456789{Hello}123456789", Expected = "9Daniel" },
            }
            .ToList();

            cases.ForEach(val =>
            {
                var bytes = Encoding.UTF8.GetBytes(val.Input);
                var memoryStream = new MemoryStream();
                var testStream = new UTf8StreamReplacer(memoryStream, val.Replacer, val.StartDelim, val.EndDelim);

                testStream.Write(bytes, val.Offset, val.Count);
                testStream.Flush();

                var result = Encoding.UTF8.GetString(memoryStream.ToArray());

                Assert.AreEqual(val.Expected, result);
            });
        }

        [TestMethod]
        public void UTf8StreamReplacer_SimpleReplaceTest()
        {
            var cases = new[]
            {
                // varying buffer sizes
                new { BufferSize = 5,  Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "123456789Daniel123456789" },
                new { BufferSize = 10, Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "123456789Daniel123456789" },
                new { BufferSize = 20, Match = "Hello", Replace = "Daniel", Input = "123456789Hello123456789", Expected = "123456789Daniel123456789" },

                // diff match positions
                new { BufferSize = 10, Match = "Hello", Replace = "Daniel", Input = "Hello123456789", Expected = "Daniel123456789" },
                new { BufferSize = 10, Match = "Hello", Replace = "Daniel", Input = "123456789Hello", Expected = "123456789Daniel" },

                // partial matching
                new { BufferSize = 10, Match = "Hello", Replace = "Daniel", Input = "HHeHelHellHello",     Expected = "HHeHelHellDaniel" },
                new { BufferSize = 10, Match = "Hello", Replace = "Daniel", Input = "HHeHelHellHelloHell", Expected = "HHeHelHellDanielHell" },
            }
            .ToList();

            cases.ForEach(val =>
            {
                var bytes = Encoding.UTF8.GetBytes(val.Input);
                var memoryStream = new MemoryStream();
                var testStream = new UTf8StreamReplacer(memoryStream, val.Match, val.Replace);

                var chunks = bytes.Split(val.BufferSize);
                foreach (var c in chunks)
                {
                    testStream.Write(c, 0, c.Length);
                }
                testStream.Flush();

                var result = Encoding.UTF8.GetString(memoryStream.ToArray());

                Assert.AreEqual(val.Expected, result);
            });
        }

        [TestMethod]
        public void UTf8StreamReplacerTest_DelimitedReplaceTest()
        {
            var cases = new[]
            {
                // varying buffer sizes
                new { BufferSize = 5,  StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{Hello}123456", Expected = "123456Daniel123456" },
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{Hello}123456", Expected = "123456Daniel123456" },
                new { BufferSize = 20, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{Hello}123456", Expected = "123456Daniel123456" },

                // varying start delimiter
                new { BufferSize = 10, StartDelim = "{{",  EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{{Hello}123456",  Expected = "123456Daniel123456" },
                new { BufferSize = 10, StartDelim = "{{{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{{{Hello}123456", Expected = "123456Daniel123456" },

                // varying end delimiter
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}}",  Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{Hello}}123456",  Expected = "123456Daniel123456" },
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}}}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456{Hello}}}123456", Expected = "123456Daniel123456" },

                // diff delimiter positions
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "{Hello}123456789", Expected = "Daniel123456789" },
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "123456789{Hello}", Expected = "123456789Daniel" },

                // partial delimiter matches
                new { BufferSize = 10, StartDelim = "{{", EndDelim = "}}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "1{123{{Hello}}123", Expected = "1{123Daniel123" },
                new { BufferSize = 10, StartDelim = "{{", EndDelim = "}}", Replacer = (StringReplacer)((str) => "Daniel"), Input = "1{{Hello}123}}123", Expected = "1Daniel123"     },

                // varying replacers
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => str == "Hello" ? "Ben" : ""),                          Input = "123456{Hello}123456",        Expected = "123456Ben123456"    },
                new { BufferSize = 10, StartDelim = "{", EndDelim = "}", Replacer = (StringReplacer)((str) => str == "Hello" ? "Ben" : str == "World" ? "Bob" : ""), Input = "123456{Hello}123456{World}", Expected = "123456Ben123456Bob" },
            }
            .ToList();

            cases.ForEach(val =>
            {
                var bytes = Encoding.UTF8.GetBytes(val.Input);
                var memoryStream = new MemoryStream();
                var testStream = new UTf8StreamReplacer(memoryStream, val.Replacer, val.StartDelim, val.EndDelim);

                var chunks = bytes.Split(val.BufferSize);
                foreach (var c in chunks)
                {
                    testStream.Write(c, 0, c.Length);
                }
                testStream.Flush();

                var result = Encoding.UTF8.GetString(memoryStream.ToArray());

                Assert.AreEqual(val.Expected, result);
            });
        }
    }
}