using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    // public HealthBar healthBar;
    // public GameObject bloodEffect;
    public ShakeData hitshakedata;

    private void Start()
    {
        currentHealth = maxHealth;
        // healthBar.SetMaxHealth(maxHealth);
    }


    public void TakeDamage(int damage, Vector2 origin)
    {
        currentHealth -= damage;

        // Blood Particle example
        // Instantiate(bloodEffect, transform.position, Quaternion.identity);
        
        // Camera shake code example
       CameraShakerHandler.Shake(hitshakedata);

        // Knockback code example
        GetComponent<Rigidbody2D>().AddForce((GetComponent<Rigidbody2D>().position - origin).normalized * 65f, ForceMode2D.Force);

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }

        // healthBar.SetCurrentHealth(currentHealth);
    }
}
