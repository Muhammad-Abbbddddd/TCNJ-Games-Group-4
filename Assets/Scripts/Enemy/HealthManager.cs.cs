using System.Collections;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public ShakeData hitshakedata;
    private SpriteRenderer spriteRenderer;

    public GameObject debuffEffectPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage, Vector2 origin, GameObject player)
    {
        currentHealth -= damage;

        CameraShakerHandler.Shake(hitshakedata);
        GetComponent<Rigidbody2D>().AddForce((GetComponent<Rigidbody2D>().position - origin).normalized * 205f, ForceMode2D.Force);
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            ApplyPlayerDebuffs(player);
            Destroy(gameObject);
        }
    }

    private void ApplyPlayerDebuffs(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerMelee melee = player.GetComponent<PlayerMelee>();
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        if (movement != null && melee != null)
        {
            movement.moveSpeed *= 0.8f;
            movement.jumpPower *= 0.8f;
            melee.attackDamage = Mathf.RoundToInt(melee.attackDamage * 0.8f);

            // Clamp values to prevent extreme reductions
            movement.moveSpeed = Mathf.Max(movement.moveSpeed, 2f);
            movement.jumpPower = Mathf.Max(movement.jumpPower, 4f);
            melee.attackDamage = Mathf.Max(melee.attackDamage, 5);

            StartCoroutine(RestorePlayerStatsAfterDelay(player, 5f)); // 5 seconds

            if (sr != null)
            {
                sr.color = Color.cyan;
            }

            if (debuffEffectPrefab != null)
            {
                GameObject fx = Instantiate(debuffEffectPrefab, player.transform.position, Quaternion.identity, player.transform);
                Destroy(fx, 5f);
            }
        }
    }

    private IEnumerator RestorePlayerStatsAfterDelay(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerMelee melee = player.GetComponent<PlayerMelee>();
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        if (movement != null)
        {
            movement.moveSpeed = movement.defaultMoveSpeed;
            movement.jumpPower = movement.defaultJumpPower;
        }

        if (melee != null)
        {
            melee.attackDamage = melee.defaultAttackDamage;
        }

        if (sr != null)
        {
            sr.color = Color.white;
        }

        Debug.Log("Player stats restored.");
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}
