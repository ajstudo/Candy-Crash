using UnityEngine;

namespace AJStudios.Puzzle.Core
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;

        [SerializeField] private GameObject tilePrefab;

        Tile[,] _allTiles;

        private void Start()
        {
            _allTiles = new Tile[width, height];
            SetupTiles();
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
                }
            }
        }
    }
}

