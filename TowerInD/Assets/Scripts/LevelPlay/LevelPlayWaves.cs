using System.Collections;
using UnityEngine;

public partial class LevelPlay : MonoBehaviour
{
    [Header("PlyrPlaceUnit?")]
    public bool startedPlacement = false;

    [Header("WaveCreationScript")]
    public WaveCreation wc;

    private IEnumerator StartWaves()
    {
        // wait for unit placement
        while (!startedPlacement)
        {

            yield return null;
        }

        // actually start the waves
        wc.EnemySpawn(waveSave, spawnTime, tracksSave, breakTime);

        yield return null;
    }

}
