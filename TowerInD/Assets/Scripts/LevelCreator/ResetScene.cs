using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    public string sceneName;

    public void ResetMap()
    {
        SceneManager.LoadScene(sceneName);
    }
}
