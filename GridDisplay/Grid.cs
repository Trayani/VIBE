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
        public int CellSizeX { get; private set; }
        public int CellSizeY { get; private set; }

        public Grid(int width, int height, int cellSizeX = 28, int cellSizeY = 20)
        {
            Width = width;
            Height = height;
            CellSizeX = cellSizeX;
            CellSizeY = cellSizeY;
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
                        blocked: random.Next(100) < 5,
                        height: random.Next(0, 10),
                        alignment: random.Next(0, 20)
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

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 offset, Func<int, int, bool> isVisibleCallback = null)
        {
            // Draw cells
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridCell cell = cells[x, y];
                    Vector2 position = new Vector2(x * CellSizeX + offset.X, y * CellSizeY + offset.Y);
                    
                    Color cellColor = GetCellColor(cell);
                    bool isVisible = isVisibleCallback?.Invoke(x, y) ?? false;
                    
                    // Apply yellow tint to visible cells
                    if (isVisible)
                    {
                        cellColor = Color.Lerp(cellColor, Color.Yellow, 0.4f);
                    }
                    
                    spriteBatch.Draw(pixelTexture, 
                        new Rectangle((int)position.X, (int)position.Y, CellSizeX, CellSizeY), 
                        cellColor);
                    
                    // Draw orange border for visible cells
                    if (isVisible)
                    {
                        // Draw border as 4 rectangles (top, bottom, left, right)
                        Color orangeBorder = Color.Orange;
                        int borderThickness = 1;
                        
                        // Top border
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle((int)position.X, (int)position.Y, CellSizeX, borderThickness), 
                            orangeBorder);
                        // Bottom border
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle((int)position.X, (int)position.Y + CellSizeY - borderThickness, CellSizeX, borderThickness), 
                            orangeBorder);
                        // Left border
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle((int)position.X, (int)position.Y, borderThickness, CellSizeY), 
                            orangeBorder);
                        // Right border
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle((int)position.X + CellSizeX - borderThickness, (int)position.Y, borderThickness, CellSizeY), 
                            orangeBorder);
                    }
                    
                    
                    DrawThickNumber(spriteBatch, pixelTexture, cell.Height, position + new Vector2(1, 1), Color.Black, 6);
                    
                    DrawAlignmentLetter(spriteBatch, pixelTexture, cell.Alignment, position + new Vector2(1, CellSizeY - 7), Color.Black, 6);
                }
            }
            
            // Draw grid lines
            for (int x = 0; x <= Width; x++)
            {
                int xPos = (int)(x * CellSizeX + offset.X);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle(xPos, (int)offset.Y, 1, Height * CellSizeY), 
                    Color.Black);
            }
            
            for (int y = 0; y <= Height; y++)
            {
                int yPos = (int)(y * CellSizeY + offset.Y);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle((int)offset.X, yPos, Width * CellSizeX, 1), 
                    Color.Black);
            }
        }

        private Color GetCellColor(GridCell cell)
        {
            if (cell.Blocked)
                return Color.Red;
            
            // Height affects cyan scale (0 = white, 9 = full cyan)
            float heightNormalized = cell.Height / 9f;
            byte redValue = (byte)(255 - heightNormalized * 255);
            return new Color(redValue, 255, 255);
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
        
        private void DrawThickNumber(SpriteBatch spriteBatch, Texture2D pixelTexture, int number, Vector2 position, Color color, int size)
        {
            string numStr = Math.Abs(number).ToString();
            int scale = size / 7; // Thickness scale
            int digitWidth = 5 * scale; // 5 pixels wide scaled
            int startX = (int)position.X;
            
            if (number < 0)
            {
                // Draw minus sign
                for (int dy = 0; dy < scale; dy++)
                {
                    for (int dx = 0; dx < digitWidth; dx++)
                    {
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle(startX + dx, (int)position.Y + size/2 + dy, 1, 1), 
                            color);
                    }
                }
                startX += digitWidth + scale;
            }
            
            foreach (char digit in numStr)
            {
                DrawThickDigit(spriteBatch, pixelTexture, digit - '0', new Vector2(startX, position.Y), color, size);
                startX += digitWidth + scale;
            }
        }
        
        private void DrawThickDigit(SpriteBatch spriteBatch, Texture2D pixelTexture, int digit, Vector2 position, Color color, int size)
        {
            int scale = size / 7; // Thickness scale
            int w = 4 * scale;
            int h = size;
            int x = (int)position.X;
            int y = (int)position.Y;
            
            bool[,] segments = new bool[,]
            {
                { true, true, true, false, true, true, true },      // 0
                { false, false, true, false, false, true, false },  // 1
                { true, false, true, true, true, false, true },     // 2
                { true, false, true, true, false, true, true },     // 3
                { false, true, true, true, false, true, false },    // 4
                { true, true, false, true, false, true, true },     // 5
                { true, true, false, true, true, true, true },      // 6
                { true, false, true, false, false, true, false },   // 7
                { true, true, true, true, true, true, true },       // 8
                { true, true, true, true, false, true, true }       // 9
            };
            
            // Top horizontal (thicker)
            if (segments[digit, 0])
            {
                for (int dy = 0; dy < scale; dy++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + dy, w, 1), color);
            }
            // Top-left vertical (thicker)
            if (segments[digit, 1])
            {
                for (int dx = 0; dx < scale; dx++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + dx, y, 1, h/2), color);
            }
            // Top-right vertical (thicker)
            if (segments[digit, 2])
            {
                for (int dx = 0; dx < scale; dx++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + w - scale + dx, y, 1, h/2), color);
            }
            // Middle horizontal (thicker)
            if (segments[digit, 3])
            {
                for (int dy = 0; dy < scale; dy++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h/2 - scale/2 + dy, w, 1), color);
            }
            // Bottom-left vertical (thicker)
            if (segments[digit, 4])
            {
                for (int dx = 0; dx < scale; dx++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + dx, y + h/2, 1, h/2), color);
            }
            // Bottom-right vertical (thicker)
            if (segments[digit, 5])
            {
                for (int dx = 0; dx < scale; dx++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + w - scale + dx, y + h/2, 1, h/2), color);
            }
            // Bottom horizontal (thicker)
            if (segments[digit, 6])
            {
                for (int dy = 0; dy < scale; dy++)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h - scale + dy, w, 1), color);
            }
        }
        
        private void DrawAlignmentLetter(SpriteBatch spriteBatch, Texture2D pixelTexture, int alignment, Vector2 position, Color color, int size)
        {
            // Convert 0-19 to A-T
            char letter = (char)('A' + alignment);
            DrawThickLetter(spriteBatch, pixelTexture, letter, position, color, size);
        }
        
        private void DrawThickLetter(SpriteBatch spriteBatch, Texture2D pixelTexture, char letter, Vector2 position, Color color, int size)
        {
            // Get the letter pattern (reuse from existing character patterns)
            bool[,] pattern = GetLetterPattern(letter);
            int scale = size / 7; // Make letters even thicker by scaling more
            
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (pattern[row, col])
                    {
                        // Draw thicker pixels by drawing multiple rectangles
                        for (int dy = 0; dy < scale; dy++)
                        {
                            for (int dx = 0; dx < scale; dx++)
                            {
                                spriteBatch.Draw(pixelTexture, 
                                    new Rectangle((int)position.X + col * scale + dx, (int)position.Y + row * scale + dy, 1, 1), 
                                    color);
                            }
                        }
                    }
                }
            }
        }
        
        private bool[,] GetLetterPattern(char c)
        {
            switch (c)
            {
                case 'A':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true}
                    };
                case 'B':
                    return new bool[,] {
                        {true,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,false}
                    };
                case 'C':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,true},
                        {false,true,true,true,false}
                    };
                case 'D':
                    return new bool[,] {
                        {true,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,false}
                    };
                case 'E':
                    return new bool[,] {
                        {true,true,true,true,true},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,true,true,true,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,true,true,true,true}
                    };
                case 'F':
                    return new bool[,] {
                        {true,true,true,true,true},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,true,true,true,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false}
                    };
                case 'G':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,false},
                        {true,false,true,true,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,true,true,false}
                    };
                case 'H':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true}
                    };
                case 'I':
                    return new bool[,] {
                        {true,true,true,true,true},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {true,true,true,true,true}
                    };
                case 'J':
                    return new bool[,] {
                        {false,false,false,false,true},
                        {false,false,false,false,true},
                        {false,false,false,false,true},
                        {false,false,false,false,true},
                        {false,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,true,true,false}
                    };
                case 'K':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,false,false,true,false},
                        {true,false,true,false,false},
                        {true,true,false,false,false},
                        {true,false,true,false,false},
                        {true,false,false,true,false},
                        {true,false,false,false,true}
                    };
                case 'L':
                    return new bool[,] {
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,true,true,true,true}
                    };
                case 'M':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,true,false,true,true},
                        {true,false,true,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true}
                    };
                case 'N':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,true,false,false,true},
                        {true,false,true,false,true},
                        {true,false,false,true,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true}
                    };
                case 'O':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,true,true,false}
                    };
                case 'P':
                    return new bool[,] {
                        {true,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false},
                        {true,false,false,false,false}
                    };
                case 'Q':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,true,false,true},
                        {true,false,false,true,false},
                        {false,true,true,false,true}
                    };
                case 'R':
                    return new bool[,] {
                        {true,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,false},
                        {true,false,true,false,false},
                        {true,false,false,true,false},
                        {true,false,false,false,true}
                    };
                case 'S':
                    return new bool[,] {
                        {false,true,true,true,false},
                        {true,false,false,false,true},
                        {true,false,false,false,false},
                        {false,true,true,true,false},
                        {false,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,true,true,false}
                    };
                case 'T':
                    return new bool[,] {
                        {true,true,true,true,true},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false}
                    };
                default:
                    // Default pattern for unknown letters
                    return new bool[,] {
                        {true,true,true,true,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,true,true,true,true}
                    };
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