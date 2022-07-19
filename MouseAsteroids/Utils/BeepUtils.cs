using System;
using System.Threading.Tasks;

namespace MouseAsteroids.Utils
{
    public static class BeepUtils
    {
        public static void Beep(int frequency, int duration)
        {
            new Task(() => Console.Beep(frequency, duration)).Start();
        }
    }
}
