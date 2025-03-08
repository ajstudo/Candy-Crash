using AJStudios.Puzzle.Core;
using System.Collections;
using UnityEngine;

namespace AJStudios.Puzzle.Gameplay
{
    public enum InterpolationType
    {
        Linear,
        EaseIn,
        EaseOut,
        Smooth,
        Smoother
    }
    public class GamePiece : MonoBehaviour
    {
        [SerializeField] public int xIndex { get; private set; }
        [SerializeField] public int yIndex { get; private set; }

        [SerializeField] private InterpolationType interpolationType = InterpolationType.Smoother;

        private bool _isMoving;

        private Board _board;
        public void Init( Board board)
        {
            _board = board;
        }

        public void SetCoordinates(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        private void Update()
        {
            /*if(Input.GetKeyDown(KeyCode.D)) // moves right
            {
                Move((int)transform.position.x + 1, (int)transform.position.y, 1f);
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                Move((int)transform.position.x - 1, (int)transform.position.y, 1f);
            }*/
        }

        public void Move(int destX, int destY, float moveTime)
        {
            if(!_isMoving)
            {
                StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), moveTime));
            }
        }

        private IEnumerator MoveRoutine(Vector3 destination, float moveTime)
        {
            Vector3 startPosition = transform.position;

            bool isReachedDestination = false;

            float elaspsedTime = 0f;

            _isMoving = true;

            while(!isReachedDestination)
            {
                if (Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    isReachedDestination = true;

                    if(_board != null)
                    {
                        _board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                    }

                    break;
                }

                elaspsedTime += Time.deltaTime;

                float t = Mathf.Clamp(elaspsedTime / moveTime, 0f, 1f);
                t = GetSmoothTime(t);

                transform.position = Vector3.Lerp(startPosition, destination, t);

                yield return null;
            }

            _isMoving = false;
        }

        private float GetSmoothTime(float t)
        {
            switch (interpolationType)
            {
                case InterpolationType.Linear:
                    break;
                case InterpolationType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.Smooth:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterpolationType.Smoother:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;

            }

            return t;
        }
    }
}

