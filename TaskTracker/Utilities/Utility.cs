using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTracker.Utilities
{
    internal class Utility
    {

        public static void InfoMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n" + message);
            Console.ResetColor();
        }

        public static void HelpMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n" + message);
            Console.ResetColor();
        }

        public static void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + message);
            Console.ResetColor();
        }

        public static void CommandMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n" + message);
            Console.ResetColor();
        }

        public static void ClearConsole()
        {
            Console.Clear();
        }
    }
}
