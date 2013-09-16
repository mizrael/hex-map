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
            var w = _tileSize.X * 0.75f;

            var i = (int)Math.Floor((x * 2f) / (W + w));
            var j = (int)Math.Floor((y * 2f) / h);

            var u = x - (i * ((W + w) * 0.5f));
            var v = y - (j * h * 0.5f);            

            var isGreenArea = (u < (W - w) * 0.5f);
            if (isGreenArea) {
                var isUpper = (IsEven(ref i) && IsEven(ref j));

                //u = (2f * u) / (W - w);
                //v = (2f * v) / h;

                if ((isUpper && ((1f - v) > u) || (!isUpper && v < u)))
                    i--;                
            }

            if (!IsEven(ref i))
                j--;

            j = (int)Math.Floor(j * 0.5);

            if (i >= _tiles.GetLength(0)) return;
            if (i < 0) return;
            if (j >= _tiles.GetLength(1)) return;
            if (j < 0) return;
          //  ClampIndices(ref i, ref j);

            _selectedTile = _tiles[i, j];

            _selectedTile.Selected = true;
        }

        private void ClampIndices(ref int i, ref int j) {
            if (i >= _tiles.GetLength(0))
                i = _tiles.GetLength(0) - 1;
            if (i < 0) i = 0;
            if (j >= _tiles.GetLength(1))
                j = _tiles.GetLength(1) - 1;
            if (j < 0) j = 0;
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
