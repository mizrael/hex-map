using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HexMapRenderer
{
    public class HexTile
    {
        public Rectangle SourceRectangle;
        public Vector2 Position;
        public bool Selected;
    }

    public class HexMapConfig {
        public int TilesCountX = 10;
        public int TilesCountY = 10;
        public Vector2 TileSize;
        public float WidthScale = .5f;
    }

    public class HexMap
    {
        #region Members

        private HexMapConfig _config;

        private HexTile[,] _tiles;
      
        private Vector2 _tileHalfSize;
        private Texture2D _tilesAsset;

        private HexTile _selectedTile;

        private CameraService _camera;

        private Rectangle _mapFullBounds;

        private float _h;
        private float _W;
        private float _w;
        private float _k;

        #endregion Members

        public HexMap(Game game)
        {
            _camera = game.Services.GetService(typeof(CameraService)) as CameraService;            
        }

        #region Methods

        public void Load(HexMapConfig config, Texture2D tilesAsset)
        {
            _config = config;            

            _tiles = new HexTile[config.TilesCountX, config.TilesCountY];

            _tilesAsset = tilesAsset;

            _tileHalfSize = _config.TileSize * .5f;

            _h = _config.TileSize.Y;
            _W = _config.TileSize.X;
            _w = _config.TileSize.X * _config.WidthScale;
            _k = (_W + _w) * .5f;

            _mapFullBounds = new Rectangle(0, 0, _config.TilesCountX, _config.TilesCountY);

            SetupTiles();            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var mapCullingBounds = ComputeMapCulling();

            for (int y = mapCullingBounds.Y; y != mapCullingBounds.Height; ++y)
            {
                for (int x = mapCullingBounds.X; x != mapCullingBounds.Width; ++x)
                {
                    var currTile = _tiles[x, y];
                    spriteBatch.Draw(_tilesAsset, currTile.Position, currTile.SourceRectangle, 
                                     currTile.Selected ? Color.Red : Color.White, 
                                     0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, SpriteFont font) {
            var h = _config.TileSize.Y;
            var hOver2 = h * .5f;

            var W = _config.TileSize.X;
            var w = _config.TileSize.X * 0.5f;            

            var k = (W-w) * 0.5f;

            var mapCullingBounds = ComputeMapCulling();

            for (int x = mapCullingBounds.X; x != mapCullingBounds.Width; ++x) {
                var currTile = _tiles[x, 0];

                var firstLineX = k + currTile.Position.X;

                var pos = new Vector2(firstLineX, 0f);
                spriteBatch.DrawLine(pos, 800, MathHelper.PiOver2, Color.GreenYellow);

                pos = new Vector2(firstLineX + w, 0f);
                spriteBatch.DrawLine(pos, 800, MathHelper.PiOver2, Color.GreenYellow);               
            }

            for (int y = mapCullingBounds.Y; y != mapCullingBounds.Height; ++y)
            {
                var currTile = _tiles[0, y];

                var firstLineY = currTile.Position.Y;

                var pos = new Vector2(0f, firstLineY);
                spriteBatch.DrawLine(pos, 800, 0f, Color.GreenYellow);

                pos = new Vector2(0f, firstLineY + hOver2);
                spriteBatch.DrawLine(pos, 800, 0f, Color.GreenYellow);
            }

            for (int y = mapCullingBounds.Y; y != mapCullingBounds.Height; ++y)
            {
                for (int x = mapCullingBounds.X; x != mapCullingBounds.Width; ++x)
                {
                    var currTile = _tiles[x, y];

                    FontHelpers.Print(spriteBatch, font, string.Format("{0},{1}", x, y), currTile.Position + _config.TileSize * .5f, 0.5f, Color.White, true);
                }
            }
        }

        /// <summary>      
        /// http://gamedev.stackexchange.com/questions/20742/how-can-i-implement-hexagonal-tilemap-picking-in-xna
        /// </summary>
        /// <param name="screenCoords"></param>
        public void SelectTile(ref Vector2 screenCoords) 
        {
            if (null != _selectedTile)
            {
                _selectedTile.Selected = false;
                _selectedTile = null;
            }

            _selectedTile = PickTile(ref screenCoords);
            if(null != _selectedTile)
                _selectedTile.Selected = true;
        }

        private static bool IsEven(ref int value) {
            return 0 == (value & 1);
        }

        #endregion Methods

        #region Private Methods

        private void SetupTiles()
        {
            var hexPos = Vector2.Zero;
            
            var sourceRect = new Rectangle(0, 0, (int)_config.TileSize.X, (int)_config.TileSize.Y);
            var hexOffset = new Vector2(_config.TileSize.X * 0.75f, _config.TileSize.Y * -0.5f);

            for (int y = 0; y != _tiles.GetLength(1); ++y)
            {
                // ISO
                //hexPos.Y = _config.TileSize.Y * y * .5f;
                //hexPos.X = hexOffset.X * y;     

                // GRID
                hexPos.Y = _config.TileSize.Y * y - hexOffset.Y;
                hexPos.X = -hexOffset.X;

                for (int x = 0; x != _tiles.GetLength(0); ++x)
                {
                    // ISO
                    //hexPos += hexOffset;            
        
                    // GRID
                    hexPos.X += hexOffset.X;
                    hexPos.Y += ((x & 1) == 1) ? -hexOffset.Y : hexOffset.Y;

                    _tiles[x, y] = new HexTile()
                    {
                        Selected = false,
                        SourceRectangle = sourceRect,
                        Position = hexPos
                    };
                }
            }
        }

        private HexTile PickTile(ref Vector2 screenCoords)
        {
            var x = screenCoords.X + _camera.Position.X;
            var y = screenCoords.Y + _camera.Position.Y;

            var i = (int)Math.Floor(x / _k);
            var j = (int)Math.Floor((y * 2f) / _h);

            var u = x - (_k * i);
            var v = y - (_h * j * 0.5f);

            var is_i_even = IsEven(ref i);

            var isGreenArea = (u < (_W - _w) * 0.5f);
            if (isGreenArea)
            {
                var is_j_even = IsEven(ref j);

                var isUpper = (0 == ((i + j) & 1));
                u = (2f * u) / (_W - _w);
                v = (2f * v) / _h;

                if ((!isUpper && v > u) || (isUpper && (1f - v) > u))
                {
                    i--;
                    is_i_even = !is_i_even;
                }
            }

            if (!is_i_even)
                j--;

            j = (int)Math.Floor(j * 0.5);

            if (i < 0 || i >= _tiles.GetLength(0)) return null;
            if (j < 0 || j >= _tiles.GetLength(1)) return null;

            return _tiles[i, j];
        }

        private Rectangle ComputeMapCulling()
        {
            var bounding = Rectangle.Empty;
            
            var halfScreenSize = new Vector2(_camera.Game.GraphicsDevice.Viewport.Width * 0.5f, _camera.Game.GraphicsDevice.Viewport.Height * 0.5f);

            var midTile = PickTile(ref halfScreenSize);
            if (null == midTile)
                return _mapFullBounds;

            var midTileCenter = midTile.Position + _tileHalfSize;

            bounding.X = (int)Math.Floor((midTileCenter.X - halfScreenSize.X) / _config.TileSize.X);
            bounding.Y = (int)Math.Floor((midTileCenter.Y - halfScreenSize.Y) / _config.TileSize.Y) ;

            bounding.Width = (int)Math.Ceiling((midTileCenter.X + halfScreenSize.X) / _config.TileSize.X);
            bounding.Height = (int)Math.Ceiling((midTileCenter.Y + halfScreenSize.Y) / _config.TileSize.Y);

            bounding.X = bounding.X >= 0 ? bounding.X : 0;            
            bounding.Y = bounding.Y >= 0 ? bounding.Y : 0;

            bounding.Width = bounding.Width > _config.TilesCountX ? _config.TilesCountX : bounding.Width;
            bounding.Height = bounding.Height > _config.TilesCountY ? _config.TilesCountY : bounding.Height;            

            return bounding;
        }

        #endregion Private Methods
    }
}
