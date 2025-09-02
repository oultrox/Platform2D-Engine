using System.Collections.Generic;
using UnityEngine;

namespace _2DPlatformAdvanced.Scripts.CustomPlatforms
{
    /// <summary>
    /// In charge of movable platforms using the standalone RaycastMotor2D.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class MovingPlatformController : MonoBehaviour
    {
        private struct PassengerState
        {
            public Transform Transform;
            public Vector3 Velocity;
            public bool IsStandingOnPlatform;
            public bool IsMovingBeforePlatform;

            public PassengerState(Transform t, Vector3 v, bool standing, bool movingBefore)
            {
                Transform = t;
                Velocity = v;
                IsStandingOnPlatform = standing;
                IsMovingBeforePlatform = movingBefore;
            }
        }

        [Header("Platform Settings")]
        public LayerMask passengerMask;
        public Vector3[] localWaypoints;
        [Range(0, 2)] public float easeAmount;
        public float speed;
        public float waitTime;
        public bool isCyclic;

        private RaycastMotor2D _motor;
        private BoxCollider2D _collider;

        private List<PassengerState> passengers;
        private Dictionary<Transform, PlatformMotor2D> dictionaryPassengers;
        private HashSet<Transform> movedPassengers;

        private Vector3[] globalWayPointsPosition;
        private int fromWayPointIndex;
        private float percentBetweenWaypoints;
        private float nextMoveTime;

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _motor = new RaycastMotor2D(_collider);
            SetWayPoints();
        }

        void Update()
        {
            _motor.UpdateRaycastOrigins();
            Vector3 velocity = CalculatePlatformMovement();
            CalculatePassengerMovement(velocity);
            DisplaceWithPassengers(velocity);
        }

        private void SetWayPoints()
        {
            dictionaryPassengers = new Dictionary<Transform, PlatformMotor2D>();
            passengers = new List<PassengerState>();
            movedPassengers = new HashSet<Transform>();

            globalWayPointsPosition = new Vector3[localWaypoints.Length];
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                globalWayPointsPosition[i] = localWaypoints[i] + transform.position;
            }
        }

        private float Ease(float x)
        {
            float a = easeAmount + 1;
            return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
        }

        private Vector3 CalculatePlatformMovement()
        {
            if (Time.time < nextMoveTime)
                return Vector3.zero;

            fromWayPointIndex %= globalWayPointsPosition.Length;
            int toWayPointIndex = (fromWayPointIndex + 1) % globalWayPointsPosition.Length;

            float distance = Vector3.Distance(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex]);
            percentBetweenWaypoints += Time.deltaTime * speed / distance;
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

            Vector3 newPos = Vector3.Lerp(
                globalWayPointsPosition[fromWayPointIndex],
                globalWayPointsPosition[toWayPointIndex],
                Ease(percentBetweenWaypoints)
            );

            if (percentBetweenWaypoints >= 1)
            {
                percentBetweenWaypoints = 0;
                fromWayPointIndex++;

                if (!isCyclic && fromWayPointIndex >= globalWayPointsPosition.Length - 1)
                {
                    fromWayPointIndex = 0;
                    System.Array.Reverse(globalWayPointsPosition);
                }

                nextMoveTime = Time.time + waitTime;
            }

            return newPos - transform.position;
        }

        private void DisplaceWithPassengers(Vector3 velocity)
        {
            MovePassenger(true);
            transform.Translate(velocity);
            MovePassenger(false);
        }

        private void MovePassenger(bool beforeMovePlatform)
        {
            foreach (var passenger in passengers)
            {
                if (!dictionaryPassengers.ContainsKey(passenger.Transform))
                    dictionaryPassengers.Add(passenger.Transform, passenger.Transform.GetComponent<PlayerMovementController>().PlayerMotor);

                if (passenger.IsMovingBeforePlatform == beforeMovePlatform)
                    dictionaryPassengers[passenger.Transform].Move(passenger.Velocity, passenger.IsStandingOnPlatform);
            }
        }

        private void CalculatePassengerMovement(Vector3 velocity)
        {
            movedPassengers.Clear();
            passengers.Clear();

            float directionX = Mathf.Sign(velocity.x);
            float directionY = Mathf.Sign(velocity.y);

            CheckVerticalMovement(velocity, directionY);
            CheckHorizontalMovement(velocity, directionX);
            PlayerMovementOnPlatform(velocity, directionY);
        }

        private void CheckVerticalMovement(Vector3 velocity, float directionY)
        {
            if (velocity.y == 0) return;

            float rayLength = Mathf.Abs(velocity.y) + RaycastMotor2D.SKIN_WIDTH;

            for (int i = 0; i < _motor.VerticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? _motor.GetBottomLeft() : _motor.GetTopLeft();
                rayOrigin += Vector2.right * (_motor.VerticalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit && !movedPassengers.Contains(hit.transform))
                {
                    float pushX = (directionY == 1) ? velocity.x : 0;
                    float pushY = velocity.y - (hit.distance - RaycastMotor2D.SKIN_WIDTH) * directionY;

                    movedPassengers.Add(hit.transform);
                    passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                }
            }
        }

        private void CheckHorizontalMovement(Vector3 velocity, float directionX)
        {
            if (velocity.x == 0) return;

            float rayLength = Mathf.Abs(velocity.x) + RaycastMotor2D.SKIN_WIDTH;

            for (int i = 0; i < _motor.HorizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? _motor.GetBottomLeft() : _motor.GetBottomRight();
                rayOrigin += Vector2.up * (_motor.HorizontalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                if (hit && !movedPassengers.Contains(hit.transform))
                {
                    float pushX = velocity.x - (hit.distance - RaycastMotor2D.SKIN_WIDTH) * directionX;
                    float pushY = -RaycastMotor2D.SKIN_WIDTH;

                    movedPassengers.Add(hit.transform);
                    passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), false, true));
                }
            }
        }

        private void PlayerMovementOnPlatform(Vector3 velocity, float directionY)
        {
            if (!(directionY == -1 || (velocity.y == 0 && velocity.x != 0))) return;

            float rayLength = RaycastMotor2D.SKIN_WIDTH * 2;

            for (int i = 0; i < _motor.VerticalRayCount; i++)
            {
                Vector2 rayOrigin = _motor.GetTopLeft() + Vector2.right * (_motor.VerticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && !movedPassengers.Contains(hit.transform))
                {
                    movedPassengers.Add(hit.transform);
                    passengers.Add(new PassengerState(hit.transform, velocity, true, false));
                }
            }
        }

        #region Debugging
        private void OnDrawGizmos()
        {
            if (localWaypoints == null) return;

            Gizmos.color = Color.red;
            float size = 0.3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWayPointPos = (Application.isPlaying ? globalWayPointsPosition[i] : localWaypoints[i] + transform.position);
                Gizmos.DrawLine(globalWayPointPos - Vector3.up * size, globalWayPointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWayPointPos - Vector3.left * size, globalWayPointPos + Vector3.left * size);
            }
        }
        #endregion
    }
}
