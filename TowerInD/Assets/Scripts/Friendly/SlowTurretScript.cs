// Author: Brandon Birman
// Current Revision Date: 11/24/2026
// Purpose: Track enemy targets within a circle of the turret and slow them down
// References: BulletScript

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SlowTurretScript : MonoBehaviour
{
    public GameObject towerRadiusPrefab; // Sprite for turret radius
    public float turretRadius; // How far the turret scans for targets
    public float newTileSpeed;
    private float enemySpeed;

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
        
        yield return new WaitForSeconds(0f);
    }

    private void OnTriggerEnter2D(Collider2D enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            enemySpeed = enemy.GetComponent<Pathing>().tileSpeed;
            enemy.GetComponent<Pathing>().tileSpeed = newTileSpeed;
        }
    }

    private void OnTriggerExit2D(Collider2D enemy)
    {
        if (enemy.CompareTag("Enemy"))
        {
            enemy.GetComponent<Pathing>().tileSpeed = enemySpeed;
        }
    }

}