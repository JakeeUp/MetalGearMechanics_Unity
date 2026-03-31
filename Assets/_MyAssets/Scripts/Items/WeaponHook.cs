using UnityEngine;

public class WeaponHook : MonoBehaviour
{
    // ============================
    // Serialized Fields
    // ============================

    public int currentAmmo;
    public int allAmmo = 40;
    [HideInInspector] public WeaponItem baseItem;
    public Transform bulletEmmiter;
    public AudioSource gunSoundSource;
    public AudioClip[] gunSound;

    // ============================
    // State
    // ============================

    ParticleSystem[] particles;

    // ============================
    // Public API
    // ============================

    public void Init(WeaponItem weaponItem)
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        baseItem = weaponItem;
        currentAmmo = baseItem.magazineAmmo;
    }

    public void Shoot()
    {
        if (gunSound != null && gunSound.Length > 0 && gunSoundSource != null)
        {
            gunSoundSource.clip = gunSound[Random.Range(0, gunSound.Length)];
            gunSoundSource.Play();
        }

        if (particles != null)
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i].Play();
        }

        currentAmmo--;
        GameReferences.UpdateLastKnownPositionOfCloseby(transform.position, 50);
    }

    public void Reload()
    {
        if (allAmmo <= baseItem.magazineAmmo)
        {
            currentAmmo = allAmmo;
            allAmmo = 0;
        }
        else
        {
            currentAmmo = baseItem.magazineAmmo;
            allAmmo -= baseItem.magazineAmmo;
        }
    }
}
