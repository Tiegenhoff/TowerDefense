using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Routes
{
    public List<Vector3> path;
}

[System.Serializable]
public class Waves
{
    public List<GameObject> enemies;
}