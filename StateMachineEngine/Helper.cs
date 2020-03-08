using System;
using System.IO;
using System.Threading.Tasks;

namespace StateMachineEngine
{
    public static class Helper
    {
        public static Task WaitSeconds(int seconds)
        {
            return Task.Delay(TimeSpan.FromSeconds(seconds));
        }
        public static string SaveStringToFile(string pathToFile, string content)
        {
            File.AppendAllText(pathToFile, content);
            return content;
        }
    }
}
