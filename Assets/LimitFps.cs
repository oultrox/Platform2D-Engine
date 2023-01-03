using UnityEngine;

public class LimitFps : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 10;
    }
}