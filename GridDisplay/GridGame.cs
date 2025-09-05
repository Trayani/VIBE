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
                
                DrawLine(spriteBatch, pixelTexture, startCenter, mouseCenter, Color.Cyan, 4);
                
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
            // Simple 5x7 bitmap characters - just draw basic rectangles for visibility
            switch (char.ToUpper(c))
            {
                case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H':
                case 'I': case 'J': case 'K': case 'L': case 'M': case 'N': case 'O': case 'P':
                case 'Q': case 'R': case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
                case 'Y': case 'Z':
                    // Draw a simple rectangle for letters
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 4, 7), color);
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 4, 1), color);
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + 3, 4, 1), color);
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + 6, 4, 1), color);
                    break;
                case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
                    // Draw numbers with distinct patterns
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 4, 7), color);
                    if (c != '1') spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 4, 1), color);
                    if (c != '1' && c != '7') spriteBatch.Draw(pixelTexture, new Rectangle(x, y + 6, 4, 1), color);
                    break;
                case '[': case ']': case '(': case ')':
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 2, 7), color);
                    break;
                case ',': case '.':
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + 5, 2, 2), color);
                    break;
                case ':':
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + 1, y + 2, 1, 1), color);
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + 1, y + 4, 1, 1), color);
                    break;
                case '-':
                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y + 3, 4, 1), color);
                    break;
                default:
                    // Default character (small rectangle)
                    spriteBatch.Draw(pixelTexture, new Rectangle(x + 1, y + 2, 2, 3), color);
                    break;
            }
        }
    }
}