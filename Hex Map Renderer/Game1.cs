using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HexMapRenderer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        HexMap _hexMap;
        
        SpriteFont _font;

        CameraService _camera;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _camera = new CameraService(this);
            this.Components.Add(_camera);
            this.Services.AddService(typeof(CameraService), _camera);
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

            this.IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var hexTexture = this.Content.Load<Texture2D>("dwarven-castle-floor");
            var tileSize = new Vector2(hexTexture.Width, hexTexture.Height);            
            
            _hexMap = new HexMap(this);
            _hexMap.Load(new HexMapConfig()
            {
                TilesCountX = 30,
                TilesCountY = 30,
                TileSize = tileSize
            }, hexTexture);           

            _font = this.Content.Load<SpriteFont>(@"Commodore");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            var mouseState = Mouse.GetState();        

            var mousePosVec = new Vector2(mouseState.X, mouseState.Y);
            _hexMap.SelectTile(ref mousePosVec);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _camera.Matrix);            
          
            _hexMap.Draw(spriteBatch);

#if DEBUG
            _hexMap.DrawDebug(spriteBatch, _font);
            var mouseState = Mouse.GetState();
            FontHelpers.Print(spriteBatch, _font, string.Format("x: {0}, y: {1}", mouseState.X, mouseState.Y) , new Vector2(500, 0), 0.7f, Color.White, false);
#endif

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }

}
