using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : RaycastMotor2D {

    //Struct que contiene los estados de sus pasajeros.
    private struct PassengerState
    {
        public Transform transform;
        public Vector3 velocity;
        public bool isStandingOnPlaform;
        public bool isMovingBeforePlatform;

        //Constructor
        public PassengerState(Transform _transform, Vector3 _velocity, bool _isStandingOnPlaform, bool _isMovingBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            isStandingOnPlaform = _isStandingOnPlaform;
            isMovingBeforePlatform = _isMovingBeforePlatform;
        }
    }

    //Variables editables 
    public LayerMask passengerMask;             //El layermask donde debe registrar los pasajeros.
    public Vector3[] localWaypoints;            //Las posiciones de los puntos por donde se moverá la plataforma.
    [Range(0, 2)]
    public float easeAmount;                    //Cantidad de la suavidad que tendrá, varía entre 0 y 2.
    public float speed;                         //Velocidad de la plataforma.
    public float waitTime;                      //Cantidad de tiempo a esperar entre movimientos de la plataforma.
    public bool isCyclic;                       //Si es cíclico o no.

    //Privates
    private List<PassengerState> passengers;    
    private Dictionary<Transform, PlatformMotor2D> dictionaryPassengers;
    private HashSet<Transform> movedPassangers;
    private Vector3[] globalWayPointsPosition;  // Convierte las posiciones locales que se ven en el editor en las posiciones globales para el mov.
    private int fromWayPointIndex;              // Controla la posicion actual de la plataforma en terminos de waypoints.
    private float percentBetweenWaypoints;      // Entre 0 y 1.
    private float nextMoveTime;                 // Almacena cuando es el siguiente movimiento de cada iteracion.
    
    //----------Metodos API----------
    //Inicialización
    public override void Start ()
    {
        base.Start();
        dictionaryPassengers = new Dictionary<Transform, PlatformMotor2D>();
        globalWayPointsPosition = new Vector3[localWaypoints.Length];
        passengers = new List<PassengerState>();
        movedPassangers = new HashSet<Transform>();

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

    //----------Metodos custom----------
    //Realiza el algoritmo para la suavidad del movimiento.
    private float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
    }

    //Maneja el movimiento de la plataforma.
    private Vector3 CalculatePlatformMovement()
    {
        //Si aun no es tiempo de moverse, devolver 0.
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWayPointIndex = fromWayPointIndex % globalWayPointsPosition.Length;
        int toWayPointIndex = (fromWayPointIndex + 1) % globalWayPointsPosition.Length;

        float distanceBetweenWayPoints = Vector3.Distance(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWayPoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

        //Aplica la suavidad(El calculo) al porcentaje creado entre los waypoints.
        float easedPercentBetweenWayPoints = Ease(percentBetweenWaypoints);

        //La nueva posicion para llegar desde el punto actual hacia el otro con el porcentaje de movimiento entre los puntos.
        Vector3 newPos = Vector3.Lerp(globalWayPointsPosition[fromWayPointIndex], globalWayPointsPosition[toWayPointIndex], easedPercentBetweenWayPoints);
        
        //Si el porcentaje llego a 1 significa que se debe devolver, y si es ciclico va hacia el punto de inicio.
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

        //Retorna la nueva posición.
        return newPos - transform.position;
    }

    //Mover a los pasajeros en base a el diccionario de los pasajeros que contiene la plataforma.
    private void MovePassenger(bool beforeMovePlatform)
    {
        //Para optimizar la llamada de los calculos se utiliza un diccionario que agrega a los pasajeros que no se encuentren y si los encuentra
        //ejecuta directamente el componente de su controlador2D y lo mueve.
        for (int i = 0; i < passengers.Count; i++)
        {
            if (!dictionaryPassengers.ContainsKey(passengers[i].transform))
            {
                dictionaryPassengers.Add(passengers[i].transform, passengers[i].transform.GetComponent<PlatformMotor2D>());
            }
            if (passengers[i].isMovingBeforePlatform == beforeMovePlatform)
            {
                dictionaryPassengers[passengers[i].transform].Move(passengers[i].velocity, passengers[i].isStandingOnPlaform);
            }
        }
    }

    //Calcula el movimiento de los pasajeros.
    private void CalculatePassengerMovement(Vector3 velocity)
    {
        movedPassangers.Clear();
        passengers.Clear();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        CheckVerticalMovement(velocity, directionY);
        CheckHorizontalMovement(velocity, directionX);
        PlayerMovementOnPlatform(velocity, directionY);
    }

    private void CheckVerticalMovement(Vector3 velocity, float directionY)
    {
        bool isMovingVertically = velocity.y != 0;
        if (!isMovingVertically)
        {
            return;
        }

        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        //Por cada raycast pregunta si toca un pasajero, lo que nos permite saber que tiene pasajeros la plataforma.
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

            if (hit && hit.distance != 0)
            {
                //Si el pasajero que no está almacenado, lo guarda.
                if (movedPassangers.Contains(hit.transform))
                {
                    return;
                }

                float pushX = (directionY == 1) ? velocity.x : 0;
                float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;

                movedPassangers.Add(hit.transform);
                passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
            } 
        }
    }

    private void CheckHorizontalMovement(Vector3 velocity, float directionX)
    {
        bool isMovingHorizontally = velocity.x != 0;
        if (!isMovingHorizontally)
        {
            return;
        }

        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

            if (hit && hit.distance != 0)
            {
                //Si el pasajero que no está almacenado, lo guarda.
                if (movedPassangers.Contains(hit.transform))
                {
                    return;
                }

                float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                float pushY = -SKIN_WIDTH;

                movedPassangers.Add(hit.transform);
                passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), false, true));
            }
        }
        
    }

    //Chequeo Si hay un pasajero encima de una plataforma en movimiento (horizontal o vertical hacia abajo)
    private void PlayerMovementOnPlatform(Vector3 velocity, float directionY)
    {
        bool isPassengerMoving = directionY == -1 || velocity.y == 0 && velocity.x != 0;
        if (!isPassengerMoving)
        {
            return;
        }

        float rayLength = SKIN_WIDTH * 2;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigin.topLeft + Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

            if (hit && hit.distance != 0)
            {
                //Si el pasajero que no está almacenado, lo guarda.
                if (movedPassangers.Contains(hit.transform))
                {
                    return;
                }

                float pushX = velocity.x;
                float pushY = velocity.y;

                movedPassangers.Add(hit.transform);
                passengers.Add(new PassengerState(hit.transform, new Vector3(pushX, pushY), true, false));
                
            }
        }
    }


    //----------Metodos para el editor----------
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
