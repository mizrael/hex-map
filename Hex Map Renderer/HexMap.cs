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

    public class HexMap
    {
        #region Members

        private HexTile[,] _tiles;
        private Vector2 _tileSize;
        private Vector2 _hexOffset;
        private Texture2D _tilesAsset;

        private HexTile _selectedTile;

        #endregion Members

        public HexMap(int tileCountX, int tileCountY, Vector2 tileSize, Texture2D tilesAsset)
        {
            _tileSize = tileSize;
            _hexOffset = new Vector2(_tileSize.X * 0.75f, _tileSize.Y * -0.5f);

            _tiles = new HexTile[tileCountX, tileCountY];
            _tilesAsset = tilesAsset;

            SetupTiles();
        }

        #region Methods

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

        public void DrawDebug(SpriteBatch spriteBatch) {
            var h = _tileSize.Y;
            var hOver2 = h * .5f;

            var W = _tileSize.X;
            var w = _tileSize.X * 0.5f;            

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
        }

        /// <summary>
        /// WIP
        /// http://gamedev.stackexchange.com/questions/20742/how-can-i-implement-hexagonal-tilemap-picking-in-xna
        /// </summary>
        /// <param name="mousePos"></param>
        public void SelectTile(Vector2 mousePos) 
        {
            if (null != _selectedTile)
            {
                _selectedTile.Selected = false;
                _selectedTile = null;
            }

            var x = mousePos.X;
            var y = mousePos.Y;

            var h = _tileSize.Y;
            var W = _tileSize.X;
            var w = _tileSize.X * 0.5f;

            var k = (W + w) * .5f;

            var i = (int)Math.Floor(x / k); 
            var j = (int)Math.Floor((y * 2f) / h);

            var u = x - (k * i);
            var v = y - (h * 0.5f * j);            

            var is_i_even = IsEven(ref i);
            
            var isGreenArea = (u < (W - w) * 0.5f);
            if (isGreenArea) {
                var isUpper = (is_i_even && IsEven(ref j));
                u = (2f * u) / (W - w);
                v = (2f * v) / h;

                if ((!isUpper && v < u) || (isUpper && (1f - v) > u))
                {
                    i--;
                    is_i_even = !is_i_even;
                }
            }

            if (!is_i_even)
                j--;

            j = (int)Math.Floor(j * 0.5);

            if (i >= _tiles.GetLength(0)) return;
            if (i < 0) return;
            if (j >= _tiles.GetLength(1)) return;
            if (j < 0) return;

            _selectedTile = _tiles[i, j];

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
            
            var sourceRect = new Rectangle((int)_tileSize.X * 2, 0, (int)_tileSize.X, (int)_tileSize.Y);            

            for (int y = 0; y != _tiles.GetLength(1); ++y)
            {
                // ISO
                //hexPos.Y = _tileSize.Y * y * .5f;
                //hexPos.X = _hexOffset.X * y;     

                // GRID
                hexPos.Y = _tileSize.Y * y - _hexOffset.Y;
                hexPos.X = -_hexOffset.X;

                for (int x = 0; x != _tiles.GetLength(0); ++x)
                {
                    // ISO
                    //hexPos += _hexOffset;            
        
                    // GRID
                    hexPos.X += _hexOffset.X;
                    hexPos.Y += ((x & 1) == 1) ? -_hexOffset.Y : _hexOffset.Y;

                    _tiles[x, y] = new HexTile()
                    {
                        Selected = false,
                        SourceRectangle = sourceRect,
                        Position = hexPos
                    };
                }
            }
        }

        private Rectangle ComputeMapCulling()
        {
            /*var bounding = Rectangle.Empty;

            //midTile = GetCentreTile(gameCamera);
            var midTile = new Vector2(0, 0);
            bounding.Width = (int)(midTile.X + Math.Round((_halfScreenSize.X / (_tileSize.X)) / _cameraZoom));
            bounding.X = (int)(midTile.X - Math.Round((_halfScreenSize.X / (_tileSize.X)) / _cameraZoom));
            bounding.Height = (int)(midTile.Y + Math.Round((_halfScreenSize.Y / (_tileSize.Y)) / _cameraZoom));
            bounding.Y = (int)(midTile.Y - Math.Round((_halfScreenSize.Y / (_tileSize.Y)) / _cameraZoom));

            bounding.X = bounding.X >= 0 ? bounding.X : 0;
            bounding.Width = bounding.Width <= tileCountX - 1 ? bounding.Width : tileCountX - 1;
            bounding.Y = bounding.Y >= 0 ? bounding.Y : 0;
            bounding.Height = bounding.Height <= tileCountY - 1 ? bounding.Height : tileCountY - 1;

            return bounding;*/

            return new Rectangle(0, 0, _tiles.GetLength(0), _tiles.GetLength(1));
        }

        #endregion Private Methods
    }
}
