// Author: Brandon Birman
// Current Revision Date: 11/24/2026
// Purpose: Track enemy targets within a circle of the turret and fire a projectile at them
// References: None

using UnityEngine;

public class BulletTrackingScript : MonoBehaviour
{
    private GameObject enemy;
    private GameObject turret;
    private Collider2D enemyCollider;
    private Rigidbody2D bulletCollider;
    private EnemyModifiers enemyStats;
    private Transform enemyTransform;
    public string bulletType;
    private float speed;
    public float bulletDamage;

    void Start()
    {
        // Set turret as parent object of the bullet
        turret = transform.parent.gameObject;

        // Get enemy gameobject and its collider from the turret's target
        enemy = turret.GetComponent<BulletScript>().player;
        enemyTransform = enemy.transform;
        enemyCollider = enemy.GetComponent<Collider2D>();
        bulletCollider = GetComponent<Rigidbody2D>();
        enemyStats = enemy.GetComponent<EnemyModifiers>();

        // Get bullet's speed
        speed = gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude;

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
    }

    void Update()
    {
        // If the enemy exists, face and move towards it
        if (enemy != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
            Vector3 enemyDirection = enemyTransform.position - transform.position;
            // An improvement in tracking, aims a little ahead of the center of the enemy, towards edge of map
            enemyDirection.x += 0.25f;
            enemyDirection.y += 0.25f;
            float angle = Mathf.Atan2(enemyDirection.y, enemyDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
        }
        // otherwise, destroy bullet after 0.01 seconds
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

        // If bullet and enemy are touching, both destruct
        else if (bulletCollider.IsTouching(enemyCollider))
        { 
            enemyStats.enemyHealth -= bulletDamage;
            Destroy(gameObject);
            enabled = false;
        }
    }
}