using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovingScript : MonoBehaviour
{
    public float speed = 5.0f;

    private void Update()
    {
        Vector3 newPos = transform.position + Vector3.right * speed * Time.deltaTime;

        transform.position = newPos;
    }
}
