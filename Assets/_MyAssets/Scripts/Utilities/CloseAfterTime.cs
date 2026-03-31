using UnityEngine;

namespace Jacob.Utilities
{
    public class CloseAfterTime : MonoBehaviour
    {
        public float lifeTime = 2;
        float timer;

        private void OnEnable()
        {
            timer = lifeTime;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
                gameObject.SetActive(false);
        }
    }
}
