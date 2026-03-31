using UnityEngine;

namespace Jacob.Utilities
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform targetTransform;
        Transform mTransform;

        private void Start()
        {
            mTransform = transform;
        }

        private void Update()
        {
            if (targetTransform == null)
            {
                enabled = false;
                return;
            }

            mTransform.position = targetTransform.position;
        }
    }
}
