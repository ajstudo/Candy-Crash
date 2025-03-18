using AJStudios.Puzzle.Gameplay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

        private bool _playerInputEnabled = true;

        private void Start()
        {
            _allTiles = new Tile[width, height];
            _allGamePiece = new GamePiece[width, height];
            SetupTiles();
            SetupCamera();
            FillBoard();
            //HighlightMatches();
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

        private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {
            int maxIteration = 100;
            int iteration = 0;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if (_allGamePiece[i, j] == null)
                    {
                        GamePiece gamePiece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iteration = 0;

                        while (HasMatchPieceAt(i, j))
                        {
                            ClearMatchPieceAt(i, j);
                            gamePiece = FillRandomAt(i, j);

                            iteration++;

                            if (iteration >= maxIteration)
                            {
                                Debug.Log("break =====================");
                                break;
                            }
                        }
                    }      
                }
            }
        }

        private GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            GameObject gamePiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity, transform);

            if (gamePiece != null)
            {
                gamePiece.GetComponent<GamePiece>().Init(this);
                PlaceGamePiece(gamePiece.GetComponent<GamePiece>(), x, y);

                if(falseYOffset != 0)
                {
                    gamePiece.transform.position = new Vector3(x, falseYOffset , 0f);
                    gamePiece.GetComponent<GamePiece>().Move(x, y, moveTime);
                }

                return gamePiece.GetComponent<GamePiece>();
            }

            return null;
        }

        private bool HasMatchPieceAt(int x, int y, int minLength = 3)
        {
            List<GamePiece> leftMatches = FindMatchesAt(x, y);
            List<GamePiece> downMatches = FindMatchesAt(x, y);

            if(leftMatches == null)
            {
                leftMatches = new List<GamePiece>();
            }

            if(downMatches == null)
            {
                downMatches = new List<GamePiece>();
            }

            return (leftMatches.Count > 0 || downMatches.Count > 0);
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
                SwitchTiles(_clickedTile, _targetTile);
            }

            _clickedTile = null;
            _targetTile = null;
        }

        private void SwitchTiles(Tile clickedTile, Tile targetTile)
        {
            StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
        }

        private  IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
        {
            if (_playerInputEnabled)
            {
                GamePiece clickedPiece = _allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
                GamePiece targetPiece = _allGamePiece[targetTile.xIndex, targetTile.yIndex];

                if (clickedPiece != null && targetPiece != null)
                {
                    clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
                    targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);

                    yield return new WaitForSeconds(swapSpeed);

                    List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedPiece.xIndex, clickedPiece.yIndex);
                    List<GamePiece> targetPieceMatches = FindMatchesAt(targetPiece.xIndex, targetPiece.yIndex);

                    if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                    {
                        clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);
                        targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
                    }
                    else
                    {
                        yield return new WaitForSeconds(swapSpeed);

                        ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    }
                }
            }   
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

        private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
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

                if(nextPiece == null)
                {
                    break;
                }
                else
                {
                    if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                    {
                        matches.Add(nextPiece);
                    }
                    else
                    {
                        break;
                    }
                }          
            }

            if(matches.Count >= minLength)
            {
                return matches;
            }

            return null;
        }

        private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
        {
            List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
            List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

            if (upwardMatches == null) upwardMatches = new List<GamePiece>();
            if (downwardMatches == null) downwardMatches = new List<GamePiece>();

            var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

            return (combinedMatches.Count >= minLength) ? combinedMatches : null;
        }

        private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
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
                    HighlightMatchesAt(i, j);

                }
            }
        }

        private void HighlightMatchesAt(int i, int j)
        {
            SpriteRenderer spriteRenderer = HighlightOff(i, j);

            List<GamePiece> combinedMatch = FindMatchesAt(i, j);

            if (combinedMatch.Count > 0)
            {
                foreach (GamePiece piece in combinedMatch)
                {
                    spriteRenderer = HighlightOn(spriteRenderer, piece);
                }
            }
        }

        private void HighlightMatchPieces(List<GamePiece> gamePieces)
        {
            foreach(GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    HighlightOn(_allGamePiece[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>(), piece);
                }
            }
        }

        private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
        {
            List<GamePiece> verticalMatch = FindVerticalMatches(x, y, minLength);
            List<GamePiece> horizonatMatch = FindHorizontalMatches(x, y, minLength);

            if (verticalMatch == null) verticalMatch = new List<GamePiece>();
            if (horizonatMatch == null) horizonatMatch = new List<GamePiece>();

            var combinedMatch = verticalMatch.Union(horizonatMatch).ToList();
            return combinedMatch;
        }

        private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            foreach(GamePiece piece in gamePieces)
            {
                matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
            }

            return matches;
        }

        List<GamePiece> FindAllMatches()
        {
            List<GamePiece> combinedMatches = new List<GamePiece>();

            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    List<GamePiece> matches = FindMatchesAt(i, j);
                    combinedMatches = combinedMatches.Union(matches).ToList();
                }
            }

            return combinedMatches;
        }

        private SpriteRenderer HighlightOff(int i, int j)
        {
            SpriteRenderer spriteRenderer = _allTiles[i, j].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            return spriteRenderer;
        }
       
        private SpriteRenderer HighlightOn(SpriteRenderer spriteRenderer, GamePiece piece)
        {
            if (piece != null)
            {
                spriteRenderer = _allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
                spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
            }

            return spriteRenderer;
        }

        /*private void HighlightOn(int x, int y, Color color)
        {
            SpriteRenderer spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
        }*/

        private void ClearMatchPieceAt(int x, int y)
        {
            GamePiece gamePiece = _allGamePiece[x, y];
            if(gamePiece != null)
            {
                _allGamePiece[x, y] = null;
                Destroy(gamePiece.gameObject);
            }

            HighlightOff(x, y);
        }

        private void ClearMatchPieceAt(List<GamePiece> gamePieces)
        {
            foreach(GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    ClearMatchPieceAt(piece.xIndex, piece.yIndex);
                }
            }
        }

        private void ClearBoard()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    ClearMatchPieceAt(i, j);
                }
            }
        }

        List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();

            for (int i = 0; i < height - 1; i++)
            {
                if (_allGamePiece[column, i] == null)
                {
                    for (int j = i + 1; j < height; j++)
                    {
                        if (_allGamePiece[column, j] != null)
                        {
                            _allGamePiece[column, j].Move(column, i, collapseTime * (j - i));
                            _allGamePiece[column, i] = _allGamePiece[column, j];
                            _allGamePiece[column, i].SetCoordinates(column, i);

                            if (!movingPieces.Contains(_allGamePiece[column, i]))
                            {
                                movingPieces.Add(_allGamePiece[column, i]);
                            }

                            _allGamePiece[column, j] = null;

                            break;
                        }
                    }
                }
            }

            return movingPieces;
        }

        List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<int> columnsToCollapse = GetColums(gamePieces);

            foreach(int column in columnsToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList(); ;
            }

            return movingPieces;
        }

        List<int> GetColums(List<GamePiece> gamePieces)
        {
            List<int> columns = new List<int>();
            
            foreach(GamePiece piece in gamePieces)
            {
                if(!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }

            return columns;
        }

        void ClearAndRefillBoard(List<GamePiece> gamePieces)
        {
            StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
        }

        IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
        {
            _playerInputEnabled = false;
            List<GamePiece> matches = gamePieces;

            do
            {
                //clear and collapse
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
                yield return null;

                //refill
                yield return StartCoroutine(RefillRoutine());
                matches = FindAllMatches();

                yield return new WaitForSeconds(0.5f);
            }
            while (matches.Count != 0);
            
            _playerInputEnabled = true;
        }
        
        IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<GamePiece> matches = new List<GamePiece>();

            HighlightMatchPieces(gamePieces);

            yield return new WaitForSeconds(0.5f);

            bool isFinished = false;

            while(!isFinished)
            {
                ClearMatchPieceAt(gamePieces);

                yield return new WaitForSeconds(0.25f);

                movingPieces = CollapseColumn(gamePieces);

                while(!IsCollapsed(movingPieces))
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);

                matches = FindMatchesAt(movingPieces);

                if(matches.Count == 0)
                {
                    isFinished = true;
                    break;
                }
                else
                {
                    yield return StartCoroutine(ClearAndCollapseRoutine(matches));
                }
            }

            yield return null;
        }

        IEnumerator RefillRoutine()
        {
            FillBoard(10, 0.5f);
            yield return null;
        }

        bool IsCollapsed(List<GamePiece> gamePieces)
        {
            foreach(GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    if(piece.transform.position.y - (float)piece.yIndex > 0.001f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }

   
}

