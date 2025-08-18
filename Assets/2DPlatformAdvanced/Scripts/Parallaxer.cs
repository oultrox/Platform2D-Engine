using UnityEngine;

/// <summary>
/// It moves each background at a different speed depending on its Z position relative to the camera, giving a sense
/// of depth. It supports horizontal parallax by default and can optionally enable vertical parallax.
/// </summary>
public class Parallaxer : MonoBehaviour {

    [Header("Parallax References")]
    [SerializeField] private Transform[] backgrounds;   
    [SerializeField] private float smoothing;	          
    [SerializeField] private bool isParallaxYEnabled;   
    private float[] parallaxScales;                    
    private Transform cam;                              
    private Vector3 previousCamPos;                    
    private float parallaxX;
    private float parallaxY;
    private float backgroundTargetPosX;
    private float backgroundTargetPosY;
    private Vector3 backgroundTargetPos;
    
    private void Awake()
    {
        cam = Camera.main.transform;
    }
    
    private void Start()
    {
        previousCamPos = cam.position;
        parallaxScales = new float[backgrounds.Length];
        AssignZLayerPositions();
    }
    
    private void Update()
    {
        ApplyParallax();
    }
    
    private void AssignZLayerPositions()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            // Not multiplied by -1 because background layers are already positioned at negative Z coordinates.
            parallaxScales[i] = backgrounds[i].position.z;
        }
    }
    
    /// <summary>
    /// Executes the perspective algorithm based on the camera and the Z coordinates of the backgrounds.
    /// </summary>
    private void ApplyParallax()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxX = (previousCamPos.x - cam.position.x) * parallaxScales[i];
            parallaxY = (previousCamPos.y - cam.position.y) * parallaxScales[i];

            backgroundTargetPosX = backgrounds[i].position.x + parallaxX;
            backgroundTargetPos.x = backgroundTargetPosX;

            // Assigns parallax Y if enabled via the editor.
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

            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime
            );
        }

        previousCamPos = cam.position;
    }
}
