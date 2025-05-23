using System.Collections;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public ShakeData hitshakedata;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage, Vector2 origin, GameObject player)
    {
        currentHealth -= damage;

        // Camera shake
        CameraShakerHandler.Shake(hitshakedata);

        // Knockback
        GetComponent<Rigidbody2D>().AddForce((GetComponent<Rigidbody2D>().position - origin).normalized * 205f, ForceMode2D.Force);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            PlayerMovement playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();

            if (playerMovement != null && !playerMovement.IsDebuffActive())
            {
                playerMovement.ActivateDebuff();
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}
