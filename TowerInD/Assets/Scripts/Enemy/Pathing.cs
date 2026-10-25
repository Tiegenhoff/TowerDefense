using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pathing : MonoBehaviour
{
    // the path the enemy will follow
    private List<Vector3> path;
    private bool moving;

    private Vector3 forwardSet = new Vector3(0, 0, -1);

    public float tileSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BeginPassage(List<Vector3> p)
    {
        path = p;
        moving = true;
        transform.position = p[0] + forwardSet;
        StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        int i = 0;

        while (moving)
        {
            
            float elapsedTime = 0;
            


            while (elapsedTime < tileSpeed)
            {
                float t = elapsedTime / tileSpeed;

                transform.position = Vector3.Lerp(path[i] + forwardSet, path[i + 1] + forwardSet, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            i++;
            transform.position = path[i] + forwardSet;
            
            // if at end
            if (i + 1 == path.Count)
                moving = false;
        }

        Destroy(gameObject);
        yield return null;
    }

}
