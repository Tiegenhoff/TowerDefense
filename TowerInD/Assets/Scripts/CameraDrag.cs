using UnityEngine;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    public Camera cam;



    void Start()
    {
        //cam = GetComponent<Camera>();
    }

    


    void Update()
    {
        
    }


    //Function called by UP and DOWN buttons in CameraControls
    public void Zoom(bool zoom) 
    {
        if (zoom && cam.orthographicSize > 1) // if true decrease camera size
        {
            cam.orthographicSize--;
        }
        else if (!zoom && cam.orthographicSize < 10) // if false increase camera size
        {
            cam.orthographicSize++;
        }
    }
}
