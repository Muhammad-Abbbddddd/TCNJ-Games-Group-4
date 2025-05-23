using System.Collections;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public Transform attackOrigin;
    public float attackRadius = 1f;
    public LayerMask enemyMask;

    public float cooldownTime = .5f;
    private float cooldownTimer = 0f;

    public int attackDamage = 25;
    [HideInInspector] public int defaultAttackDamage;

    public Animator animator;

    private void Start()
    {
        defaultAttackDamage = attackDamage;
    }

    private void Update()
    {
        if (cooldownTimer <= 0)
        {
            if (Input.GetKey(KeyCode.E))
            {
                AttackEnemy(attackOrigin, attackRadius, "Bite", enemyMask, attackDamage, transform.position, gameObject, cooldownTime, out cooldownTimer);
            }
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    
    public static void AttackEnemy(Transform attackOrigin, float attackRadius, string attackType, LayerMask enemyMask, int attackDamage, Vector3 attackerPosition, GameObject attacker, float cooldownTime, out float cooldownTimer)
    {
        if (attackType == "Bite") 
        {
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyMask);
            foreach (var enemy in enemiesInRange)
            {
                enemy.GetComponent<HealthManager>().TakeDamage(attackDamage, attackerPosition, attacker);
                SoundEffectManager.Play("Bite");
                MusicManager.OnPlayerAttack();
            }

            cooldownTimer = cooldownTime;
        }
        else if (attackType == "Dash") //Made this into a seperate function incase I get time to turn this into a looped event while dash is occuring
        {
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyMask);
            foreach (var enemy in enemiesInRange)
            {
                enemy.GetComponent<HealthManager>().TakeDamage(attackDamage, attackerPosition, attacker);
                SoundEffectManager.Play("Bite");
            }

            cooldownTimer = cooldownTime; 
        }
        else
        {
            cooldownTimer = 0f; // Or some fallback/default value
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
    }
}
