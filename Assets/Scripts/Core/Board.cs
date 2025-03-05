using UnityEngine;

namespace AJStudios.Puzzle.Core
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private int borderSize;

        [SerializeField] private GameObject tilePrefab;

        Tile[,] _allTiles;

        private void Start()
        {
            _allTiles = new Tile[width, height];
            SetupTiles();
            SetupCamera();
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
    }
}

