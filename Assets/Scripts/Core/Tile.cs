using UnityEngine;

namespace AJStudios.Puzzle.Core
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private int xIndex;
        [SerializeField] private int yIndex;

        Board _board;

        public void Init(int x, int y, Board board)
        {
            xIndex = x;
            yIndex = y;

            _board = board;
        }

    }
}

