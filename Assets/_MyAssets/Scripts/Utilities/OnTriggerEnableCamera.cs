using UnityEngine;

public class OnTriggerEnableCamera : MonoBehaviour
{
    public GameObject targetCamera;

    private void Start()
    {
        targetCamera.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Controller>() != null)
            targetCamera.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Controller>() != null)
            targetCamera.SetActive(false);
    }
}
