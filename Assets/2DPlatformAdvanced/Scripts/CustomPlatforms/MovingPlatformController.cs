using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : RaycastMotor2D {

    private struct PassengerState
    {
        public Transform transform;
        public Vector3 velocity;
        public bool isStandingOnPlatform;
        public bool isMovingBeforePlatform;

        public PassengerState(Transform _transform, Vector3 _velocity, bool _isStandingOnPlatform, bool _isMovingBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            isStandingOnPlatform = _isStandingOnPlatform;
            isMovingBeforePlatform = _isMovingBeforePlatform;
        }
    }

    // Editable fields
    public LayerMask passengerMask;
    public Vector3[] localWaypoints;
    [Range(0, 2)]
    public float easeAmount;
    public float speed;
    public float waitTime;
    public bool isCyclic;
    
    private List<PassengerState> passengers;
    private Dictionary<Transform, PlatformMotor2D> dictionaryPassengers;
    private HashSet<Transform> movedPassengers;
    private Vector3[] globalWayPointsPosition;
    private int fromWayPointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;

    public override void Start()
    {
        SetLWayPoints();
    }
    
    void Update()
    {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);
        MovePassenger(true);
        transform.Translate(velocity);
        MovePassenger(false);
    }
    
    private void SetLWayPoints()
    {
        base.Start();
        dictionaryPassengers = new Dictionary<Transform, PlatformMotor2D>();
        globalWayPointsPosition = new Vector3[localWaypoints.Length];
        passengers = new List<PassengerState>();
        movedPassengers = new HashSet<Transform>();

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
        {
            return Vector3.zero;
        }

        fromWayPointIndex = fromWayPointIndex % globalWayPointsPosition.Length;
        int toWayPointIndex = (fromWayPointIndex + 1) % globalWayPointsPosition.Length;

        float distanceBetweenWayPoints = Vector3.Distance(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWayPoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

        float easedPercentBetweenWayPoints = Ease(percentBetweenWaypoints);
        Vector3 newPos = Vector3.Lerp(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex], easedPercentBetweenWayPoints);

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

    private void MovePassenger(bool beforeMovePlatform)
    {
        for (int i = 0; i < passengers.Count; i++)
        {
            if (!dictionaryPassengers.ContainsKey(passengers[i].transform))
            {
                dictionaryPassengers.Add(passengers[i].transform, passengers[i].transform.GetComponent<PlatformMotor2D>());
            }

            if (passengers[i].isMovingBeforePlatform == beforeMovePlatform)
            {
                dictionaryPassengers[passengers[i].transform].Move(passengers[i].velocity, passengers[i].isStandingOnPlatform);
            }
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

        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

            if (hit && !movedPassengers.Contains(hit.transform))
            {
                float pushX = (directionY == 1) ? velocity.x : 0;
                float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;

                movedPassengers.Add(hit.transform);
                passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
            }
        }
    }

    private void CheckHorizontalMovement(Vector3 velocity, float directionX)
    {
        if (velocity.x == 0) return;

        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

            if (hit && !movedPassengers.Contains(hit.transform))
            {
                float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                float pushY = -SKIN_WIDTH;

                movedPassengers.Add(hit.transform);
                passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), false, true));
            }
        }
    }

    private void PlayerMovementOnPlatform(Vector3 velocity, float directionY)
    {
        if (!(directionY == -1 || (velocity.y == 0 && velocity.x != 0))) return;

        float rayLength = SKIN_WIDTH * 2;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigin.topLeft + Vector2.right * (verticalRaySpacing * i);
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
        ShowWayPoints();
    }

    private void ShowWayPoints()
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
