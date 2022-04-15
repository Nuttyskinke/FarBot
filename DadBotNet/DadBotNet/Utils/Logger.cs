using DadBotNet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Utils
{
    public static class Logger
    {
        private static readonly ReadOnlyDictionary<LoggerLevel, ConsoleColor> _levelToColor = new ReadOnlyDictionary<LoggerLevel, ConsoleColor>(new Dictionary<LoggerLevel, ConsoleColor>() {
            { LoggerLevel.Info, ConsoleColor.Green },
            { LoggerLevel.Warning, ConsoleColor.Yellow },
            { LoggerLevel.Error, ConsoleColor.Red }
        });
        static bool _debugEnabled = false;
        static Logger()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            ConfigService config = new ConfigService();
            _debugEnabled = config.GetField("enableDebugInfo").ToLower() == "true";
        }

        public static void Log(string message, LoggerLevel level = LoggerLevel.Info)
        {
            if(_debugEnabled)
            {
                PrintInfoMessage(message, level);
            }
        }

        public static void Log(object message, LoggerLevel level = LoggerLevel.Info)
        {
            Log(message.ToString(), level);
        }

        private static void PrintInfoMessage(string message, LoggerLevel level)
        {
            Console.Write($"[{GetCurrentTime()}] ");

            Console.ForegroundColor = _levelToColor[level];

            Console.Write($"{level.ToString().ToUpper()}");

            Console.ResetColor();

            Console.Write(" | ");

            Console.WriteLine(message);
        }

        private static TimeSpan GetCurrentTime()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;

            time = new TimeSpan(time.Hours, time.Minutes, time.Seconds);

            return time;
        }
    }
}
