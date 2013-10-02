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

        #endregion Members

        public MapInputController(Game game) : base(game) {
            base.Visible = this.Enabled = false;              
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(base.GraphicsDevice);
        }

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

        public override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();

            if (state.IsKeyUp(Keys.Enter) && _lastKeyState.IsKeyDown(Keys.Enter) && 2 == _endpoints.Count)
            {
                Func<HexTile, HexTile, double> distance = (t1, t2) => { return (t1.TileType == HexTile.TileTypes.Wall || t2.TileType == HexTile.TileTypes.Wall) ? 1000 : 0; };
                Func<HexTile, HexTile, double> estimate = (t1, t2) => Vector2.DistanceSquared(t1.Position, t2.Position);
                Func<HexTile, IEnumerable<HexTile>> findNeighbours = t =>{return _map.GetNeighbours(t).Where(nt => nt.TileType == HexTile.TileTypes.Walkable);};
                
                _foundPath = Pathfinder.FindPath<HexTile>(_endpoints.ElementAt(0), _endpoints.ElementAt(1),
                                                          distance, estimate, findNeighbours);
            }

            _lastKeyState = state;

            UpdateMouse();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);

            if (null != _foundPath)
            {
                foreach (var node in _foundPath)
                {
                    _spriteBatch.DrawCircle(node.Position + _halfTileSize, _cursorRadius * 0.5f, 6, Color.PowderBlue, 3f);
                }
            }

            if (0 != _endpoints.Count) { 
                foreach(var point in _endpoints)
                    _spriteBatch.DrawCircle(point.Position + _halfTileSize, _cursorRadius, 6, Color.Red, 3f);
            }

            if (null != _hoverTile)
                _spriteBatch.DrawCircle(_hoverTile.Position + _halfTileSize, _cursorRadius, 6, Color.Green, 3f);           

            _spriteBatch.End();
        }

        public IEnumerable<HexTile> GetEndpoints() {
            return _endpoints.ToArray();
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

    }    
}

