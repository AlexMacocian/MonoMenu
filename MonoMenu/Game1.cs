using MonoMenu.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoMenu
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Engine.MonoMenu xMenu;
        SpriteFont font;
        Texture2D pixelText;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferHeight = 800;
            //graphics.PreferredBackBufferWidth = 800;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            Monitor.Initialize();
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            xMenu.Resize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.MonoMenu.defaultFont = Content.Load<SpriteFont>("font");
            pixelText = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixelText.SetData(new[] { Color.White });
            font = Content.Load<SpriteFont>("font");
            xMenu = new Engine.MonoMenu(GraphicsDevice, Content);
            xMenu.Load("text.xml");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            MouseInput.Poll(gameTime);
            xMenu.Update(gameTime);
            Monitor.UpdateCalled();
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Monitor.DrawCalled();
            GraphicsDevice.Clear(Color.Transparent);
            xMenu.Draw(spriteBatch);
            spriteBatch.Begin();
            spriteBatch.Draw(pixelText, new Rectangle(MouseInput.MousePosition, new Point(3, 3)), Color.Red);
            spriteBatch.DrawString(font, "UpdateRate: " + Monitor.UpdateRate, new Vector2(10, 10), Color.Red, 0, new Vector2(0, 0), 0.25f, SpriteEffects.None, 1);
            spriteBatch.DrawString(font, "DrawRate: " + Monitor.FrameRate, new Vector2(10, 30), Color.Red, 0, new Vector2(0, 0), 0.25f, SpriteEffects.None, 1);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
