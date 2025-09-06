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
        
        public RangeOfVision(Grid grid)
        {
            this.grid = grid;
            this.visibilityMap = new bool[grid.Width, grid.Height];
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
                            shadowCones.Add(newCone);
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
            
            Vector2 leftBorder, rightBorder;
            
            if (vx < obstacleX && vy < obstacleY)
            {
                rightBorder = new Vector2(obstacleX - vx, obstacleY + 1 - vy);
                leftBorder = new Vector2(obstacleX + 1 - vx, obstacleY - vy);
            }
            else if (vx > obstacleX && vy < obstacleY)
            {
                leftBorder = new Vector2(obstacleX - vx, obstacleY + 1 - vy);
                rightBorder = new Vector2(obstacleX + 1 - vx, obstacleY - vy);
            }
            else if (vx < obstacleX && vy > obstacleY)
            {
                leftBorder = new Vector2(obstacleX - vx, obstacleY - vy);
                rightBorder = new Vector2(obstacleX + 1 - vx, obstacleY + 1 - vy);
            }
            else if (vx > obstacleX && vy > obstacleY)
            {
                rightBorder = new Vector2(obstacleX - vx, obstacleY - vy);
                leftBorder = new Vector2(obstacleX + 1 - vx, obstacleY + 1 - vy);
            }
            else if (vx == obstacleX)
            {
                if (vy < obstacleY)
                {
                    leftBorder = new Vector2(obstacleX + 1 - vx, obstacleY + 1 - vy);
                    rightBorder = new Vector2(obstacleX - vx, obstacleY + 1 - vy);
                }
                else
                {
                    leftBorder = new Vector2(obstacleX - vx, obstacleY - vy);
                    rightBorder = new Vector2(obstacleX + 1 - vx, obstacleY - vy);
                }
            }
            else
            {
                if (vx < obstacleX)
                {
                    leftBorder = new Vector2(obstacleX + 1 - vx, obstacleY - vy);
                    rightBorder = new Vector2(obstacleX + 1 - vx, obstacleY + 1 - vy);
                }
                else
                {
                    rightBorder = new Vector2(obstacleX - vx, obstacleY - vy);
                    leftBorder = new Vector2(obstacleX - vx, obstacleY + 1 - vy);
                }
            }
            
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