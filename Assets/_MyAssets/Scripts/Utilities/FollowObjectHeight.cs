using UnityEngine;

public class FollowObjectHeight : MonoBehaviour
{
    public Transform targetTransform;
    Transform mTransform;
    Transform parent;
    Controller controller;

    void Start()
    {
        controller = GetComponent<Controller>();
        mTransform = transform;

        if (controller != null)
            parent = controller.transform.parent;
    }

    void Update()
    {
        if (parent == null || mTransform == null || targetTransform == null)
            return;

        Vector3 r = parent.InverseTransformPoint(targetTransform.position);
        r.x = mTransform.localPosition.x;
        r.z = mTransform.localPosition.z;
        mTransform.localPosition = r;
    }
}
