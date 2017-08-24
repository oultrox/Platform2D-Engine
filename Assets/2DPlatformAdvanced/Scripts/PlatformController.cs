using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {
    public LayerMask passengerMask;
    public Vector3 move;

    List<PassengerMovement> passengerMovements;


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

	// Use this for initialization
	public override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateRaycastOrigins();
        Vector3 velocity = move * Time.deltaTime;
        
        CalculatePassengerMovement(velocity);
        MovePassenger(true);
        transform.Translate(velocity);
        MovePassenger(false);
    }

    void MovePassenger(bool beforeMovePlatform)
    {
        for (int i = 0; i < passengerMovements.Count; i++)
        {
            if (passengerMovements[i].isMovingBeforePlatform == beforeMovePlatform)
            {
                passengerMovements[i].transform.GetComponent<Controller2D>().Move(passengerMovements[i].velocity);
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

                if (hit)
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

                if (hit)
                {
                    if (!movedPassangers.Contains(hit.transform))
                    {
                        movedPassangers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                        float pushY = 0;
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

                if (hit)
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
}
