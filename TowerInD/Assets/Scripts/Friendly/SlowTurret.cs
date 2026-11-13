using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Slows all enemies within range while they remain near this turret.
public class SlowTurret : MonoBehaviour
{
    public float turretRadius = 3f;
    [Range(0.05f, 1f)] public float speedMultiplier = 0.5f;
    public float scanInterval = 0.1f;
    public LayerMask enemyLayers = ~0;
    public GameObject towerRadiusPrefab;

    private HashSet<Pathing> affected = new HashSet<Pathing>();
    private GameObject radiusVisual;

    private void OnEnable()
    {
        if (towerRadiusPrefab != null)
        {
            radiusVisual = Instantiate(towerRadiusPrefab, transform.position, Quaternion.identity, transform);
            radiusVisual.transform.localScale = new Vector3(turretRadius * 2, turretRadius * 2, 1);
        }
        StartCoroutine(ScanEnemies());
    }

    private IEnumerator ScanEnemies()
    {
        while (true)
        {
            HashSet<Pathing> inside = new HashSet<Pathing>();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(0f, turretRadius), enemyLayers);
            foreach (Collider2D hit in hits)
            {
                Pathing enemy = hit.GetComponentInParent<Pathing>();
                if (enemy == null || !enemy.isActiveAndEnabled) continue;
                inside.Add(enemy);
                enemy.SetSlow(this, speedMultiplier);
            }
            foreach (Pathing enemy in affected)
                if (enemy != null && !inside.Contains(enemy)) enemy.RemoveSlow(this);
            affected = inside;
            yield return new WaitForSeconds(Mathf.Max(0.02f, scanInterval));
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (Pathing enemy in affected)
            if (enemy != null) enemy.RemoveSlow(this);
        affected.Clear();
        if (radiusVisual != null) Destroy(radiusVisual);
        radiusVisual = null;
    }
}
