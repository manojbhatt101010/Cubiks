using UnityEngine;
using UnityEngine.EventSystems;

public class RotateCube : MonoBehaviour {
	public GameObject cube;
	public static bool isRotating;
	public static bool canRotate;
	bool overUI = false;

	void Start() {
		isRotating = false;
		canRotate = true;
	}

	void Update() {
		// Finished Rotating
		if(Input.GetMouseButtonUp(0)) {
			overUI = false;
			isRotating = false;
		}
	}

	void OnMouseDrag() {
		// Clicked on UI
		if(EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject()) {
			overUI = true;
			isRotating = true; 
        }

        if(!overUI && canRotate) {
			isRotating = true;
			Camera.main.transform.RotateAround(cube.transform.position, transform.up, Input.GetAxis("Mouse X") * PlayerPrefs.GetFloat("cubeRotationSensitivity", 4f));
			Camera.main.transform.RotateAround(cube.transform.position, transform.right, Input.GetAxis("Mouse Y") * -PlayerPrefs.GetFloat("cubeRotationSensitivity", 4f));
		}
	}
}