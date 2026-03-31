using UnityEngine;

public class BackgroundHelicopterManager : MonoBehaviour
{
    public float speed = 10f;
    public float verticalSpeed = 2f;
    public float forwardLimit = 50f;
    public float backwardLimit = -50f;
    public float upperLimit = 20f;
    public float lowerLimit = 5f;

    private bool movingForward = true;
    private bool movingUp = true;

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * step);

        if (movingForward && transform.position.z >= forwardLimit)
        {
            transform.Rotate(0, 180, 0);
            movingForward = false;
        }
        else if (!movingForward && transform.position.z <= backwardLimit)
        {
            transform.Rotate(0, 180, 0);
            movingForward = true;
        }

        float verticalStep = verticalSpeed * Time.deltaTime;
        if (movingUp)
            transform.Translate(Vector3.up * verticalStep);
        else
            transform.Translate(Vector3.down * verticalStep);

        if (movingUp && transform.position.y >= upperLimit)
            movingUp = false;
        else if (!movingUp && transform.position.y <= lowerLimit)
            movingUp = true;
    }
}
