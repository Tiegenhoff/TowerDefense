// Author: Brandon Birman
// Current Revision Date: 11/24/2026
// Purpose: Launch an explosive projectile at the predicted location of an enemy
// References: None

using UnityEngine;

public class CannonTrackingScript : MonoBehaviour
{
    private GameObject enemy;
    private GameObject turret;
    // private Collider2D enemyCollider;
    // private Rigidbody2D bulletCollider;
    private EnemyModifiers enemyStats;
    private Transform enemyTransform;
    public string bulletType;
    public float speed;
    public float explosionRadius;
    private float distance;
    private Vector3 enemyVelocity;
    private Vector3 impactPoint;
    public float bulletDamage;

    void Start()
    {
        // Set turret as parent object of the bullet
        turret = transform.parent.gameObject;

        // Get enemy gameobject and its collider from the turret's target
        enemy = turret.GetComponent<BulletScript>().player;
        enemyTransform = enemy.transform;
        // enemyCollider = enemy.GetComponent<Collider2D>();

        // Get bullet's speed
        // speed = gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude;

        
        distance = Vector3.Distance(enemyTransform.position, gameObject.transform.position);
        enemyVelocity.x = (1 / enemy.GetComponent<Pathing>().tileSpeed);
        enemyVelocity.y = (1 / enemy.GetComponent<Pathing>().tileSpeed);
        impactPoint.x = enemyTransform.position.x + 0.5f;
        impactPoint.y = enemyTransform.position.y;
        impactPoint.z = -1f;
        float angle = Mathf.Atan2(impactPoint.y, impactPoint.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
    }

    void Update()
    {
        bool moving = true;
        if (Vector3.Distance(transform.position, impactPoint) <= 0.01f)
        {
            // gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
            // Debug.Log("Impacted");
            moving = false;
            transform.position = impactPoint;
            HitTargets();
            Destroy(gameObject, 0f);

            enabled = false;
        }
        // Move towards impact point
        else if (enemy != null && moving == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, impactPoint, speed * Time.deltaTime);
        }
        // otherwise, destroy cannonball after 0.01 seconds
        else
        {
            Destroy(gameObject, 0.01f);
            enabled = false;
        }

        // Self destruct after 0.01 seconds if enemy no longer exists
        if (enemy == null)
        {
            Destroy(gameObject, 0.01f);
            enabled = false;
        }
    }
    private void HitTargets()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                enemyStats = target.GetComponent<EnemyModifiers>();
                // Set damage based on enemy and bullet's types
                for (int i = 0; i < enemyStats.enemyWeaknesses.Length; i++)
                {
                    if (bulletType == enemyStats.enemyWeaknesses[i])
                    {
                        bulletDamage *= 1.25f;
                        break;
                    }
                }
                for (int i = 0; i < enemyStats.enemyStrengths.Length; i++)
                {
                    if (bulletType == enemyStats.enemyStrengths[i])
                    {
                        bulletDamage *= 0.75f;
                        break;
                    }
                }
                target.GetComponent<EnemyModifiers>().enemyHealth -= bulletDamage;
            }
        }
    }
}