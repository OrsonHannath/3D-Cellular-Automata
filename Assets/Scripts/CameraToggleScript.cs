using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToggleScript : MonoBehaviour
{

    private Camera[] cameras;
    private int currentCamIndex = 0;

    // Start is called before the first frame update
    void Start() {

        cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++) {

            if (i != currentCamIndex) {
                cameras[i].gameObject.SetActive(false);
            } else {
                cameras[i].gameObject.SetActive(true);
            }
        }
    }

    public void ToggleCamera() {

        currentCamIndex++;

        if (currentCamIndex >= cameras.Length) {

            currentCamIndex = 0;
        }

        for (int i = 0; i < cameras.Length; i++) {

            if (i != currentCamIndex) {
                cameras[i].gameObject.SetActive(false);
            } else {
                cameras[i].gameObject.SetActive(true);
            }
        }
    }
}
