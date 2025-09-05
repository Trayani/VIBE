using System;

namespace GridDisplay
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GridGame())
                game.Run();
        }
    }
}