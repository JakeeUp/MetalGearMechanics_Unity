using UnityEngine;

namespace Jacob.Utilities
{
    public class OrientTowardsCamera : MonoBehaviour
    {
        Transform camTransform;
        Transform mTransform;

        private void Start()
        {
            if (Camera.main != null)
                camTransform = Camera.main.transform;
            mTransform = transform;
        }

        private void Update()
        {
            if (camTransform == null)
            {
                enabled = false;
                return;
            }

            Vector3 eulerAngles = camTransform.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            mTransform.eulerAngles = eulerAngles;
        }
    }
}
