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
    public class CameraService : GameComponent
    {
        #region Members
        
        KeyboardState _lastState;

        Vector3 _pos3 = Vector3.Zero;
        Vector3 _zoom3 = Vector3.One;

        #endregion Members

        public CameraService(Game game) : base(game) { }

        public override void Update(GameTime gameTime) {
            var cameraOffset = 5f;

            var keyboardState = Keyboard.GetState();

            if (_lastState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.W))
                Position.Y -= cameraOffset;
            else if (_lastState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.S))
                Position.Y += cameraOffset;

            if (_lastState.IsKeyDown(Keys.A) && keyboardState.IsKeyDown(Keys.A))
                Position.X -= cameraOffset;
            else if (_lastState.IsKeyDown(Keys.D) && keyboardState.IsKeyDown(Keys.D))
                Position.X += cameraOffset;

            if (_lastState.IsKeyDown(Keys.Q) && keyboardState.IsKeyDown(Keys.Q))
                Zoom += 0.01f;
            else if (_lastState.IsKeyDown(Keys.E) && keyboardState.IsKeyDown(Keys.E))
                Zoom -= 0.01f;

            _lastState = keyboardState;

            _pos3.X = -Position.X;
            _pos3.Y = -Position.Y;

            _zoom3.X = Zoom;
            _zoom3.Y = Zoom;

            Matrix = Matrix.CreateTranslation(_pos3) *
                     Matrix.CreateScale(_zoom3);
        }

        #region Properties

        public Vector2 Position = Vector2.Zero;
        public float Zoom = 1f;
        public Matrix Matrix;

        #endregion Properties
    }
}
