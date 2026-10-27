// Author: Brandon Birman
// Current Revision Date: 10/26/2026
// Purpose: Track enemy targets within a circle of the turret and fire a projectile at them
// References: None

using UnityEngine;

public class BulletTrackingScript : MonoBehaviour
{

    private GameObject enemy;
    private GameObject turret;
    private Collider2D enemyCollider;
    private Rigidbody2D bulletCollider;
    private float speed;
    private Transform enemyTransform;

    void Start()
    {
        // Set turret as parent object of the bullet
        turret = transform.parent.gameObject;

        // Get enemy gameobject and its collider from the turret's target
        enemy = turret.GetComponent<BulletScript>().player;
        enemyTransform = enemy.transform;
        enemyCollider = enemy.GetComponent<Collider2D>();
        bulletCollider = GetComponent<Rigidbody2D>();

        // Get bullet's speed
        speed = gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude;

        //enemy = bs.player;
    }

    void Update()
    {
        // If the enemy exists, face and move towards it
        if (enemy != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
            Vector3 enemyDirection = enemyTransform.position - transform.position;
            float angle = Mathf.Atan2(enemyDirection.y, enemyDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
        }
        // otherwise, destroy bullet after 0.5 seconds
        else
        {
            Destroy(gameObject, 0.5f);
            enabled = false;
        }

        // Self destruct after 0.5 seconds if enemy no longer exists
        if (enemy == null)
        {
            Destroy(gameObject, 0.5f);
            enabled = false;
        }

        // If bullet and enemy are touching, both destruct
        else if (bulletCollider.IsTouching(enemyCollider))
        {
            Destroy(enemy);
            Destroy(gameObject);
            enabled = false;
        }
    }
}