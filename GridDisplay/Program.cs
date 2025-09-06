using System;

namespace GridDisplay
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0] == "test" || args[0] == "TestRangeOfVision"))
            {
                TestRangeOfVision.RunTest();
            }
            else
            {
                using (var game = new GridGame())
                    game.Run();
            }
        }
    }
}