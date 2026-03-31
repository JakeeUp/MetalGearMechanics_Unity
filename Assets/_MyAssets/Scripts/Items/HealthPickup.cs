using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healthIncrease = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GameController"))
            return;

        Controller controller = other.GetComponent<Controller>();
        if (controller != null && controller.currentHealth < controller.maxHealth)
        {
            controller.currentHealth += healthIncrease;
            controller.currentHealth = Mathf.Min(controller.currentHealth, controller.maxHealth);
            Destroy(gameObject);
        }
    }
}
