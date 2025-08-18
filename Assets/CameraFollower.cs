using UnityEngine;

/// <summary>
/// Just follows the target transform.
/// </summary>
public class CameraFollower : MonoBehaviour
{
    public static CameraFollower instance;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothness = 0.1f;

    private Transform cameraTransform;
    private bool isActive = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        cameraTransform = this.transform;
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        if (isActive == false)
        {
            return;
        }

        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 targetPos = target.position;
        targetPos.z = cameraTransform.position.z;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, smoothness * Time.deltaTime);
    }
}
