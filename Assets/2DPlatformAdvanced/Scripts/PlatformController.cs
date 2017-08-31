using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool isStandingOnPlaform;
        public bool isMovingBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _isStandingOnPlaform, bool _isMovingBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            isStandingOnPlaform = _isStandingOnPlaform;
            isMovingBeforePlatform = _isMovingBeforePlatform;
        }
    }

    List<PassengerMovement> passengerMovements;
    Dictionary<Transform, Controller2D> dictionaryPassengers = new Dictionary<Transform, Controller2D>();

    public LayerMask passengerMask;
    public Vector3[] localWaypoints;
    private Vector3[] globalWayPointsPosition;

    public float speed;
    int fromWayPointIndex;
    float percentBetweenWaypoints; // Entre 0 y 1.
    public bool isCyclic;
    public float waitTime;
    private float nextMoveTime;

    [Range(0,2)]
    public float easeAmount;
	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        globalWayPointsPosition = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWayPointsPosition[i] = localWaypoints[i] + transform.position;
        }
	}
	
	// Update is called once per frame
	void Update () {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        
        CalculatePassengerMovement(velocity);
        MovePassenger(true);
        transform.Translate(velocity);
        MovePassenger(false);
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
    }
    Vector3 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }
        fromWayPointIndex %= globalWayPointsPosition.Length;
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

            //Si no es ciclico, invertir los waypoints para devolverse.
            if (!isCyclic)
            {
                if (fromWayPointIndex >= globalWayPointsPosition.Length - 1)
                {
                    fromWayPointIndex = 0;
                    System.Array.Reverse(globalWayPointsPosition);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - transform.position;
    }

    void MovePassenger(bool beforeMovePlatform)
    {
        for (int i = 0; i < passengerMovements.Count; i++)
        {
            if (!dictionaryPassengers.ContainsKey(passengerMovements[i].transform))
            {
                dictionaryPassengers.Add(passengerMovements[i].transform, passengerMovements[i].transform.GetComponent<Controller2D>());
            }
            if (passengerMovements[i].isMovingBeforePlatform == beforeMovePlatform)
            {
                dictionaryPassengers[passengerMovements[i].transform].Move(passengerMovements[i].velocity, passengerMovements[i].isStandingOnPlaform);
            }
        }
    }
    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassangers = new HashSet<Transform>();
        passengerMovements = new List<PassengerMovement>();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Movimiento vertical
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        movedPassangers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;

                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Horizontal movement platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        movedPassangers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                        float pushY = -SKIN_WIDTH;

                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Si hay un pasajero encima de una plataforma en movimiento (horizontal o vertical hacia abajo)
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = SKIN_WIDTH * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        movedPassangers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
            
        }
    }

    //Metodo para visualizar en el editor los waypoints locales y al ejecutar el juego
    //los globales de las plataformas en movimiento.
    private void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;
            Vector3 globalWayPointPos;
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                globalWayPointPos = (Application.isPlaying ? globalWayPointsPosition[i] : localWaypoints[i] + transform.position);
                Gizmos.DrawLine(globalWayPointPos - Vector3.up * size, globalWayPointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWayPointPos - Vector3.left * size, globalWayPointPos + Vector3.left * size);
            }
        }
    }
}
