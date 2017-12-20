using System.IO;

namespace PokerBot
{
    public static class Log
    {
        public static void Write(params string[] messages) => File.AppendAllLines("log.txt", messages);
    }
}
