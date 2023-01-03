using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public static CameraFollower instance;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothness = 0.1f;

    private Transform cameraTransform;
    private bool isActive = true;

    private void Awake()
    {
        //Singleton creation
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

    // Update is called once per frame
    void Update()
    {
        if (isActive == false)
        {
            return;
        }
        Vector3 targetPos = target.position;
        targetPos.z = cameraTransform.position.z;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, smoothness * Time.deltaTime);
    }

    public void Activate(bool _isActive)
    {
        isActive = _isActive;
        if (!isActive)
        {
            this.transform.position = new Vector3(0, 0, transform.position.z);
        }
    }

    #region Properties
    public bool IsActive
    {
        get
        {
            return isActive;
        }

        set
        {
            isActive = value;
        }
    }

    public Transform Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
        }
    }
    #endregion
}
