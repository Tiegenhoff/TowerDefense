// Author: Brandon Birman
// Current Revision Date: 11/24/2026
// Purpose: Very basic modifier system that needs to be fleshed out
// References:

using UnityEngine;

public class EnemyModifiers : MonoBehaviour
{
    private GameObject enemy;
    public string enemyType;
    public string[] enemyWeaknesses;
    public string[] enemyStrengths;
    public float enemyHealth;
    void Start()
    {
        enemy = gameObject;
    }

    void Update()
    {
        if (enemyHealth <= 0)
        {
            Destroy(enemy);
        }
    }
}