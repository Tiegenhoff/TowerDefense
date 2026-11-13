using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Periodically destroys every enemy in range, using the existing one-hit enemy system.
public class PulseTurret : MonoBehaviour
{
    public float turretRadius = 2f;
    public float pulseDelay = 2f;
    public LayerMask enemyLayers = ~0;
    public GameObject towerRadiusPrefab;
    public GameObject pulseEffectPrefab;
    private GameObject radiusVisual;

    private void OnEnable()
    {
        if (towerRadiusPrefab != null)
        {
            radiusVisual = Instantiate(towerRadiusPrefab, transform.position, Quaternion.identity, transform);
            radiusVisual.transform.localScale = new Vector3(turretRadius * 2, turretRadius * 2, 1);
        }
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, pulseDelay));
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Mathf.Max(0f, turretRadius), enemyLayers);
            HashSet<Pathing> enemies = new HashSet<Pathing>();
            foreach (Collider2D hit in hits)
            {
                Pathing enemy = hit.GetComponentInParent<Pathing>();
                if (enemy != null && enemy.isActiveAndEnabled) enemies.Add(enemy);
            }
            if (enemies.Count == 0) continue;
            if (pulseEffectPrefab != null)
            {
                GameObject effect = Instantiate(pulseEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            foreach (Pathing enemy in enemies)
            {
                EnemyMaterialDrop drop = enemy.GetComponentInParent<EnemyMaterialDrop>();
                if (drop != null) drop.DropMaterial();
                Destroy(enemy.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (radiusVisual != null) Destroy(radiusVisual);
        radiusVisual = null;
    }
}
