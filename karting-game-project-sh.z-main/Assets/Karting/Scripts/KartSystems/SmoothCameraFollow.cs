using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; //Follow the target, which is the empty game object
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void Update()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Rotation hold is controlled by Oculus
    }
}
