using System;
using System.Diagnostics;

namespace NoPony.Json
{
    // TODO: This is trash and should be replaced with something that sucks less
    internal static class Log
    {
        private static readonly object l = new object();
        private static readonly Stopwatch sw = Stopwatch.StartNew();

        internal static void Write(string Value)
        {
            lock(l)
            {
                Console.WriteLine($"{sw.ElapsedMilliseconds}: {Value}");
            }
        }
    }
}
