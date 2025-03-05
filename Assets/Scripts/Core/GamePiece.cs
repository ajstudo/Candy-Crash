using System.Collections;
using UnityEngine;

namespace AJStudios.Puzzle.Gameplay
{
    public class GamePiece : MonoBehaviour
    {
        [SerializeField] private int xIndex;
        [SerializeField] private int yIndex;

        private bool _isMoving;

        public void SetCoordinates(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.D)) // moves right
            {
                Move((int)transform.position.x + 1, (int)transform.position.y, 1f);
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                Move((int)transform.position.x - 1, (int)transform.position.y, 1f);
            }
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
                if(Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    isReachedDestination = true;
                    transform.position = destination;
                    SetCoordinates((int)destination.x, (int)destination.y);
                    break;
                }

                elaspsedTime += Time.deltaTime;

                float t = Mathf.Clamp(elaspsedTime / moveTime , 0f, 1f);

                transform.position = Vector3.Lerp(startPosition, destination, t);

                yield return null;
            }

            _isMoving = false;
        }
    }
}

