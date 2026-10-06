using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSound : MonoBehaviour
{
    public AudioSource pickupSoundSource;
    public AudioClip pickupSoundClip;

    private void Start()
    {
        pickupSoundSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {

            pickupSoundSource.clip = pickupSoundClip;
            pickupSoundSource.Play();
            Destroy(this);
        }
    }
}
