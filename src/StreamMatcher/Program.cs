using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StreamMatcher
{ 
    class Program
    {
        static void Main(string[] args)
        {
            var input1 = "fghjfhghelloggfhdfghelldgfhellogh";
            var bytes1 = Encoding.UTF8.GetBytes(input1);
            var memoryStream1 = new MemoryStream();
            var testStream1 = new UTf8StreamReplacer(memoryStream1, "hello", "daniel");

            //testStream1.Write(bytes1, 3, 8);
            //testStream1.Flush();
            //var result2 = Encoding.UTF8.GetString(memoryStream1.ToArray());
            //if (result2 == "jfhghell")
            //{

            //}

            var chunks1 = Split(bytes1, 10);
            foreach (var c in chunks1)
            {
                testStream1.Write(c, 0, c.Length);
            }
            testStream1.Flush();

            var result1 = Encoding.UTF8.GetString(memoryStream1.ToArray());
            if (result1 == "fghjfhgdanielggfhdfghelldgfdanielgh")
            {

            }

            // test

            var input = "tegdh%%{testing}}jgj%gd%%od%%{gfhj}}fgf";
            var bytes = Encoding.UTF8.GetBytes(input);
            var memoryStream = new MemoryStream();
            var testStream = new UTf8StreamReplacer(memoryStream, (str) =>
            {
                if (str == "%%{testing}}") return "HelloWorld";
                if (str == "%%{gfhj}}") return "Hello";
                return null;
            }, "%%{", "}}");

            var chunks = Split(bytes, 10);
            foreach (var c in chunks)
            {
                testStream.Write(c, 0, c.Length);
            }
            testStream.Flush();

            var result = Encoding.UTF8.GetString(memoryStream.ToArray());
            if (result == "tegdhHelloWorldjgj%gd%%odHellofgf")
            {

            }
        }

        private static IEnumerable<T[]> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }
    }
}
