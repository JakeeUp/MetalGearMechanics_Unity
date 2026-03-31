using UnityEngine;

public class ItemRotation : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float bounceSpeed = 2f;
    public float bounceHeight = 0.5f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
