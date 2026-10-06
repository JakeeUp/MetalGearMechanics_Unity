using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Controller controller;
    public Slider healthSlider;
    public Slider easeHealthSlider;
    private float hpMaxHealth;
    private float health;
    private float lerpSpeed = 0.05f;
    

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<Controller>(); 
        hpMaxHealth = controller.maxHealth;
        health = hpMaxHealth;
        healthSlider.maxValue = hpMaxHealth;
        healthSlider.value = hpMaxHealth;

    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != controller.currentHealth)
        {
            healthSlider.value = controller.currentHealth;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            takeDamage(10);
        }
        if(healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, controller.currentHealth, lerpSpeed);
        }
    }

    private void takeDamage(float damage)
    {
        controller.OnHit(damage);
    }
}
