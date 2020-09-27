using System;

namespace YanivDesktop
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Yaniv())
                game.Run();
        }
    }
}
