using UnityEngine;

public static class GameReferences
{
    public static LayerMask ignoreForShooting;
    public static LayerMask controllersLayer;
    public static float damage;

    static ObjectPooler _objectPooler;
    static readonly Collider[] nearbyBuffer = new Collider[32];

    public static ObjectPooler objectPooler
    {
        get
        {
            if (_objectPooler == null)
            {
                _objectPooler = Resources.Load("ObjectPooler") as ObjectPooler;
                _objectPooler.Init();
            }
            return _objectPooler;
        }
    }

    public static void RaycastShoot(Transform mTransform, WeaponHook weaponHook)
    {
        if (objectPooler == null)
            return;

        Vector3 origin = Random.insideUnitCircle * weaponHook.baseItem.weaponSpread;
        origin = mTransform.TransformPoint(origin);
        origin.y += 1.3f;
        origin += mTransform.forward;

        Vector3 endPosition = origin + mTransform.forward * 100;

        if (Physics.Raycast(origin, mTransform.forward, out RaycastHit hit, 100, ignoreForShooting))
        {
            IShootable shootable = hit.transform.GetComponentInParent<IShootable>();

            if (shootable != null)
            {
                GameObject fx = objectPooler.GetObject(shootable.GetHitFx());
                if (fx != null)
                {
                    fx.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
                    fx.SetActive(true);
                    shootable.OnHit(damage);
                }
            }
            else
            {
                GameObject fx = objectPooler.GetObject("default");
                if (fx != null)
                {
                    fx.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
                    fx.SetActive(true);
                }
            }

            endPosition = hit.point;
        }

        GameObject bulletLine = objectPooler.GetObject("bulletLine");
        if (bulletLine != null)
        {
            LineRenderer line = bulletLine.GetComponent<LineRenderer>();
            line.SetPosition(0, weaponHook.bulletEmmiter.position);
            line.SetPosition(1, endPosition);
            bulletLine.SetActive(true);
        }
    }

    public static void UpdateLastKnownPositionOfCloseby(Vector3 targetPosition, float radius)
    {
        int count = Physics.OverlapSphereNonAlloc(targetPosition, radius, nearbyBuffer, controllersLayer);

        for (int i = 0; i < count; i++)
        {
            AIController ai = nearbyBuffer[i].transform.GetComponentInParent<AIController>();
            if (ai != null)
                ai.UpdateLastKnowPosition(targetPosition);
        }
    }
}
