using AJStudios.Puzzle.Gameplay;
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

        Tile[,] _allTiles;
        GamePiece[,] allGamePiece;

        private void Start()
        {
            _allTiles = new Tile[width, height];
            SetupTiles();
            SetupCamera();
            FillRandom();
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

        private void PlaceGamePiece(GamePiece gamePiece, int x,int y)
        {
            if (gamePiece == null) return;

            gamePiece.transform.position = new Vector3(x,y,0);
            gamePiece.transform.rotation = Quaternion.identity;

            gamePiece.SetCoordinates(x,y);

        }

        private void FillRandom()
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    GameObject gamePiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity);

                    if(gamePiece != null)
                    {
                        PlaceGamePiece(gamePiece.GetComponent<GamePiece>(), i, j);
                    }
                }
            }
        }
    }
}

