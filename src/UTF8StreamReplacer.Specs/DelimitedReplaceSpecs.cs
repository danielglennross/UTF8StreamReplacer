using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTF8StreamReplacer.Specs
{
    public class DelimitedReplaceSpecs
    {
        public void TestDelimitedReplace(int bufferSize, string input, string expected)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var memoryStream = new MemoryStream();
            var testStream = new UTf8StreamReplacer(memoryStream, (str) =>
            {
                if (str == "%%{testing}}") return "HelloWorld";
                if (str == "%%{gfhj}}") return "Hello";
                return null;
            }, "%%{", "}}");

            var chunks = bytes.Split(10);
            foreach (var c in chunks)
            {
                testStream.Write(c, 0, c.Length);
            }
            testStream.Flush();

            var result = Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
