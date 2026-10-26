using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayButton : MonoBehaviour
{
    public GameObject camera;

    public void PressPlay()
    {
        StartCoroutine(GoDown());
    }

    private IEnumerator GoDown()
    {
        Vector3 start = camera.transform.position;
        Vector3 end = new Vector3(camera.transform.position.x, 0f, camera.transform.position.z);
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            camera.transform.position = Vector3.Lerp(start, end, elapsed / 1f);
            yield return null;
        }

        camera.transform.position = end;
    }
}
