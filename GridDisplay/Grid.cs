using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GridDisplay
{
    public class Grid
    {
        private GridCell[,] cells;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CellSize { get; private set; }

        public Grid(int width, int height, int cellSize = 32)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            cells = new GridCell[width, height];
            
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            Random random = new Random();
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    cells[x, y] = new GridCell(
                        blocked: random.Next(100) < 20,
                        height: random.Next(0, 10),
                        alignment: random.Next(-5, 6)
                    );
                }
            }
        }

        public GridCell GetCell(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return cells[x, y];
            return null;
        }

        public void SetCell(int x, int y, GridCell cell)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                cells[x, y] = cell;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 offset)
        {
            // Draw cells
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridCell cell = cells[x, y];
                    Vector2 position = new Vector2(x * CellSize + offset.X, y * CellSize + offset.Y);
                    
                    Color cellColor = GetCellColor(cell);
                    
                    spriteBatch.Draw(pixelTexture, 
                        new Rectangle((int)position.X, (int)position.Y, CellSize, CellSize), 
                        cellColor);
                    
                    if (cell.Blocked)
                    {
                        spriteBatch.Draw(pixelTexture,
                            new Rectangle((int)position.X + 2, (int)position.Y + 2, CellSize - 5, CellSize - 5),
                            Color.Black);
                    }
                    
                    DrawNumber(spriteBatch, pixelTexture, cell.Height, position + new Vector2(2, 2), Color.White, 8);
                    
                    DrawNumber(spriteBatch, pixelTexture, cell.Alignment, position + new Vector2(2, CellSize - 10), Color.Yellow, 8);
                }
            }
            
            // Draw grid lines
            for (int x = 0; x <= Width; x++)
            {
                int xPos = (int)(x * CellSize + offset.X);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle(xPos, (int)offset.Y, 1, Height * CellSize), 
                    Color.Black);
            }
            
            for (int y = 0; y <= Height; y++)
            {
                int yPos = (int)(y * CellSize + offset.Y);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle((int)offset.X, yPos, Width * CellSize, 1), 
                    Color.Black);
            }
        }

        private Color GetCellColor(GridCell cell)
        {
            if (cell.Blocked)
                return Color.DarkGray;
            
            float heightNormalized = cell.Height / 10f;
            byte green = (byte)(100 + heightNormalized * 155);
            return new Color(50, green, 50);
        }
        
        private void DrawNumber(SpriteBatch spriteBatch, Texture2D pixelTexture, int number, Vector2 position, Color color, int size)
        {
            string numStr = Math.Abs(number).ToString();
            int digitWidth = size / 2;
            int startX = (int)position.X;
            
            if (number < 0)
            {
                spriteBatch.Draw(pixelTexture, new Rectangle(startX, (int)position.Y + size / 2 - 1, digitWidth, 2), color);
                startX += digitWidth + 1;
            }
            
            foreach (char digit in numStr)
            {
                DrawDigit(spriteBatch, pixelTexture, digit - '0', new Vector2(startX, position.Y), color, size);
                startX += digitWidth + 2;
            }
        }
        
        private void DrawDigit(SpriteBatch spriteBatch, Texture2D pixelTexture, int digit, Vector2 position, Color color, int size)
        {
            int w = size / 2;
            int h = size;
            int x = (int)position.X;
            int y = (int)position.Y;
            
            bool[,] segments = new bool[,]
            {
                { true, true, true, false, true, true, true },
                { false, false, true, false, false, true, false },
                { true, false, true, true, true, false, true },
                { true, false, true, true, false, true, true },
                { false, true, true, true, false, true, false },
                { true, true, false, true, false, true, true },
                { true, true, false, true, true, true, true },
                { true, false, true, false, false, true, false },
                { true, true, true, true, true, true, true },
                { true, true, true, true, false, true, true }
            };
            
            if (segments[digit, 0])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y, w, 1), color);
            if (segments[digit, 1])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 1, h/2), color);
            if (segments[digit, 2])
                spriteBatch.Draw(pixelTexture, new Rectangle(x + w - 1, y, 1, h/2), color);
            if (segments[digit, 3])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h/2 - 1, w, 1), color);
            if (segments[digit, 4])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h/2, 1, h/2), color);
            if (segments[digit, 5])
                spriteBatch.Draw(pixelTexture, new Rectangle(x + w - 1, y + h/2, 1, h/2), color);
            if (segments[digit, 6])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h - 1, w, 1), color);
        }
    }
}