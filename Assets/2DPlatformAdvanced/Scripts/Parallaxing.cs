using UnityEngine;

public class Parallaxing : MonoBehaviour {

    [SerializeField] private Transform[] backgrounds;   //Arreglo que almacena los fondos que van a ser 'parallaxeados'.
    [SerializeField] private float smoothing;	          //El nivel de intensidad en la que se hará el parallax.
    [SerializeField] private bool isParallaxYEnabled;   //Para habilitar el parallaxeo en coordenadas Y (opcional).
    private float[] parallaxScales;                     //La proporcion del movimiento del a camara para mover los fondos.
    private Transform cam;                              //Referencia a la Main camera
    private Vector3 previousCamPos;                     //La posición de la camara en el frame anterior.

    //----------Metodos API-------------
    //Referencias
    private void Awake()
    {
        cam = Camera.main.transform;
    }

    //Inicialización
    private void Start()
    {
        previousCamPos = cam.position;
        parallaxScales = new float[backgrounds.Length];

        // se asigna las escalas correspondientes.
        for (int i = 0; i < backgrounds.Length; i++)
        {
            //No se multiplica por -1 debido a que el posicionamiento de los fondos está en coordenadas negativas z de por sí.
            parallaxScales[i] = backgrounds[i].position.z;

        }
    }

    //Cached variables utilizadas en Update()
    private float parallaxX;
    private float parallaxY;
    private float backgroundTargetPosX;
    private float backgroundTargetPosY;
    private Vector3 backgroundTargetPos;

    //Ejecuta el algoritmo de perspectiva en base a la camara y las coordenadas z de los fondos.
    private void Update()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxX = (previousCamPos.x - cam.position.x) * parallaxScales[i];
            parallaxY = (previousCamPos.y - cam.position.y) * parallaxScales[i];

            backgroundTargetPosX = backgrounds[i].position.x + parallaxX;
            backgroundTargetPos.x = backgroundTargetPosX;

            //Asigna el paralaje Y en caso de que esté activo vía el editor.
            if (isParallaxYEnabled)
            {
                backgroundTargetPosY = backgrounds[i].position.y + parallaxY;
                backgroundTargetPos.y = backgroundTargetPosY;
            }
            else
            {
                backgroundTargetPos.y = backgrounds[i].position.y;
            }
            backgroundTargetPos.z = backgrounds[i].position.z;

            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);

        }

        previousCamPos = cam.position;
    }
}
