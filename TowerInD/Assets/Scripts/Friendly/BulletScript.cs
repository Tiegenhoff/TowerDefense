// Author: Brandon Birman
// Current Revision Date: 10/26/2026
// Purpose: Track enemy targets within a circle of the turret and fire a projectile at them
// References: Most of this code was created by Austin for a similiar purpose


using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BulletScript : MonoBehaviour
{
    public GameObject bulletPrefab; //the bullet in question
    public GameObject towerRadiusPrefab; // Sprite for turret radius
    public int bulletCount; //amount of bullets spawned
    public int radius; //360 how much room to spawn the bullets
    public float turretRadius; // How far the turret scans for targets
    public bool AimAtPlr; //else random
    public int repeatShot; //direct follow up shots
    public float repeatShotDelay; //delay between repeat shots
    public float delay; //time between sending another round of bullets
    public float velocity; //speed of the bullets
    public GameObject player; // enemy being targeted

    // Start BulletRoutine() without any call from another function, can remove if spawning function calls startBulletRoutine() instead
    private void Start()
    {
        StartCoroutine(BulletRoutine());
    }

    public void startBulletRoutine()
    {
        GameObject towerRadiusVisual = Instantiate(towerRadiusPrefab, transform.position, Quaternion.identity);
        towerRadiusVisual.transform.localScale = new Vector3(turretRadius, turretRadius, 1);

        StartCoroutine(BulletRoutine());
    }

    IEnumerator BulletRoutine()
    {
        // Creates a png transparent circle around towers to show their attack range
        GameObject towerRadiusVisual = Instantiate(towerRadiusPrefab, transform.position, Quaternion.identity);
        towerRadiusVisual.transform.localScale = new Vector3(turretRadius * 2, turretRadius * 2, 1);

        while (true)
        {
            // Checks if any enemies exist within the turret's radius
            player = FindTarget(turretRadius);
            if (player == null)
            {
                //Debug.Log("No enemies present");
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            else Debug.Log("Enemies present!");

                float defAngle = GetAngle(AimAtPlr);
            float angle;

            for (int i = 0; i < repeatShot + 1; i++)
            {
                //need to split the area into say bullet count and then get the middle of each of those areas.
                angle = defAngle - (radius / 2);
                for (int j = 0; j < bulletCount; j++)
                {
                    angle += (radius / bulletCount) / 2;
                    //spawn bullet
                    //Vector2 offset = Vector2.zero;
                    //offset = (angle > 90 && angle < 270) ? -offset : offset;
                    Vector3 angleSet = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);

                    GameObject bullet = Instantiate(bulletPrefab, (transform.position + angleSet), Quaternion.identity, transform);
                    
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    rb.AddForce(angleSet * velocity, ForceMode2D.Impulse);

                    angle += (radius / bulletCount)/2;
                }
                yield return new WaitForSeconds(repeatShotDelay);
            }
            yield return new WaitForSeconds(delay);
        }
    }

    float GetAngle(bool plyrORandom)
    {
        float angle = 0;
        if (plyrORandom)
        {
            Vector3 plyrPos = player.transform.position;
            plyrPos.z = 0f;

            Vector2 direction = (plyrPos - transform.position).normalized;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        else
        {
            angle = Random.Range(0f, 360f);
        }

        return angle;
    }

    // Checks within a circle radius from the turret for enemies
    GameObject FindTarget(float turretRadius)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, turretRadius);

        foreach (var target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                return target.gameObject;
            }
        }

        return null;
    }
}