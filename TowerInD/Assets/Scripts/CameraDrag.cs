using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraDrag : MonoBehaviour
{
    public Camera cam;
    public GameObject camG;
    public GameObject Circle; // joystick
    public float camSpeed;

    private Vector3 cVelocity;
    private Vector3 dVelocity;

    // Called via button press of the CameraControl
    public void CameraPan()
    {
        Debug.Log($"Starting Pan");
        StartCoroutine(MoveCamera());
    }

    // While Camera Controls Are Held Down, Continue Panning
    public IEnumerator MoveCamera()
    {
        // Doesn't change so no need to recall numerous times
        Vector3 panelPos = transform.position;
        cVelocity = Vector3.zero;
        dVelocity = Vector3.zero;

        while (Mouse.current.leftButton.isPressed)
        {
            Debug.Log($"Panning");
            // Set joystick to mouse position
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 circlePos = new Vector3(mousePos.x, mousePos.y, 0);
            Circle.transform.position = circlePos;

            // Get direction and magnitude
            Vector3 diff = circlePos - panelPos;
            float magnitude = diff.magnitude;
            //Vector3 direction = diff.normalized;

            // Apply to the camera via updated values
            dVelocity = Vector3.ClampMagnitude(diff, magnitude);
            //cVelocity = Vector3.MoveTowards(cVelocity, dVelocity, camSpeed * Time.deltaTime);
            //camG.transform.position += cVelocity * Time.deltaTime;
            camG.transform.position += dVelocity * camSpeed * Time.deltaTime;

            yield return null;
        }

        Circle.transform.position = panelPos;
        yield return null;
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
