using UnityEngine;

public interface IShootable
{
    float currentHealth { get; }
    void OnHit(float damage);
    string GetHitFx();
}

public interface IPointOfInterest
{
    bool OnDetect(AIController aIController);
    Transform GetTransform();
}
