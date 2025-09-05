using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        private int targetCellX = -1;
        private int targetCellY = -1;
        private VisibilityCone visibilityCone;
        private bool isDragging = false;
        private int draggedPointIndex = -1;
        private bool isCameraDragging = false;
        private Vector2 lastCameraDragPosition;
        private float zoomLevel = 1.0f;
        private const float minZoom = 0.25f;
        private const float maxZoom = 4.0f;
        private const float zoomStep = 0.1f;

        public GridGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
        }

        protected override void Initialize()
        {
            grid = new Grid(45, 35, 28, 20);
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
            
            // Handle scroll wheel input
            int scrollDelta = mouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
                {
                    // CTRL+Scroll = Zoom
                    float zoomChange = (scrollDelta > 0) ? zoomStep : -zoomStep;
                    float newZoom = MathHelper.Clamp(zoomLevel + zoomChange, minZoom, maxZoom);
                    
                    // Get mouse cell center for zoom target
                    Vector2 zoomMouseWorldPos = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                    int mouseGridX = (int)((zoomMouseWorldPos.X - cameraOffset.X) / grid.CellSizeX);
                    int mouseGridY = (int)((zoomMouseWorldPos.Y - cameraOffset.Y) / grid.CellSizeY);
                    
                    // Clamp to valid grid bounds
                    mouseGridX = MathHelper.Clamp(mouseGridX, 0, grid.Width - 1);
                    mouseGridY = MathHelper.Clamp(mouseGridY, 0, grid.Height - 1);
                    
                    // Use cell center as zoom target
                    Vector2 cellCenter = new Vector2(
                        mouseGridX * grid.CellSizeX + grid.CellSizeX / 2 + cameraOffset.X,
                        mouseGridY * grid.CellSizeY + grid.CellSizeY / 2 + cameraOffset.Y);
                    
                    Vector2 screenCellCenter = WorldToScreen(cellCenter);
                    Vector2 worldCellCenter = ScreenToWorld(screenCellCenter);
                    
                    zoomLevel = newZoom;
                    
                    // Adjust camera to keep cell center consistent
                    Vector2 newWorldCellCenter = ScreenToWorld(screenCellCenter);
                    Vector2 difference = worldCellCenter - newWorldCellCenter;
                    cameraOffset += difference;
                }
                else if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    // SHIFT+Scroll = Horizontal movement
                    float scrollSpeed = 30.0f / zoomLevel;
                    cameraOffset.X += (scrollDelta > 0) ? -scrollSpeed : scrollSpeed;
                }
                else
                {
                    // Regular Scroll = Vertical movement
                    float scrollSpeed = 30.0f / zoomLevel;
                    cameraOffset.Y += (scrollDelta > 0) ? scrollSpeed : -scrollSpeed;
                }
            }
            
            // Camera movement (scaled by zoom for consistent feel)
            float moveSpeed = 5.0f / zoomLevel;
            if (keyboardState.IsKeyDown(Keys.Left))
                cameraOffset.X += moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Right))
                cameraOffset.X -= moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Up))
                cameraOffset.Y += moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Down))
                cameraOffset.Y -= moveSpeed;
            
            // Handle camera dragging with mouse + key
            if (keyboardState.IsKeyDown(Keys.Space)) // Use Space key for camera dragging
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!isCameraDragging)
                    {
                        // Start camera dragging
                        isCameraDragging = true;
                        lastCameraDragPosition = new Vector2(mouseState.X, mouseState.Y);
                    }
                    else
                    {
                        // Continue camera dragging
                        Vector2 currentMousePos = new Vector2(mouseState.X, mouseState.Y);
                        Vector2 dragDelta = (currentMousePos - lastCameraDragPosition) / zoomLevel;
                        cameraOffset += dragDelta;
                        lastCameraDragPosition = currentMousePos;
                    }
                }
                else
                {
                    isCameraDragging = false;
                }
            }
            else
            {
                isCameraDragging = false;
            }
            
            Vector2 mouseWorldPos = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            int gridX = (int)((mouseWorldPos.X - cameraOffset.X) / grid.CellSizeX);
            int gridY = (int)((mouseWorldPos.Y - cameraOffset.Y) / grid.CellSizeY);
            
            if (gridX >= 0 && gridX < grid.Width && gridY >= 0 && gridY < grid.Height)
            {
                selectedX = gridX;
                selectedY = gridY;
                
                // Only allow cell interactions when not camera dragging
                if (!isCameraDragging)
                {
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
                            cell.Alignment = (cell.Alignment + 1) % 20;
                        }
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
                grid = new Grid(45, 35, 28, 20);
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
            
            // Set target cell with T key (for line drawing)
            if (keyboardState.IsKeyDown(Keys.T) && !previousKeyboardState.IsKeyDown(Keys.T))
            {
                if (selectedX >= 0 && selectedY >= 0)
                {
                    targetCellX = selectedX;
                    targetCellY = selectedY;
                }
            }
            
            // Spawn visibility cone with V key
            if (keyboardState.IsKeyDown(Keys.V) && !previousKeyboardState.IsKeyDown(Keys.V))
            {
                if (visibilityCone == null)
                {
                    // Create cone at center of screen
                    Vector2 centerScreen = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    Vector2 focus = new VisibilityCone().SnapToGrid(centerScreen, cameraOffset, grid.CellSizeX, grid.CellSizeY);
                    Vector2 leftBorder = new VisibilityCone().SnapToGrid(centerScreen + new Vector2(-100, -50), cameraOffset, grid.CellSizeX, grid.CellSizeY);
                    Vector2 rightBorder = new VisibilityCone().SnapToGrid(centerScreen + new Vector2(100, -50), cameraOffset, grid.CellSizeX, grid.CellSizeY);
                    
                    visibilityCone = new VisibilityCone(focus, leftBorder, rightBorder, cameraOffset);
                }
                else
                {
                    // Toggle off
                    visibilityCone = null;
                    isDragging = false;
                    draggedPointIndex = -1;
                }
            }
            
            // Handle cone point dragging (only when not camera dragging)
            if (visibilityCone != null && visibilityCone.IsActive && !isCameraDragging)
            {
                Vector2 mousePosition = ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                
                if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    // Start dragging if near a cone point
                    int closestPoint = visibilityCone.GetClosestPoint(mousePosition, cameraOffset);
                    if (closestPoint >= 0)
                    {
                        isDragging = true;
                        draggedPointIndex = closestPoint;
                    }
                }
                
                if (isDragging && mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Update dragged point position with snapping
                    Vector2 snappedPos = visibilityCone.SnapToGrid(mousePosition, cameraOffset, grid.CellSizeX, grid.CellSizeY);
                    visibilityCone.SetPoint(draggedPointIndex, snappedPos, cameraOffset);
                }
                
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    isDragging = false;
                    draggedPointIndex = -1;
                }
            }
            
            previousMouseState = mouseState;
            previousKeyboardState = keyboardState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Apply zoom transformation
            Matrix transformMatrix = Matrix.CreateScale(zoomLevel) * Matrix.CreateTranslation(0, 0, 0);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, transformMatrix);
            
            grid.Draw(spriteBatch, pixelTexture, cameraOffset);
            
            // Draw start cell indicator
            Vector2 startPos = new Vector2(startCellX * grid.CellSizeX + cameraOffset.X - 2,
                                          startCellY * grid.CellSizeY + cameraOffset.Y - 2);
            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)startPos.X, (int)startPos.Y, grid.CellSizeX + 3, grid.CellSizeY + 3),
                Color.Lime * 0.6f);
            
            // Draw line from start cell to target cell (only when target is set)
            if (targetCellX >= 0 && targetCellY >= 0)
            {
                Vector2 startCenter = new Vector2(startCellX * grid.CellSizeX + grid.CellSizeX / 2 + cameraOffset.X,
                                                 startCellY * grid.CellSizeY + grid.CellSizeY / 2 + cameraOffset.Y);
                Vector2 targetCenter = new Vector2(targetCellX * grid.CellSizeX + grid.CellSizeX / 2 + cameraOffset.X,
                                                  targetCellY * grid.CellSizeY + grid.CellSizeY / 2 + cameraOffset.Y);
                
                DrawLine(spriteBatch, pixelTexture, startCenter, targetCenter, Color.Black, 4);
                
                // Draw target cell highlight
                Vector2 targetPosition = new Vector2(targetCellX * grid.CellSizeX + cameraOffset.X - 2, 
                                                    targetCellY * grid.CellSizeY + cameraOffset.Y - 2);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle((int)targetPosition.X, (int)targetPosition.Y, grid.CellSizeX + 3, grid.CellSizeY + 3), 
                    Color.Blue * 0.5f);
            }
            
            // Draw mouse selection highlight
            if (selectedX >= 0 && selectedY >= 0)
            {
                Vector2 position = new Vector2(selectedX * grid.CellSizeX + cameraOffset.X - 2, 
                                              selectedY * grid.CellSizeY + cameraOffset.Y - 2);
                spriteBatch.Draw(pixelTexture, 
                    new Rectangle((int)position.X, (int)position.Y, grid.CellSizeX + 3, grid.CellSizeY + 3), 
                    Color.Red * 0.5f);
            }
            
            // Draw visibility cone
            if (visibilityCone != null && visibilityCone.IsActive)
            {
                DrawVisibilityCone(spriteBatch, pixelTexture);
            }
            
            spriteBatch.End();
            
            // Draw UI elements without zoom transformation
            spriteBatch.Begin();
            
            // Draw cell info window
            DrawCellInfoWindow(spriteBatch, pixelTexture);
            
            // Draw controls window
            DrawControlsWindow(spriteBatch, pixelTexture);
            
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
            // Position window to the right of the grid (grid width: 45 * 28 = 1260px + 50px offset = 1310px)
            int windowX = 1330;
            int windowY = 50;
            int windowWidth = 250;
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
                    char alignmentLetter = (char)('A' + cell.Alignment);
                    DrawPixelText(spriteBatch, pixelTexture, $"Alignment: {alignmentLetter}", windowX + 10, windowY + 90, Color.Yellow);
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
        
        private void DrawVisibilityCone(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            // Dark purple color
            Color darkPurple = new Color(75, 0, 130); // Dark purple RGB
            Color lightPurple = new Color(138, 43, 226); // Lighter purple for hover
            
            // Get world coordinates for drawing
            Vector2 focusPoint = visibilityCone.GetPoint(0, cameraOffset);
            Vector2 leftBorderPoint = visibilityCone.GetPoint(1, cameraOffset);
            Vector2 rightBorderPoint = visibilityCone.GetPoint(2, cameraOffset);
            
            // Draw lines from focus point to border points
            DrawLine(spriteBatch, pixelTexture, focusPoint, leftBorderPoint, darkPurple, 3);
            DrawLine(spriteBatch, pixelTexture, focusPoint, rightBorderPoint, darkPurple, 3);
            
            // Draw arc from left border point to right border point around focus point
            DrawArc(spriteBatch, pixelTexture, focusPoint, leftBorderPoint, rightBorderPoint, darkPurple, 2);
            
            // Check for hover on points (convert mouse to world coordinates)
            Vector2 mousePosition = ScreenToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            int hoveredPoint = visibilityCone.GetClosestPoint(mousePosition, cameraOffset, 15f / zoomLevel);
            
            // Draw the three circular points
            int pointRadius = 4;
            
            for (int i = 0; i < 3; i++)
            {
                Vector2 point = visibilityCone.GetPoint(i, cameraOffset);
                Color pointColor = darkPurple;
                int currentRadius = pointRadius;
                
                // Highlight if hovered or being dragged
                if (i == hoveredPoint || (isDragging && i == draggedPointIndex))
                {
                    pointColor = lightPurple;
                    currentRadius = pointRadius + 2; // Make it slightly larger
                }
                
                DrawCircle(spriteBatch, pixelTexture, point, currentRadius, pointColor);
            }
        }
        
        private void DrawCircle(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 center, int radius, Color color)
        {
            // Draw a filled circle using rectangles
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        spriteBatch.Draw(pixelTexture, 
                            new Rectangle((int)center.X + x, (int)center.Y + y, 1, 1), 
                            color);
                    }
                }
            }
        }
        
        private void DrawArc(SpriteBatch spriteBatch, Texture2D pixelTexture, Vector2 center, Vector2 startPoint, Vector2 endPoint, Color color, int thickness)
        {
            // Calculate distances from center to both points and use the maximum as radius
            float radiusToStart = Vector2.Distance(center, startPoint);
            float radiusToEnd = Vector2.Distance(center, endPoint);
            float radius = Math.Max(radiusToStart, radiusToEnd);
            
            // Calculate angles for start and end points
            float startAngle = (float)Math.Atan2(startPoint.Y - center.Y, startPoint.X - center.X);
            float endAngle = (float)Math.Atan2(endPoint.Y - center.Y, endPoint.X - center.X);
            
            // Always go from left to right (counter-clockwise direction)
            // Normalize to ensure we go the correct direction
            if (endAngle < startAngle)
            {
                endAngle += (float)(2 * Math.PI);
            }
            
            float angleDiff = endAngle - startAngle;
            
            // Draw the arc with small angle increments
            float angleStep = 0.02f; // Small step for smooth arc
            int steps = (int)(angleDiff / angleStep);
            
            if (steps < 2) return; // Too small to draw
            
            Vector2 prevPoint = center + new Vector2(
                (float)(Math.Cos(startAngle) * radius),
                (float)(Math.Sin(startAngle) * radius)
            );
            
            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;
                float currentAngle = startAngle + angleDiff * t;
                
                Vector2 currentPoint = center + new Vector2(
                    (float)(Math.Cos(currentAngle) * radius),
                    (float)(Math.Sin(currentAngle) * radius)
                );
                
                // Draw line segment with specified thickness
                DrawLine(spriteBatch, pixelTexture, prevPoint, currentPoint, color, thickness);
                prevPoint = currentPoint;
            }
        }
        
        private void DrawControlsWindow(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            // Position window under the cell info window
            int windowX = 1330;
            int windowY = 190; // Cell info window is at y=50 with height=120, so 50+120+20 = 190
            int windowWidth = 250;
            int windowHeight = 290;
            
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
            
            int yPos = windowY + 10;
            int lineHeight = 16;
            
            // Title
            DrawPixelText(spriteBatch, pixelTexture, "CONTROLS", windowX + 10, yPos, Color.Cyan);
            yPos += lineHeight + 5;
            
            // Mouse controls
            DrawPixelText(spriteBatch, pixelTexture, "MOUSE:", windowX + 10, yPos, Color.Yellow);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Left Click - Toggle Blocked", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Right Click - Change Height", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Middle Click - Change Alignment", windowX + 10, yPos, Color.White);
            yPos += lineHeight + 5;
            
            // Keyboard controls
            DrawPixelText(spriteBatch, pixelTexture, "KEYBOARD:", windowX + 10, yPos, Color.Yellow);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "S - Set Start Cell", windowX + 10, yPos, Color.Lime);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "T - Set Target Cell", windowX + 10, yPos, Color.Blue);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "V - Toggle Visibility Cone", windowX + 10, yPos, new Color(75, 0, 130));
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Arrow Keys - Move Camera", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Scroll - Move Vertical", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "SHIFT+Scroll - Move Horizontal", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "SPACE+Drag - Move Camera", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "CTRL+Scroll - Zoom", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "R - Reset Grid", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "ESC - Exit", windowX + 10, yPos, Color.White);
            yPos += lineHeight + 5;
            
            // Visual legend
            DrawPixelText(spriteBatch, pixelTexture, "COLORS:", windowX + 10, yPos, Color.Yellow);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Red - Blocked Cell", windowX + 10, yPos, Color.Red);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Cyan Scale - Height 0-9", windowX + 10, yPos, Color.Cyan);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Green - Start Cell", windowX + 10, yPos, Color.Lime);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Blue - Target Cell", windowX + 10, yPos, Color.Blue);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Black Line - Start to Target", windowX + 10, yPos, Color.White);
            yPos += lineHeight;
            DrawPixelText(spriteBatch, pixelTexture, "Purple Cone - Visibility", windowX + 10, yPos, new Color(75, 0, 130));
        }
        
        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return screenPosition / zoomLevel;
        }
        
        private Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return worldPosition * zoomLevel;
        }
    }
}