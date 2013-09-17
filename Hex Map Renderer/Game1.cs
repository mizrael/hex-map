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

        Vector2 _cameraPos = Vector2.Zero;
        float _cameraZoom = 1f;
        Matrix _cameraMatrix;

        Vector3 _halfScreenSize;

        KeyboardState _lastState;

        SpriteFont _font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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

            var hexTexture = this.Content.Load<Texture2D>("tiles");
            var tileSize = new Vector2(hexTexture.Width / 5, hexTexture.Height / 2);            
            
            _hexMap = new HexMap(10, 10, tileSize, hexTexture);

            _hexMap.SelectTile(new Vector2(153, 116));

            _halfScreenSize = new Vector3(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f, 0);

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
            _hexMap.SelectTile(new Vector2(mouseState.X, mouseState.Y));

            var cameraOffset = 5f;

            var keyboardState = Keyboard.GetState();

            if (_lastState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.W))
                _cameraPos.Y -= cameraOffset;
            else if (_lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.S))
                _cameraPos.Y += cameraOffset;

            if (_lastState.IsKeyDown(Keys.A) && keyboardState.IsKeyDown(Keys.A))
                _cameraPos.X -= cameraOffset;
            else if (_lastState.IsKeyDown(Keys.D) && keyboardState.IsKeyDown(Keys.D))
                _cameraPos.X += cameraOffset;

            if (_lastState.IsKeyDown(Keys.Q) && keyboardState.IsKeyDown(Keys.Q))
                _cameraZoom += 0.01f;
            else if (_lastState.IsKeyDown(Keys.E) && keyboardState.IsKeyDown(Keys.E))
                _cameraZoom -= 0.01f;

            _lastState = keyboardState;

            _cameraMatrix = Matrix.CreateTranslation(new Vector3(-_cameraPos.X, -_cameraPos.Y, 0)) *                                                    
                                                     Matrix.CreateScale(new Vector3(_cameraZoom, _cameraZoom, 1)) *
                                                     Matrix.CreateTranslation(_halfScreenSize);            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _cameraMatrix);            
            spriteBatch.Begin();            

            _hexMap.Draw(spriteBatch);

            var mouseState = Mouse.GetState();
            FontHelpers.Print(spriteBatch, _font, string.Format("x: {0}, y: {1}", mouseState.X, mouseState.Y) , new Vector2(500, 0), 0.7f, Color.White, false);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

}
