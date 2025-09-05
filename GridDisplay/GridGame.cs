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
            
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
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
    }
}