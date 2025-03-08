using UnityEngine;

namespace AJStudios.Puzzle.Core
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] public int xIndex { get; private set; }
        [SerializeField] public int yIndex { get; private set; }

        Board _board;

        public void Init(int x, int y, Board board)
        {
            xIndex = x;
            yIndex = y;

            _board = board;
        }

        private void OnMouseDown()
        {
            if(_board != null)
            {
                _board.ClickTile(this);
            }
        }

        private void OnMouseEnter()
        {
            if(_board != null)
            {
                _board.DragToTile(this);
            }
        }

        private void OnMouseUp()
        {
            if (_board != null)
            {
                _board.ReleaseTile();
            }
        }

    }
}

