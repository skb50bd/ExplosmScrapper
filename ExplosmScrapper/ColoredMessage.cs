using System;
using static System.Console;
using static System.ConsoleColor;

namespace ExplosmScrapper
{
    public static class ConsolePrint
    {
        public static void WriteWithColor(string msg, ConsoleColor color)
        {
            var previousTextColor = ForegroundColor;
            ForegroundColor = color;
            WriteLine(msg);
            ForegroundColor = previousTextColor;
        }

        public static void WriteSuccess(string msg) =>
            WriteWithColor(msg, Green);

        public static void WriteError(string msg) =>
            WriteWithColor(msg, Red);

        public static void WriteWarning(string msg) =>
            WriteWithColor(msg, Yellow);

        public static void WriteInfo(string msg) =>
            WriteWithColor(msg, Blue);
    }
}