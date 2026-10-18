using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCreation : MonoBehaviour
{
    [System.NonSerialized] public List<Waves> enemyWave; // P.S. waves is in Routes Script

    // setup call
    public void EnemySpawn(List<Waves> eWaves, float spawnRate, List<Routes> tracks, float breakTime)
    {
        enemyWave = eWaves;
        StartCoroutine(RunWaves(spawnRate, breakTime, tracks));
    }

    // run through each wave and initialize enemy gameobjects
    public IEnumerator RunWaves(float spawnRate, float breakTime, List<Routes> tracks)
    {
        int i = 0;
        while (i < enemyWave.Count)
        {
            Debug.Log($"Wave: {i + 1}");
            foreach (GameObject enemy in enemyWave[i].enemies)
            {
                GameObject enemyInitialized = Instantiate(enemy);
                Pathing enemyPath = enemyInitialized.GetComponent<Pathing>();
                
                // pick a random path to follow
                int pathPick = Random.Range (0, tracks.Count);
                enemyPath.BeginPassage(tracks[pathPick].path);

                yield return new WaitForSeconds(spawnRate);
            }

            i++;
            yield return new WaitForSeconds(breakTime);
        }

        yield return null;
    }

}

/*
 * Thinking about it maybe we can make straights combined instead of a point through tile? Might reduce lag if need be
 * Also thinking about how we can maybe group enemies that are moving in the same direction? Not sure
 */