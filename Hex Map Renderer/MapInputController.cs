using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System;

namespace HexMapRenderer
{    
    public class MapInputController : DrawableGameComponent 
    {
        #region Members
        
        private HexMap _map;
        private MouseState _lastMouseState;

        private HexTile _hoverTile;
        private Queue<HexTile> _endpoints;

        private SpriteBatch _spriteBatch;

        private Vector2 _halfTileSize;
        private float _cursorRadius;

        private KeyboardState _lastKeyState;

        private Path<HexTile> _foundPath;

        private Func<HexTile, HexTile, double> _distanceFunc;
        private Func<HexTile, HexTile, double> _estimateFunc;
        private Func<HexTile, IEnumerable<HexTile>> _findNeighboursFunc;

        private CameraService _camera;

        #endregion Members

        public MapInputController(Game game)
            : base(game)
        {
            base.Visible = this.Enabled = false;

            _distanceFunc = (t1, t2) => 0;
            _estimateFunc = (t1, t2) => Vector2.DistanceSquared(t1.Position, t2.Position);
            _findNeighboursFunc = t => { return _map.GetNeighbours(t).Where(nt => nt.TileType == HexTile.TileTypes.Walkable); };
        }

        #region DrawableGameComponent overrides

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(base.GraphicsDevice);            
        }

        public override void Initialize()
        {
            base.Initialize();

            _camera = base.Game.Services.GetService(typeof(CameraService)) as CameraService;
            if (null == _camera)
                throw new Exception("Unable to find CameraService");
        }

        public override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();

            if (state.IsKeyUp(Keys.Enter) && _lastKeyState.IsKeyDown(Keys.Enter) && 2 == _endpoints.Count)
                _foundPath = Pathfinder.FindPath<HexTile>(_endpoints.ElementAt(0), _endpoints.ElementAt(1),
                                                          _distanceFunc, _estimateFunc, _findNeighboursFunc);

            _lastKeyState = state;

            UpdateMouse();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);

            Vector2 tmpPos;
            var cursorSize = _cursorRadius * _camera.Zoom;

            if (null != _foundPath)
            {
                foreach (var node in _foundPath)
                {
                    ComputCursorPosition(node, out tmpPos);
                    _spriteBatch.DrawCircle(tmpPos, cursorSize * 0.5f, 6, Color.PowderBlue, 3f);
                }
            }

            if (0 != _endpoints.Count) {
                foreach (var point in _endpoints)
                {
                    ComputCursorPosition(point, out tmpPos);
                    _spriteBatch.DrawCircle(tmpPos, cursorSize, 6, Color.Red, 3f);
                }
            }

            if (null != _hoverTile)
            {
                ComputCursorPosition(_hoverTile, out tmpPos);
                _spriteBatch.DrawCircle(tmpPos, cursorSize, 6, Color.Green, 3f);
            }

            _spriteBatch.End();
        }

        #endregion DrawableGameComponent overrides

        #region Public Methods

        public void SetMap(HexMap map)
        {
            _endpoints = _endpoints ?? new Queue<HexTile>();
            _endpoints.Clear();

            _map = map;
            _halfTileSize = _map.Config.TileSize * .5f;

            _cursorRadius = _halfTileSize.Y;

            _hoverTile = null;

            base.Visible = base.Enabled = (null != _map);
        }

        public IEnumerable<HexTile> GetEndpoints() {
            return _endpoints.ToArray();
        }

        #endregion Public Methods

        #region Private Methods

        private void ComputCursorPosition(HexTile tile, out Vector2 position) { 
            position = tile.Position + _halfTileSize;
            Vector2.Transform(ref position, ref _camera.Matrix, out position);
        }

        private void UpdateMouse()
        {
            var mouseState = Mouse.GetState();

            var mousePosVec = new Vector2(mouseState.X, mouseState.Y);

            _hoverTile = _map.PickTile(ref mousePosVec);

            if (null != _hoverTile)
            {
                if (mouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed)
                    _hoverTile.TileType = _hoverTile.TileType == HexTile.TileTypes.Walkable ? HexTile.TileTypes.Wall : HexTile.TileTypes.Walkable;

                if (mouseState.RightButton == ButtonState.Released && _lastMouseState.RightButton == ButtonState.Pressed)
                {
                    if (_endpoints.Contains(_hoverTile))
                        _endpoints.Dequeue();
                    else if (_endpoints.Count < 2)
                        _endpoints.Enqueue(_hoverTile);
                }
            }

            _lastMouseState = mouseState;
        }

        #endregion Private Methods
    }    
}

