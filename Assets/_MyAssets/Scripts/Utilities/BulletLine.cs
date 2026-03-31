using UnityEngine;

public class BulletLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float speed = .2f;

    private void OnEnable()
    {
        lineRenderer.widthMultiplier = 1;
    }

    private void Update()
    {
        lineRenderer.widthMultiplier -= Time.deltaTime / speed;

        if (lineRenderer.widthMultiplier <= 0)
            gameObject.SetActive(false);
    }
}
