using AJStudios.Puzzle.Gameplay;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AJStudios.Puzzle.Core
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private int borderSize;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject[] gamePiecePrefab;

        [SerializeField] private float swapSpeed;

        private Tile[,] _allTiles;
        private Tile _clickedTile; 
        private Tile _targetTile;

        private GamePiece[,] _allGamePiece;

        private void Start()
        {
            _allTiles = new Tile[width, height];
            _allGamePiece = new GamePiece[width, height];
            SetupTiles();
            SetupCamera();
            FillRandom();
            HighlightMatches();
        }

        private void SetupTiles()
        {
            for(int i=0; i<width; i++)
            {
                for(int j=0; j<height; j++)
                {
                    GameObject tileObject = Instantiate(tilePrefab, new Vector3(i,j,0), Quaternion.identity, transform);

                    tileObject.name = "Tile ("+i+","+j+")";

                    _allTiles[i, j] = tileObject.GetComponent<Tile>();

                    _allTiles[i, j].Init(i,j,this);
                }
            }
        }

        private void SetupCamera()
        {
            Camera.main.transform.position = new Vector3(
                    (float)(width -1) * 0.5f,
                    (float)(height - 1) * 0.5f,
                    -10f
                );

            float aspectRatio = (float) Screen.width / (float) Screen.height;

            float verticalSize = (float) height/2f + (float) borderSize;
            float horizontalSize = ((float) height/2f + (float) borderSize)/aspectRatio;

            Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

        }

        private GameObject GetRandomPiece()
        {
            int randomIndex = Random.Range(0, gamePiecePrefab.Length);

            if (gamePiecePrefab[randomIndex] == null)
            {
                Debug.LogWarning("Gamepiece missing at "+ randomIndex);
                return null;
            }

            return gamePiecePrefab[randomIndex];
        }

        public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
        {
            if (gamePiece == null) return;

            gamePiece.transform.position = new Vector3(x,y,0);
            gamePiece.transform.rotation = Quaternion.identity;
            if(IsWithinBounds(x,y))
            {
                _allGamePiece[x, y] = gamePiece;
            }
            gamePiece.SetCoordinates(x,y);

        }

        private bool IsWithinBounds(int x, int y)
        {
            return (x >= 0 && x < width && y >= 0 && y < height);
        }

        private void FillRandom()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    GameObject gamePiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity, transform);

                    if(gamePiece != null)
                    {
                        gamePiece.GetComponent<GamePiece>().Init(this);
                        PlaceGamePiece(gamePiece.GetComponent<GamePiece>(), i, j);
                    }
                }
            }
        }

        public void ClickTile(Tile tile)
        {
            if(_clickedTile == null)
            {
                _clickedTile = tile;
            }
        }

        public void DragToTile(Tile tile)
        {
            if(_clickedTile != null && IsNextTo(_clickedTile, tile))
            {
                _targetTile = tile;
            }
        }

        public void ReleaseTile()
        {
            if(_clickedTile != null && _targetTile != null)
            {
                SwitchTile(_clickedTile, _targetTile);
            }

            _clickedTile = null;
            _targetTile = null;
        }

        private void SwitchTile(Tile clickedTile, Tile targetTile)
        {
            GamePiece clickedPiece = _allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = _allGamePiece[targetTile.xIndex, targetTile.yIndex];

            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);
        }

        private bool IsNextTo(Tile start, Tile end)
        {
            if(Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
            {
                return true;
            }

            if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
            {
                return true;
            }

            return false;
        }

        List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            GamePiece startPiece = null;

            if(IsWithinBounds(startX, startY))
            {
                startPiece = _allGamePiece[startX, startY];
            }

            if(startPiece != null)
            {
                matches.Add(startPiece);
            }

            int maxValue = (width > height) ? width : height;

            int nextX;
            int nextY;

            for(int i = 1; i < maxValue-1; i++)
            {
                nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
                nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

                if(!IsWithinBounds(nextX, nextY))
                {
                    break;
                }

                GamePiece nextPiece = _allGamePiece[nextX, nextY];
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }

            }

            if(matches.Count >= minLength)
            {
                return matches;
            }

            return null;
        }

        List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
        {
            List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
            List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

            if (upwardMatches == null) upwardMatches = new List<GamePiece>();
            if (downwardMatches == null) downwardMatches = new List<GamePiece>();

            var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

            return (combinedMatches.Count >= minLength) ? combinedMatches : null;
        }

        List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
        {
            List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
            List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

            if (rightMatches == null) rightMatches = new List<GamePiece>();
            if (leftMatches == null) leftMatches = new List<GamePiece>();

            var combinedMatches = rightMatches.Union(leftMatches).ToList();

            return (combinedMatches.Count >= minLength) ? combinedMatches : null;
        }

        private void HighlightMatches()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    SpriteRenderer spriteRenderer = _allTiles[i, j].GetComponent<SpriteRenderer>();
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.25f);

                    List<GamePiece> verticalMatch = FindVerticalMatches(i, j, 3);
                    List<GamePiece> horizonatMatch = FindHorizontalMatches(i, j, 3);

                    if (verticalMatch == null) verticalMatch = new List<GamePiece>();
                    if (horizonatMatch == null) horizonatMatch = new List<GamePiece>();

                    var combinedMatch = verticalMatch.Union(horizonatMatch).ToList();

                    if(combinedMatch.Count > 0)
                    {
                        foreach(GamePiece piece in combinedMatch)
                        {
                            if(piece != null)
                            {
                                spriteRenderer = _allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
                                spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
                            }
                        }
                    }

                }
            }
        }
    }
}

