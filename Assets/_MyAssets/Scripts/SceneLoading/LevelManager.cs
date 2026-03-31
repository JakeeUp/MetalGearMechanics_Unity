using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Transform playerSpawnposition;
    public Collider cameraConfinerCollider;

    public static LevelManager singleton;

    private void Awake()
    {
        singleton = this;
    }
}
