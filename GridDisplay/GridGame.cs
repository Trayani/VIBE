using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridDisplay
{
    public class GridGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Grid grid;
        private Texture2D pixelTexture;
        private Vector2 cameraOffset;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;
        private int selectedX = -1;
        private int selectedY = -1;
        private int startCellX = 0;
        private int startCellY = 0;

        public GridGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            graphics.PreferredBackBufferWidth = 1400;
            graphics.PreferredBackBufferHeight = 800;
        }

        protected override void Initialize()
        {
            grid = new Grid(20, 15, 40);
            cameraOffset = new Vector2(50, 50);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            
        }
        

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            
            if (keyboardState.IsKeyDown(Keys.Left))
                cameraOffset.X += 5;
            if (keyboardState.IsKeyDown(Keys.Right))
                cameraOffset.X -= 5;
            if (keyboardState.IsKeyDown(Keys.Up))
                cameraOffset.Y += 5;
            if (keyboardState.IsKeyDown(Keys.Down))
                cameraOffset.Y -= 5;
            
            int gridX = (int)((mouseState.X - cameraOffset.X) / grid.CellSize);
            int gridY = (int)((mouseState.Y - cameraOffset.Y) / grid.CellSize);
            
            if (gridX >= 0 && gridX < grid.Width && gridY >= 0 && gridY < grid.Height)
            {
                selectedX = gridX;
                selectedY = gridY;
                
                if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    GridCell cell = grid.GetCell(gridX, gridY);
                    if (cell != null)
                    {
                        cell.Blocked = !cell.Blocked;
                    }
                }
                
                if (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                {
                    GridCell cell = grid.GetCell(gridX, gridY);
                    if (cell != null)
                    {
                        cell.Height = (cell.Height + 1) % 10;
                    }
                }
                
                if (mouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released)
                {
                    GridCell cell = grid.GetCell(gridX, gridY);
                    if (cell != null)
                    {
                        cell.Alignment = (cell.Alignment + 1) % 11 - 5;
                    }
                }
            }
            else
            {
                selectedX = -1;
                selectedY = -1;
            }
            
            if (keyboardState.IsKeyDown(Keys.R) && !previousKeyboardState.IsKeyDown(Keys.R))
            {
                grid = new Grid(20, 15, 40);
            }
            
            // Set new start cell with S key
            if (keyboardState.IsKeyDown(Keys.S) && !previousKeyboardState.IsKeyDown(Keys.S))
            {
                if (selectedX >= 0 && selectedY >= 0)
                {
                    startCellX = selectedX;
                    startCellY = selectedY;
                }
            }
            
            previousMouseState = mouseState;
            previousKeyboardState = keyboardState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            
            grid.Draw(spriteBatch, pixelTexture, cameraOffset);
            
            // Draw start cell indicator
            Vector2 startPos = new Vector2(startCellX * grid.CellSize + cameraOffset.X - 2,
                                          startCellY * grid.CellSize + cameraOffset.Y - 2);
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)startPos.X, (int)startPos.Y, grid.CellSize + 3, grid.CellSize + 3),
                Color.Lime * 0.6f);
            
            // Draw line from start cell to mouse cell
            if (selectedX >= 0 && selectedY >= 0)
            {
                Vector2 startCenter = new Vector2(startCellX * grid.CellSize + grid.CellSize / 2 + cameraOffset.X,
                                                 startCellY * grid.CellSize + grid.CellSize / 2 + cameraOffset.Y);
                Vector2 mouseCenter = new Vector2(selectedX * grid.CellSize + grid.CellSize / 2 + cameraOffset.X,
                                                 selectedY * grid.CellSize + grid.CellSize / 2 + cameraOffset.Y);
                
                DrawLine(spriteBatch, pixelTexture, startCenter, mouseCenter, Color.Black, 4);
                
                // Draw selection highlight
                Vector2 position = new Vector2(selectedX * grid.CellSize + cameraOffset.X - 2, 
                                              selectedY * grid.CellSize + cameraOffset.Y - 2);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle((int)position.X, (int)position.Y, grid.CellSize + 3, grid.CellSize + 3), 
                    Color.Red * 0.5f);
            }
            
            
            // Draw cell info window
            DrawCellInfoWindow(spriteBatch, pixelTexture);
            
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            float angle = (float)System.Math.Atan2(direction.Y, direction.X);
            
            spriteBatch.Draw(texture,
                new Rectangle((int)start.X, (int)start.Y - thickness / 2, (int)distance, thickness),
                null,
                color,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0);
        }
        
        private void DrawCellInfoWindow(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            int windowX = 870;
            int windowY = 50;
            int windowWidth = 200;
            int windowHeight = 120;
            
            // Window background
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(windowX, windowY, windowWidth, windowHeight), 
                Color.Black * 0.8f);
            
            // Window border
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(windowX - 2, windowY - 2, windowWidth + 4, 2), 
                Color.White);
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(windowX - 2, windowY + windowHeight, windowWidth + 4, 2), 
                Color.White);
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(windowX - 2, windowY, 2, windowHeight), 
                Color.White);
            spriteBatch.Draw(pixelTexture, 
                new Rectangle(windowX + windowWidth, windowY, 2, windowHeight), 
                Color.White);
            
            // Title
            DrawPixelText(spriteBatch, pixelTexture, "CELL INFO", windowX + 10, windowY + 10, Color.Cyan);
            
            if (selectedX >= 0 && selectedY >= 0)
            {
                GridCell cell = grid.GetCell(selectedX, selectedY);
                if (cell != null)
                {
                    DrawPixelText(spriteBatch, pixelTexture, $"Position: [{selectedX},{selectedY}]", windowX + 10, windowY + 30, Color.White);
                    DrawPixelText(spriteBatch, pixelTexture, $"Blocked: {(cell.Blocked ? "YES" : "NO")}", windowX + 10, windowY + 50, cell.Blocked ? Color.Red : Color.Green);
                    DrawPixelText(spriteBatch, pixelTexture, $"Height: {cell.Height}", windowX + 10, windowY + 70, Color.White);
                    DrawPixelText(spriteBatch, pixelTexture, $"Alignment: {cell.Alignment}", windowX + 10, windowY + 90, Color.Yellow);
                }
            }
            else
            {
                DrawPixelText(spriteBatch, pixelTexture, "Hover over a cell", windowX + 10, windowY + 50, Color.Gray);
            }
        }
        
        private void DrawPixelText(SpriteBatch spriteBatch, Texture2D pixelTexture, string text, int x, int y, Color color)
        {
            // Simple bitmap font using pixel patterns for each character
            int charWidth = 6;
            int charHeight = 8;
            int originalX = x;
            
            foreach (char c in text)
            {
                if (c == ' ')
                {
                    x += charWidth;
                    continue;
                }
                else if (c == '\n')
                {
                    x = originalX;
                    y += charHeight + 2;
                    continue;
                }
                
                DrawCharacter(spriteBatch, pixelTexture, c, x, y, color);
                x += charWidth;
            }
        }
        
        private void DrawCharacter(SpriteBatch spriteBatch, Texture2D pixelTexture, char c, int x, int y, Color color)
        {
            if (char.IsDigit(c))
            {
                DrawDigitChar(spriteBatch, pixelTexture, c - '0', x, y, color);
                return;
            }
            
            // 5x7 bitmap patterns for characters
            bool[,] pattern = GetCharacterPattern(char.ToUpper(c));
            
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (pattern[row, col])
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle(x + col, y + row, 1, 1), color);
                    }
                }
            }
        }
        
        private void DrawDigitChar(SpriteBatch spriteBatch, Texture2D pixelTexture, int digit, int x, int y, Color color)
        {
            int w = 4;
            int h = 7;
            
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
            
            // Top horizontal
            if (segments[digit, 0])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y, w, 1), color);
            // Top-left vertical
            if (segments[digit, 1])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 1, h/2), color);
            // Top-right vertical
            if (segments[digit, 2])
                spriteBatch.Draw(pixelTexture, new Rectangle(x + w - 1, y, 1, h/2), color);
            // Middle horizontal
            if (segments[digit, 3])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h/2 - 1, w, 1), color);
            // Bottom-left vertical
            if (segments[digit, 4])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h/2, 1, h/2), color);
            // Bottom-right vertical
            if (segments[digit, 5])
                spriteBatch.Draw(pixelTexture, new Rectangle(x + w - 1, y + h/2, 1, h/2), color);
            // Bottom horizontal
            if (segments[digit, 6])
                spriteBatch.Draw(pixelTexture, new Rectangle(x, y + h - 1, w, 1), color);
        }
        
        private bool[,] GetCharacterPattern(char c)
        {
            // 5x7 bitmap font patterns
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
                case 'V':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,false,true,false},
                        {false,true,false,true,false},
                        {false,false,true,false,false}
                    };
                case 'Y':
                    return new bool[,] {
                        {true,false,false,false,true},
                        {true,false,false,false,true},
                        {false,true,false,true,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false},
                        {false,false,true,false,false}
                    };
                case ':':
                    return new bool[,] {
                        {false,false,false,false,false},
                        {false,false,true,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,true,false,false},
                        {false,false,false,false,false}
                    };
                case '[':
                    return new bool[,] {
                        {false,true,true,false,false},
                        {false,true,false,false,false},
                        {false,true,false,false,false},
                        {false,true,false,false,false},
                        {false,true,false,false,false},
                        {false,true,false,false,false},
                        {false,true,true,false,false}
                    };
                case ']':
                    return new bool[,] {
                        {false,false,true,true,false},
                        {false,false,false,true,false},
                        {false,false,false,true,false},
                        {false,false,false,true,false},
                        {false,false,false,true,false},
                        {false,false,false,true,false},
                        {false,false,true,true,false}
                    };
                case ',':
                    return new bool[,] {
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,true,false,false},
                        {false,true,false,false,false}
                    };
                case '-':
                    return new bool[,] {
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {true,true,true,true,true},
                        {false,false,false,false,false},
                        {false,false,false,false,false},
                        {false,false,false,false,false}
                    };
                default:
                    // Default pattern for unknown characters
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
    }
}