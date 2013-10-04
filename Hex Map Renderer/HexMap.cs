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
        public HexTile(int x, int y)
        {
            this.IndexX = x;
            this.IndexY = y;
        }

        public int IndexX;
        public int IndexY;
        public Rectangle SourceRectangle = Rectangle.Empty;
        public Vector2 Position = Vector2.Zero;        

        public TileTypes TileType = TileTypes.Walkable;

        public enum TileTypes
        {
            Walkable = 0,
            Wall,
        }

        public bool Equals(HexTile node)
        {
            return (this.IndexX == node.IndexX && this.IndexY == node.IndexY);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HexTile);
        }
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

        private HexTile[,] _tiles;
      
        private Vector2 _tileHalfSize;
        private Texture2D _tilesAsset;             

        private CameraService _camera;
        private Vector2 _topLeftScreenCoords;
        private Vector2 _bottomRightScreenCoords;

        private Rectangle _mapFullBounds;

        private float _h;
        private float _W;
        private float _w;
        private float _k;

        private static readonly IDictionary<HexTile.TileTypes, Color> _tileColors = new Dictionary<HexTile.TileTypes, Color>() 
        { 
            { HexTile.TileTypes.Walkable, Color.White}, 
            { HexTile.TileTypes.Wall, Color.Brown},           
        };

        #endregion Members

        public HexMap(Game game)
        {
            _camera = game.Services.GetService(typeof(CameraService)) as CameraService;            
        }

        #region Methods

        public void Load(HexMapConfig config, Texture2D tilesAsset)
        {
            Config = config;            

            _tiles = new HexTile[config.TilesCountX, config.TilesCountY];

            _tilesAsset = tilesAsset;

            _tileHalfSize = Config.TileSize * .5f;

            _h = Config.TileSize.Y;
            _W = Config.TileSize.X;
            _w = Config.TileSize.X * Config.WidthScale;
            _k = (_W + _w) * .5f;

            _mapFullBounds = new Rectangle(0, 0, Config.TilesCountX, Config.TilesCountY);
            _topLeftScreenCoords = Config.TileSize * -1f;
            _bottomRightScreenCoords = _camera.ScreenSize + Config.TileSize * new Vector2(1f, 2f);

            SetupTiles();            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var mapCullingBounds = ComputeBoundTilesIndices();

            for (int y = mapCullingBounds.Y; y != mapCullingBounds.Height; ++y)
            {
                for (int x = mapCullingBounds.X; x != mapCullingBounds.Width; ++x)
                {
                    var currTile = _tiles[x, y];
                    spriteBatch.Draw(_tilesAsset, currTile.Position, currTile.SourceRectangle, 
                                     _tileColors[currTile.TileType],
                                     0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, SpriteFont font) {
            var h = Config.TileSize.Y;
            var hOver2 = h * .5f;

            var W = Config.TileSize.X;
            var w = Config.TileSize.X * 0.5f;            

            var k = (W-w) * 0.5f;

            var mapCullingBounds = ComputeBoundTilesIndices();

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

                    FontHelpers.Print(spriteBatch, font, string.Format("{0},{1}", x, y), currTile.Position + Config.TileSize * .5f, 0.5f, Color.White, true);
                }
            }

            var midCenterTile = FindMidCenterTile();
            if (null != midCenterTile) {

                var position = midCenterTile.Position + Config.TileSize * .5f;             

                spriteBatch.DrawCircle(position, _camera.Zoom * Config.TileSize.Y * 0.5f, 6, Color.PowderBlue, 3f);
            }
        }

        /// <summary>      
        /// http://gamedev.stackexchange.com/questions/20742/how-can-i-implement-hexagonal-tilemap-picking-in-xna
        /// </summary>
        /// <param name="screenCoords"></param>
        public HexTile PickTile(Vector2 screenCoords)
        {                       
            Vector2.Transform(ref screenCoords, ref _camera.InverseMatrix, out screenCoords);

            var i = (int)Math.Floor(screenCoords.X / _k);
            var j = (int)Math.Floor((screenCoords .Y * 2f) / _h);

            var u = screenCoords.X - (_k * i);
            var v = screenCoords.Y - (_h * j * 0.5f);

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

        public IEnumerable<HexTile> GetNeighbours(HexTile tile) {
            var indexes_even = new[]{
                new[]{-1,-1}, new[]{0,-1}, new[]{1,-1},
                new[]{-1, 0}, new[] {0, 1}, new[]{1, 0},                            
            };

            var indexes_odd = new[]{
                new[]{-1,0}, new[]{0,-1}, new[]{1,0},
                new[]{-1, 1}, new[] {0, 1}, new[]{1, 1},                            
            };

            var indexes = IsEven(ref tile.IndexX) ? indexes_even : indexes_odd;

            var maxX = _tiles.GetLength(0);
            var maxY = _tiles.GetLength(1);

            for (int i = 0; i != indexes.Length; ++i)
            {
                var nx = indexes[i][0] + tile.IndexX;
                var ny = indexes[i][1] + tile.IndexY;
                if (nx >= 0 && ny >= 0 && nx < maxX && ny < maxY)
                    yield return _tiles[nx, ny];
            }           
        }

        #endregion Methods

        #region Private Methods

        private void SetupTiles()
        {
            var hexPos = Vector2.Zero;
            
            var sourceRect = new Rectangle(0, 0, (int)Config.TileSize.X, (int)Config.TileSize.Y);
            var hexOffset = new Vector2(Config.TileSize.X * 0.75f, Config.TileSize.Y * -0.5f);

            for (int y = 0; y != _tiles.GetLength(1); ++y)
            {
                // ISO
                //hexPos.Y = _config.TileSize.Y * y * .5f;
                //hexPos.X = hexOffset.X * y;     

                // GRID
                hexPos.Y = Config.TileSize.Y * y - hexOffset.Y;
                hexPos.X = -hexOffset.X;

                for (int x = 0; x != _tiles.GetLength(0); ++x)
                {
                    // ISO
                    //hexPos += hexOffset;            
        
                    // GRID
                    hexPos.X += hexOffset.X;
                    hexPos.Y += ((x & 1) == 1) ? -hexOffset.Y : hexOffset.Y;

                    _tiles[x, y] = new HexTile(x,y)
                    {                      
                        SourceRectangle = sourceRect,
                        Position = hexPos,
                        TileType = HexTile.TileTypes.Walkable
                    };
                }
            }
        }
            
        private Rectangle ComputeBoundTilesIndices()
        {
            var bounding = _mapFullBounds;

            var topLeftTile = PickTile(_topLeftScreenCoords);
            if (null != topLeftTile)
            {
                bounding.X = topLeftTile.IndexX;
                bounding.Y = topLeftTile.IndexY;
            }

            var bottomRightTile = PickTile(_bottomRightScreenCoords);
            if (null != bottomRightTile)
            {
                bounding.Width = bottomRightTile.IndexX;
                bounding.Height = bottomRightTile.IndexY;
            }           

            return bounding;
        }

        private HexTile FindMidCenterTile()
        {
            return PickTile(_camera.HalfScreenSize);      
        }

        private static bool IsEven(ref int value)
        {
            return 0 == (value & 1);
        }

        private static int Clamp(int value, int min, int max) {
            if (value >= min && value <= max) return value;
            if (value < min) return min;
            return max;
        }

        #endregion Private Methods

        #region Properties

        public HexMapConfig Config { get; private set; }

        #endregion Properties
    }
}
