using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -10f);
    public float smoothTime = 0.2f;
    public float xFollowStrength = 0.65f;
    public float minY = -12f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x * xFollowStrength,
            Mathf.Max(minY, target.position.y + offset.y),
            offset.z);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
