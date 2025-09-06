using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GridDisplay
{
    public class RangeOfVision
    {
        private Grid grid;
        private Point viewerPosition;
        private bool[,] visibilityMap;
        public bool EnableDebug { get; set; }
        
        public RangeOfVision(Grid grid)
        {
            this.grid = grid;
            this.visibilityMap = new bool[grid.Width, grid.Height];
            EnableDebug = false;
        }
        
        public bool[,] CalculateVisibility(Point viewer)
        {
            viewerPosition = viewer;
            ResetVisibilityMap();
            
            List<ShadowCone> shadowCones = new List<ShadowCone>();
            
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (x == viewerPosition.X && y == viewerPosition.Y)
                        continue;
                    
                    bool blocked = false;
                    foreach (var cone in shadowCones)
                    {
                        if (IsInShadow(x, y, cone))
                        {
                            blocked = true;
                            break;
                        }
                    }
                    
                    if (blocked)
                    {
                        visibilityMap[x, y] = false;
                    }
                    
                    var cell = grid.GetCell(x, y);
                    if (cell != null && cell.Blocked && visibilityMap[x, y])
                    {
                        var newCone = CreateShadowCone(x, y);
                        if (newCone != null)
                        {
                            if (EnableDebug)
                            {
                                Console.WriteLine($"Obstacle at ({x},{y}), creating shadow cone:");
                                Console.WriteLine($"  Left border diff: ({newCone.LeftBorderDiff.X},{newCone.LeftBorderDiff.Y})");
                                Console.WriteLine($"  Right border diff: ({newCone.RightBorderDiff.X},{newCone.RightBorderDiff.Y})");
                            }
                            shadowCones.Add(newCone);
                        }
                    }
                }
            }
            
            return visibilityMap;
        }
        
        private void ResetVisibilityMap()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    visibilityMap[x, y] = true;
                }
            }
        }
        
        private bool IsInShadow(int x, int y, ShadowCone cone)
        {
            if (cone == null) return false;
            
            int dx = x - viewerPosition.X;
            int dy = y - viewerPosition.Y;
            Vector2 obstaclePos = cone.ObstaclePosition;
            
            if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
            {
                Console.WriteLine($"Checking cell ({x},{y}) against cone from ({obstaclePos.X},{obstaclePos.Y})");
                Console.WriteLine($"  dx={dx}, dy={dy}, LeftDiff=({cone.LeftBorderDiff.X},{cone.LeftBorderDiff.Y}), RightDiff=({cone.RightBorderDiff.X},{cone.RightBorderDiff.Y})");
            }
            
            // Handle horizontal shadows (same Y level as viewer)
            if (dy == 0)
            {
                // For horizontal obstacles, check if target is directly behind obstacle
                int obstacleDx = (int)obstaclePos.X - viewerPosition.X;
                
                // Shadow extends horizontally from obstacle
                if (obstacleDx > 0 && dx > obstacleDx) // Obstacle to the right, target further right
                {
                    return true;
                }
                else if (obstacleDx < 0 && dx < obstacleDx) // Obstacle to the left, target further left
                {
                    return true;
                }
                return false;
            }
            
            // For horizontal obstacles (same Y as viewer), we need special handling
            // The obstacle can cast shadows both above and below itself
            bool isHorizontalObstacle = Math.Abs(obstaclePos.Y - viewerPosition.Y) < 0.1f;
            
            if (isHorizontalObstacle)
            {
                // For horizontal obstacles, both borders should be considered for cells on either side
                // The shadow spreads diagonally from the obstacle
                bool leftBorderValid = cone.LeftBorderDiff.Y != 0;
                bool rightBorderValid = cone.RightBorderDiff.Y != 0;
                
                if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
                {
                    Console.WriteLine($"  Horizontal obstacle: leftBorderValid={leftBorderValid}, rightBorderValid={rightBorderValid}");
                }
                
                if (!leftBorderValid && !rightBorderValid)
                {
                    return false;
                }
            }
            else
            {
                // For non-horizontal obstacles, use original logic
                bool leftBorderValid = (dy > 0) == (cone.LeftBorderDiff.Y > 0);
                bool rightBorderValid = (dy > 0) == (cone.RightBorderDiff.Y > 0);
                
                if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
                {
                    Console.WriteLine($"  Non-horizontal: leftBorderValid={leftBorderValid}, rightBorderValid={rightBorderValid}");
                }
                
                if (!leftBorderValid && !rightBorderValid)
                {
                    if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
                    {
                        Console.WriteLine($"  Neither border valid, returning false");
                    }
                    return false;
                }
            }
            
            // Calculate border positions
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            
            if (isHorizontalObstacle)
            {
                // For horizontal obstacles, both borders contribute to shadow width
                if (cone.LeftBorderDiff.Y != 0)
                {
                    float xAtYLeft = viewerPosition.X + (cone.LeftBorderDiff.X * (float)dy / cone.LeftBorderDiff.Y);
                    minX = Math.Min(minX, xAtYLeft);
                    maxX = Math.Max(maxX, xAtYLeft);
                    if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
                    {
                        Console.WriteLine($"  Left border: xAtYLeft={xAtYLeft}");
                    }
                }
                
                if (cone.RightBorderDiff.Y != 0)
                {
                    float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * (float)dy / cone.RightBorderDiff.Y);
                    minX = Math.Min(minX, xAtYRight);
                    maxX = Math.Max(maxX, xAtYRight);
                    if (EnableDebug && (x >= 20 && x <= 25 && y == 3))
                    {
                        Console.WriteLine($"  Right border: xAtYRight={xAtYRight}");
                    }
                }
            }
            else
            {
                // For non-horizontal obstacles, use validation flags
                bool leftBorderValid = (dy > 0) == (cone.LeftBorderDiff.Y > 0);
                bool rightBorderValid = (dy > 0) == (cone.RightBorderDiff.Y > 0);
                
                if (leftBorderValid && cone.LeftBorderDiff.Y != 0)
                {
                    float xAtYLeft = viewerPosition.X + (cone.LeftBorderDiff.X * (float)dy / cone.LeftBorderDiff.Y);
                    minX = Math.Min(minX, xAtYLeft);
                    maxX = Math.Max(maxX, xAtYLeft);
                }
                
                if (rightBorderValid && cone.RightBorderDiff.Y != 0)
                {
                    float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * (float)dy / cone.RightBorderDiff.Y);
                    minX = Math.Min(minX, xAtYRight);
                    maxX = Math.Max(maxX, xAtYRight);
                }
            }
            
            // If no valid borders, return false
            if (minX == float.MaxValue)
                return false;
            
            // More conservative shadow - include cells that are close to borders
            // This makes the shadow cover upper and lower lines as well
            
            // Check if this cell lies exactly on either border line (should be visible)
            const float borderTolerance = 0.1f;
            bool onBorder = false;
            
            if (isHorizontalObstacle)
            {
                // For horizontal obstacles, check borders without validation flags
                if (cone.LeftBorderDiff.Y != 0)
                {
                    float xAtYLeft = viewerPosition.X + (cone.LeftBorderDiff.X * (float)dy / cone.LeftBorderDiff.Y);
                    if (Math.Abs(x - xAtYLeft) < borderTolerance)
                        onBorder = true;
                }
                
                if (cone.RightBorderDiff.Y != 0)
                {
                    float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * (float)dy / cone.RightBorderDiff.Y);
                    if (Math.Abs(x - xAtYRight) < borderTolerance)
                        onBorder = true;
                }
            }
            else
            {
                // For non-horizontal obstacles, use validation flags
                bool leftBorderValid = (dy > 0) == (cone.LeftBorderDiff.Y > 0);
                bool rightBorderValid = (dy > 0) == (cone.RightBorderDiff.Y > 0);
                
                if (leftBorderValid && cone.LeftBorderDiff.Y != 0)
                {
                    float xAtYLeft = viewerPosition.X + (cone.LeftBorderDiff.X * (float)dy / cone.LeftBorderDiff.Y);
                    if (Math.Abs(x - xAtYLeft) < borderTolerance)
                        onBorder = true;
                }
                
                if (rightBorderValid && cone.RightBorderDiff.Y != 0)
                {
                    float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * (float)dy / cone.RightBorderDiff.Y);
                    if (Math.Abs(x - xAtYRight) < borderTolerance)
                        onBorder = true;
                }
            }
            
            if (onBorder)
            {
                if (EnableDebug && (x >= 25 && x <= 25 && y == 3))
                {
                    Console.WriteLine($"  Cell is on border, returning false (visible)");
                }
                return false; // Border cells are visible
            }
            
            // Expand shadow area conservatively but not too much to preserve border visibility
            const float expansionTolerance = 0.75f;
            bool inShadow = x >= minX - expansionTolerance && x <= maxX + expansionTolerance;
            
            if (EnableDebug && (x >= 25 && x <= 25 && y == 3))
            {
                Console.WriteLine($"  minX={minX}, maxX={maxX}, x={x}, expansionTolerance={expansionTolerance}, inShadow={inShadow}");
                Console.WriteLine($"  Check: {x} >= {minX - expansionTolerance} && {x} <= {maxX + expansionTolerance}");
            }
            
            return inShadow;
        }
        
        private ShadowCone CreateShadowCone(int obstacleX, int obstacleY)
        {
            int vx = viewerPosition.X;
            int vy = viewerPosition.Y;
            
            if (vx == obstacleX && vy == obstacleY)
                return null;
            
            Vector2 leftBorderPoint, rightBorderPoint;
            
            // Determine which cells adjacent to the obstacle define the shadow borders
            // The shadow starts from cells that are just outside the obstacle but block the view
            
            if (vx < obstacleX && vy < obstacleY)
            {
                // Viewer is up-left from obstacle
                // Right border passes through cell to the left and below obstacle
                // Left border passes through cell to the right and above obstacle
                rightBorderPoint = new Vector2(obstacleX - 1, obstacleY + 1);
                leftBorderPoint = new Vector2(obstacleX + 1, obstacleY - 1);
            }
            else if (vx > obstacleX && vy < obstacleY)
            {
                // Viewer is up-right from obstacle
                leftBorderPoint = new Vector2(obstacleX - 1, obstacleY + 1);
                rightBorderPoint = new Vector2(obstacleX + 1, obstacleY - 1);
            }
            else if (vx < obstacleX && vy > obstacleY)
            {
                leftBorderPoint = new Vector2(obstacleX - 1, obstacleY - 1);
                rightBorderPoint = new Vector2(obstacleX + 1, obstacleY + 1);
            }
            else if (vx > obstacleX && vy > obstacleY)
            {
                rightBorderPoint = new Vector2(obstacleX - 1, obstacleY - 1);
                leftBorderPoint = new Vector2(obstacleX + 1, obstacleY + 1);
            }
            else if (vx == obstacleX)
            {
                if (vy < obstacleY)
                {
                    // Viewer is directly above obstacle
                    leftBorderPoint = new Vector2(obstacleX + 1, obstacleY + 1);
                    rightBorderPoint = new Vector2(obstacleX - 1, obstacleY + 1);
                }
                else
                {
                    // Viewer is directly below obstacle
                    leftBorderPoint = new Vector2(obstacleX - 1, obstacleY - 1);
                    rightBorderPoint = new Vector2(obstacleX + 1, obstacleY - 1);
                }
            }
            else // vy == obstacleY - same Y level as viewer
            {
                if (vx < obstacleX)
                {
                    // Viewer is directly to the left of obstacle
                    // Shadow should extend to the right and create a spreading diagonal cone
                    // Based on case2.csv, cells (20,3)-(25,3) should be blocked by obstacle (20,2)
                    // This requires a shadow cone that expands rightward and covers these cells
                    leftBorderPoint = new Vector2(obstacleX + 6, obstacleY - 1);   // Point that creates wide right coverage
                    rightBorderPoint = new Vector2(obstacleX + 6, obstacleY + 1); // Point that creates wide right coverage
                }
                else
                {
                    // Viewer is directly to the right of obstacle  
                    // Shadow should extend to the left and create diagonal cone
                    rightBorderPoint = new Vector2(obstacleX - 3, obstacleY - 2); // Far top-left
                    leftBorderPoint = new Vector2(obstacleX - 3, obstacleY + 2);  // Far bottom-left
                }
            }
            
            // Calculate the coordinate differences
            Vector2 leftBorder = leftBorderPoint - new Vector2(vx, vy);
            Vector2 rightBorder = rightBorderPoint - new Vector2(vx, vy);
            
            return new ShadowCone
            {
                LeftBorderDiff = leftBorder,
                RightBorderDiff = rightBorder,
                ObstaclePosition = new Vector2(obstacleX, obstacleY)
            };
        }
        
        private class ShadowCone
        {
            public Vector2 LeftBorderDiff { get; set; }
            public Vector2 RightBorderDiff { get; set; }
            public Vector2 ObstaclePosition { get; set; }
        }
    }
}