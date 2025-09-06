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
            
            if (dy == 0) return false;
            
            if ((dy > 0) != (cone.LeftBorderDiff.Y > 0))
                return false;
            
            float xAtY = viewerPosition.X + (cone.LeftBorderDiff.X * dy / cone.LeftBorderDiff.Y);
            float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * dy / cone.RightBorderDiff.Y);
            
            float minX = Math.Min(xAtY, xAtYRight);
            float maxX = Math.Max(xAtY, xAtYRight);
            
            return x >= minX && x <= maxX;
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
            else // vy == obstacleY
            {
                if (vx < obstacleX)
                {
                    // Viewer is directly to the left of obstacle
                    leftBorderPoint = new Vector2(obstacleX + 1, obstacleY - 1);
                    rightBorderPoint = new Vector2(obstacleX + 1, obstacleY + 1);
                }
                else
                {
                    // Viewer is directly to the right of obstacle
                    rightBorderPoint = new Vector2(obstacleX - 1, obstacleY - 1);
                    leftBorderPoint = new Vector2(obstacleX - 1, obstacleY + 1);
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