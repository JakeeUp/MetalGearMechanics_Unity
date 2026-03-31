using UnityEngine;

public class billboard : MonoBehaviour
{
    public static Transform cam;
    public Vector3 freeRotation = Vector3.one;
    Vector3 eangles = Vector3.zero;

    void Start()
    {
        if (cam == null)
        {
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCam != null)
                cam = mainCam.transform;
        }
    }

    private void LateUpdate()
    {
        if (cam == null)
            return;

        transform.LookAt(cam);
        transform.Rotate(0, 180, 0);

        eangles = transform.eulerAngles;
        eangles.x *= freeRotation.x;
        eangles.y *= freeRotation.y;
        eangles.z *= freeRotation.z;
        transform.eulerAngles = eangles;
    }
}
