using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GridDisplay
{
    public class TestRangeOfVision
    {
        public static void RunTest()
        {
            Console.WriteLine("Testing Range of Vision Algorithm");
            Console.WriteLine("==================================");
            
            var grid = new Grid(26, 27, 28, 20);
            
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    grid.SetCell(x, y, new GridCell(false, 0, 0));
                }
            }
            
            grid.SetCell(14, 14, new GridCell(true, 0, 0));
            
            Point viewerPosition = new Point(12, 10);
            
            var rangeOfVision = new RangeOfVision(grid);
            rangeOfVision.EnableDebug = true;
            var visibilityMap = rangeOfVision.CalculateVisibility(viewerPosition);
            
            // Define expected invisible cells based on case1.txt
            var expectedInvisible = new HashSet<(int, int)>();
            // Cells marked with ! in case1.txt
            expectedInvisible.Add((15, 14));
            expectedInvisible.Add((14, 15));
            expectedInvisible.Add((15, 15));
            expectedInvisible.Add((16, 15));
            expectedInvisible.Add((14, 16));
            expectedInvisible.Add((15, 16));
            expectedInvisible.Add((16, 16));
            expectedInvisible.Add((17, 16));
            expectedInvisible.Add((14, 17));
            expectedInvisible.Add((15, 17));
            expectedInvisible.Add((16, 17));
            expectedInvisible.Add((17, 17));
            expectedInvisible.Add((18, 17));
            expectedInvisible.Add((14, 18));
            expectedInvisible.Add((15, 18));
            expectedInvisible.Add((16, 18));
            expectedInvisible.Add((17, 18));
            expectedInvisible.Add((18, 18));
            expectedInvisible.Add((19, 18));
            expectedInvisible.Add((14, 19));
            expectedInvisible.Add((15, 19));
            expectedInvisible.Add((16, 19));
            expectedInvisible.Add((17, 19));
            expectedInvisible.Add((18, 19));
            expectedInvisible.Add((19, 19));
            expectedInvisible.Add((20, 19));
            expectedInvisible.Add((14, 20));
            expectedInvisible.Add((15, 20));
            expectedInvisible.Add((16, 20));
            expectedInvisible.Add((17, 20));
            expectedInvisible.Add((18, 20));
            expectedInvisible.Add((19, 20));
            expectedInvisible.Add((20, 20));
            expectedInvisible.Add((21, 20));
            expectedInvisible.Add((15, 21));
            expectedInvisible.Add((16, 21));
            expectedInvisible.Add((17, 21));
            expectedInvisible.Add((18, 21));
            expectedInvisible.Add((19, 21));
            expectedInvisible.Add((20, 21));
            expectedInvisible.Add((21, 21));
            expectedInvisible.Add((22, 21));
            expectedInvisible.Add((15, 22));
            expectedInvisible.Add((16, 22));
            expectedInvisible.Add((17, 22));
            expectedInvisible.Add((18, 22));
            expectedInvisible.Add((19, 22));
            expectedInvisible.Add((20, 22));
            expectedInvisible.Add((21, 22));
            expectedInvisible.Add((22, 22));
            expectedInvisible.Add((23, 22));
            expectedInvisible.Add((15, 23));
            expectedInvisible.Add((16, 23));
            expectedInvisible.Add((17, 23));
            expectedInvisible.Add((18, 23));
            expectedInvisible.Add((19, 23));
            expectedInvisible.Add((20, 23));
            expectedInvisible.Add((21, 23));
            expectedInvisible.Add((22, 23));
            expectedInvisible.Add((23, 23));
            expectedInvisible.Add((15, 24));
            expectedInvisible.Add((16, 24));
            expectedInvisible.Add((17, 24));
            expectedInvisible.Add((18, 24));
            expectedInvisible.Add((19, 24));
            expectedInvisible.Add((20, 24));
            expectedInvisible.Add((21, 24));
            expectedInvisible.Add((22, 24));
            expectedInvisible.Add((23, 24));
            expectedInvisible.Add((24, 24));
            expectedInvisible.Add((15, 25));
            expectedInvisible.Add((16, 25));
            expectedInvisible.Add((17, 25));
            expectedInvisible.Add((18, 25));
            expectedInvisible.Add((19, 25));
            expectedInvisible.Add((20, 25));
            expectedInvisible.Add((21, 25));
            expectedInvisible.Add((22, 25));
            expectedInvisible.Add((23, 25));
            expectedInvisible.Add((24, 25));
            expectedInvisible.Add((25, 25));
            expectedInvisible.Add((16, 26));
            expectedInvisible.Add((17, 26));
            expectedInvisible.Add((18, 26));
            expectedInvisible.Add((19, 26));
            expectedInvisible.Add((20, 26));
            expectedInvisible.Add((21, 26));
            expectedInvisible.Add((22, 26));
            expectedInvisible.Add((23, 26));
            expectedInvisible.Add((24, 26));
            expectedInvisible.Add((25, 26));
            
            Console.WriteLine("\nGrid with visibility (V=viewer, X=obstacle, .=visible, !=not visible, ?=should be invisible):");
            Console.WriteLine("    " + string.Join("", GetColumnHeaders(grid.Width)));
            
            for (int y = 0; y < Math.Min(grid.Height, 27); y++)
            {
                Console.Write($"{y,2}: ");
                for (int x = 0; x < grid.Width; x++)
                {
                    if (x == viewerPosition.X && y == viewerPosition.Y)
                    {
                        Console.Write("V ");
                    }
                    else if (grid.GetCell(x, y).Blocked)
                    {
                        Console.Write("X ");
                    }
                    else if (visibilityMap[x, y])
                    {
                        if (expectedInvisible.Contains((x, y)))
                        {
                            Console.Write("? ");  // Visible but should be invisible
                        }
                        else
                        {
                            Console.Write(". ");  // Correctly visible
                        }
                    }
                    else
                    {
                        Console.Write("! ");  // Not visible
                    }
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("\nExpected non-visible cells based on case1.txt:");
            CheckExpectedInvisible(15, 14, visibilityMap, "Right of obstacle");
            CheckExpectedInvisible(14, 15, visibilityMap, "Below obstacle");
            CheckExpectedInvisible(15, 15, visibilityMap, "Below-right of obstacle");
            CheckExpectedInvisible(16, 15, visibilityMap, "Further right on row 15");
            
            for (int y = 16; y <= 25; y++)
            {
                CheckExpectedInvisible(15, y, visibilityMap, $"Column 15, row {y}");
                CheckExpectedInvisible(16, y, visibilityMap, $"Column 16, row {y}");
                CheckExpectedInvisible(17, y, visibilityMap, $"Column 17, row {y}");
            }
            
            Console.WriteLine("\nTest cases from case1.txt:");
            Console.WriteLine("Obstacle at (14,14), Viewer at (12,10)");
            Console.WriteLine("Right border (DL corner): (13,15) - diff (1,5)");
            Console.WriteLine("Left border (UR corner): (15,13) - diff (3,3)");
            
            Console.WriteLine("\nRight border progression (based on 13:15 with diff 1:5): ");
            CheckCoordinateProgression(12, 10, 1, 5, 14, 20, visibilityMap);
            CheckCoordinateProgression(12, 10, 1, 5, 15, 25, visibilityMap);
            
            Console.WriteLine("\nLeft border progression (based on 15:13 with diff 3:3): ");
            CheckBorderPoint(15, 13, visibilityMap, "Left border start");
            CheckBorderPoint(18, 16, visibilityMap, "Left border at 18:16 (15+3, 13+3)");
            CheckBorderPoint(21, 19, visibilityMap, "Left border at 21:19 (15+6, 13+6)");
            CheckBorderPoint(24, 22, visibilityMap, "Left border at 24:22 (15+9, 13+9)");
            CheckBorderPoint(27, 25, visibilityMap, "Left border at 27:25 (15+12, 13+12)");
        }
        
        private static string[] GetColumnHeaders(int width)
        {
            string[] headers = new string[width];
            for (int i = 0; i < width; i++)
            {
                headers[i] = $"{i,2}";
            }
            return headers;
        }
        
        private static void CheckExpectedInvisible(int x, int y, bool[,] visibilityMap, string description)
        {
            bool isVisible = visibilityMap[x, y];
            string status = isVisible ? "FAIL" : "PASS";
            Console.WriteLine($"  [{status}] Cell ({x},{y}) should be invisible - {description}: {!isVisible}");
        }
        
        private static void CheckCoordinateProgression(int vx, int vy, int dx, int dy, int targetX, int targetY, bool[,] visibilityMap)
        {
            int steps = (targetX - vx) / dx;
            int calcX = vx + steps * dx;
            int calcY = vy + steps * dy;
            
            Console.WriteLine($"  V({vx},{vy}) + {steps}*({dx},{dy}) = ({calcX},{calcY}) - Expected: ({targetX},{targetY})");
            
            if (calcX == targetX && calcY == targetY)
            {
                Console.WriteLine("    [PASS] Coordinate calculation correct");
            }
            else
            {
                Console.WriteLine("    [FAIL] Coordinate mismatch");
            }
        }
        
        private static void CheckBorderPoint(int x, int y, bool[,] visibilityMap, string description)
        {
            if (x >= 0 && x < visibilityMap.GetLength(0) && y >= 0 && y < visibilityMap.GetLength(1))
            {
                bool leftShouldBeInvisible = x > 0 && !visibilityMap[x - 1, y];
                bool rightShouldBeVisible = x < visibilityMap.GetLength(0) - 1 && visibilityMap[x, y];
                
                if (leftShouldBeInvisible || rightShouldBeVisible)
                {
                    Console.WriteLine($"  Border at ({x},{y}) - {description}: Left invisible={leftShouldBeInvisible}, Current visible={rightShouldBeVisible}");
                }
            }
        }
    }
}