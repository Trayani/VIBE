using Microsoft.Xna.Framework;

namespace GridDisplay
{
    public class VisibilityCone
    {
        // Store points relative to grid origin (without camera offset)
        public Vector2 FocusPointRelative { get; set; }
        public Vector2 LeftBorderPointRelative { get; set; }
        public Vector2 RightBorderPointRelative { get; set; }
        public bool IsActive { get; set; }

        public VisibilityCone()
        {
            IsActive = false;
            FocusPointRelative = Vector2.Zero;
            LeftBorderPointRelative = Vector2.Zero;
            RightBorderPointRelative = Vector2.Zero;
        }

        public VisibilityCone(Vector2 focus, Vector2 leftBorder, Vector2 rightBorder, Vector2 cameraOffset)
        {
            // Convert world coordinates to grid-relative coordinates
            FocusPointRelative = focus - cameraOffset;
            LeftBorderPointRelative = leftBorder - cameraOffset;
            RightBorderPointRelative = rightBorder - cameraOffset;
            IsActive = true;
        }

        public Vector2 GetPoint(int index, Vector2 cameraOffset)
        {
            // Convert grid-relative coordinates to world coordinates
            switch (index)
            {
                case 0: return FocusPointRelative + cameraOffset;
                case 1: return LeftBorderPointRelative + cameraOffset;
                case 2: return RightBorderPointRelative + cameraOffset;
                default: return Vector2.Zero;
            }
        }

        public void SetPoint(int index, Vector2 worldPosition, Vector2 cameraOffset)
        {
            // Convert world coordinates to grid-relative coordinates
            Vector2 relativePosition = worldPosition - cameraOffset;
            switch (index)
            {
                case 0: FocusPointRelative = relativePosition; break;
                case 1: LeftBorderPointRelative = relativePosition; break;
                case 2: RightBorderPointRelative = relativePosition; break;
            }
        }

        public Vector2 SnapToGrid(Vector2 worldPosition, Vector2 cameraOffset, int cellSizeX, int cellSizeY)
        {
            // Convert world position to grid space
            float gridX = (worldPosition.X - cameraOffset.X) / cellSizeX;
            float gridY = (worldPosition.Y - cameraOffset.Y) / cellSizeY;

            // Get the cell coordinates
            int cellX = (int)gridX;
            int cellY = (int)gridY;

            // Determine if we're closer to corner or center
            float fracX = gridX - cellX;
            float fracY = gridY - cellY;

            Vector2 snappedPosition;

            // Snap to corner (0,0) or center (0.5, 0.5) of cell
            if (fracX < 0.25f && fracY < 0.25f)
            {
                // Top-left corner
                snappedPosition = new Vector2(cellX * cellSizeX, cellY * cellSizeY);
            }
            else if (fracX > 0.75f && fracY < 0.25f)
            {
                // Top-right corner
                snappedPosition = new Vector2((cellX + 1) * cellSizeX, cellY * cellSizeY);
            }
            else if (fracX < 0.25f && fracY > 0.75f)
            {
                // Bottom-left corner
                snappedPosition = new Vector2(cellX * cellSizeX, (cellY + 1) * cellSizeY);
            }
            else if (fracX > 0.75f && fracY > 0.75f)
            {
                // Bottom-right corner
                snappedPosition = new Vector2((cellX + 1) * cellSizeX, (cellY + 1) * cellSizeY);
            }
            else
            {
                // Center of cell
                snappedPosition = new Vector2(cellX * cellSizeX + cellSizeX / 2, cellY * cellSizeY + cellSizeY / 2);
            }

            return snappedPosition + cameraOffset;
        }

        public int GetClosestPoint(Vector2 worldPosition, Vector2 cameraOffset, float maxDistance = 15f)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < 3; i++)
            {
                Vector2 point = GetPoint(i, cameraOffset);
                float distance = Vector2.Distance(worldPosition, point);
                
                if (distance < minDistance && distance <= maxDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}