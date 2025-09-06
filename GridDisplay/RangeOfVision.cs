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
            
            // Special handling for obstacles on same Y level as viewer
            Vector2 obstaclePos = cone.ObstaclePosition;
            if ((int)obstaclePos.Y == viewerPosition.Y)
            {
                int obstacleDx = (int)obstaclePos.X - viewerPosition.X;
                
                // For same row as obstacle (dy == 0)
                if (dy == 0)
                {
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
                
                // For rows above/below obstacle when obstacle is on same Y as viewer
                // The shadow point is at (obstacleX-1, obstacleY+/-1) based on which row we're checking
                if (obstacleDx > 0) // Obstacle is to the right of viewer (e.g., obstacle at 20,2 viewer at 3,2)
                {
                    // Shadow point for row above (y=3) is at (obstacleX-1, obstacleY+1) = (19,3)
                    // Shadow point for row below (y=1) is at (obstacleX-1, obstacleY-1) = (19,1)
                    int shadowPointX = (int)obstaclePos.X - 1;
                    
                    // Calculate the shadow expansion vector from viewer to shadow point
                    // For viewer at (3,2), obstacle at (20,2), shadow point at (19,3) for row above
                    // Shadow vector = (19-3, 3-2) = (16, 1) for row above
                    // Shadow vector = (19-3, 1-2) = (16, -1) for row below
                    int shadowDx = shadowPointX - viewerPosition.X; // e.g., 19 - 3 = 16
                    // shadowDy is simply dy (1 for row above, -1 for row below)
                    
                    // The shadow starts right after the shadow point (at X=20 for row 3)
                    // Everything from X=20 onwards is in shadow for row 3
                    if (x > shadowPointX)
                    {
                        return true;
                    }
                }
                else if (obstacleDx < 0) // Obstacle is to the left of viewer
                {
                    // Shadow point for row above is at (obstacleX+1, obstacleY+1)
                    // Shadow point for row below is at (obstacleX+1, obstacleY-1)
                    int shadowPointX = (int)obstaclePos.X + 1;
                    
                    // Everything before the shadow point is in shadow
                    if (x < shadowPointX)
                    {
                        return true;
                    }
                }
                return false;
            }
            
            // Handle normal diagonal shadows (obstacle not on same Y level)
            if (dy == 0)
            {
                // Target is on same Y level as viewer but obstacle is not
                // This shouldn't cast shadow on the viewer's row
                return false;
            }
            
            if ((dy > 0) != (cone.LeftBorderDiff.Y > 0))
                return false;
            
            // Make shadows more conservative - include more cells in shadow
            float xAtYLeft = viewerPosition.X + (cone.LeftBorderDiff.X * (float)dy / cone.LeftBorderDiff.Y);
            float xAtYRight = viewerPosition.X + (cone.RightBorderDiff.X * (float)dy / cone.RightBorderDiff.Y);
            
            float minX = Math.Min(xAtYLeft, xAtYRight);
            float maxX = Math.Max(xAtYLeft, xAtYRight);
            
            // More conservative shadow - include cells that are close to borders
            // This makes the shadow cover upper and lower lines as well
            
            // Check if this cell lies exactly on either border line (should be visible)
            const float borderTolerance = 0.1f;
            if (Math.Abs(x - xAtYLeft) < borderTolerance || Math.Abs(x - xAtYRight) < borderTolerance)
            {
                return false; // Border cells are visible
            }
            
            // Expand shadow area conservatively but not too much to preserve border visibility
            const float expansionTolerance = 0.75f;
            return x >= minX - expansionTolerance && x <= maxX + expansionTolerance;
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