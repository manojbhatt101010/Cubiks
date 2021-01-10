using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateLayersWithMouseDrag : MonoBehaviour {
	public float minimumDragPixels = 20.0f;
    public float snapSpeed = 200.0f;
    public static bool canRotate, isRotating;

    Collider[] subCubes = new Collider[9];
    Vector3[] originalPositions = new Vector3[9];
    Quaternion[] originalOrientations = new Quaternion[9];

    RotateLayers rl;

    void Start() {
        canRotate = true;
        isRotating = false;
    	rl = GameObject.Find("Cube").GetComponent<RotateLayers>();
    	StartCoroutine(Rotate());
    }

    IEnumerator Rotate() {
    	while(true) {	
    		Vector3 camForward = Camera.main.transform.forward;
    		float axisSign = Mathf.Sign(camForward.x * camForward.y * camForward.z);
    		yield return null;

    		if(!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject() || RotateCube.isRotating || rl.queue.Count != 0 || !canRotate)  
    			continue;

    		Vector2 clickPosition = Input.mousePosition;
    		Ray ray = Camera.main.ScreenPointToRay(clickPosition);

    		if(!Physics.Raycast(ray, out RaycastHit hit) || hit.transform.name == "Background")
    			continue;

            isRotating = true;
    		int normalAxis = Mathf.Abs(Mathf.RoundToInt(Vector3.Dot(hit.normal, new Vector3(0, 1, 2))));

    		Vector3 rotationAxis = Vector3.zero;
    		Vector3 alternativeAxis = Vector3.zero;

    		rotationAxis[(normalAxis + 1) % 3] = 1;
    		alternativeAxis[(normalAxis + 2) % 3] = 1;

    		float signFlip = axisSign * Mathf.Sign(Vector3.Dot(rotationAxis, camForward) * Mathf.Sign(Vector3.Dot(alternativeAxis, camForward)));
    		Vector2 rotationDirection = signFlip* ScreenDirection(clickPosition, hit.point, alternativeAxis); 
    		Vector2 alternativeDirection = -signFlip * ScreenDirection(clickPosition, hit.point, rotationAxis);

    		float signedDistance;
    		do {
    			yield return null;

    			Vector2 mousePosition = Input.mousePosition;
    			signedDistance = DistanceAlong(clickPosition, mousePosition, rotationDirection);
    			if(Mathf.Abs(signedDistance) > minimumDragPixels)
    				break;

    			signedDistance = DistanceAlong(clickPosition, mousePosition, alternativeDirection);
    			if(Mathf.Abs(signedDistance) > minimumDragPixels) {
    				rotationAxis = alternativeAxis;
    				rotationDirection = alternativeDirection;
    				break;
    			}
    		}
    		while(Input.GetMouseButton(0));

    		Vector3 extents = Vector3.one - 0.9f * rotationAxis;
    		extents = extents * 2.0f;
    		int subCubeCount = Physics.OverlapBoxNonAlloc(hit.collider.transform.position, extents, subCubes);

    		for(int i = 0; i < subCubeCount; i++) {
    			var subCube = subCubes[i].transform;
    			originalPositions[i] = subCube.position;
    			originalOrientations[i] = subCube.rotation;
    		}

    		float angle = 0.0f;
    		while(Input.GetMouseButton(0)) {
    			angle = signedDistance * PlayerPrefs.GetFloat("layersRotationSensitivity", 0.4f);
    			RotateGroup(angle, rotationAxis, subCubeCount);

    			yield return null;
    			Vector2 mousePosition = Input.mousePosition;
    			signedDistance = DistanceAlong(clickPosition, mousePosition, rotationDirection);
    		}

    		float snappedAngle = Mathf.Round(angle / 90.0f) * 90.0f;
    		while(angle != snappedAngle) {
    			angle = Mathf.MoveTowards(angle, snappedAngle, snapSpeed * Time.deltaTime);

    			RotateGroup(angle, rotationAxis, subCubeCount);
    			yield return null;
    		}

    		PerformOtherOperations(subCubeCount);
    	}
    }

    bool Equal(Vector3 a, Vector3 b) {
        return Vector3.SqrMagnitude(a - b) < 0.001;
    }

    void PerformOtherOperations(int count) {
    	HashSet<GameObject> layer = new HashSet<GameObject>();
    	for(int i = 0; i < count; i++) {
    		layer.Add(subCubes[i].gameObject);
    	}

    	if(layer.Contains(rl.cube[0]) && layer.Contains(rl.cube[4])) {
    		if(Equal(rl.cube[0].transform.position, new Vector3(1, 1, -1))) {
    			rl.cfopSolver.U(0);
    			rl.newUAngle = rl.oldUAngle + 90f;
    			rl.SwapU(0);
    		}
    		else if(Equal(rl.cube[0].transform.position, new Vector3(-1, 1, 1))) {
    			rl.cfopSolver.U(1);
    			rl.newUAngle = rl.oldUAngle - 90f;
    			rl.SwapU(1);
    		}
    		else if(Equal(rl.cube[0].transform.position, new Vector3(-1, 1, -1))) {
    			rl.cfopSolver.U(2);
    			rl.newUAngle = rl.oldUAngle + 180f;
    			rl.SwapU(2);
    		}
    		rl.oldUAngle = rl.newUAngle;
    		rl.uLayer.localRotation = Quaternion.Euler(0f, rl.newUAngle, 0f);
    	}

    	else if(layer.Contains(rl.cube[18]) && layer.Contains(rl.cube[22])) {
    		if(Equal(rl.cube[18].transform.position, new Vector3(-1, -1, 1))) {
    			rl.cfopSolver.D(0);
    			rl.newDAngle = rl.oldDAngle - 90f;
    			rl.SwapD(0);
    		}
    		else if(Equal(rl.cube[18].transform.position, new Vector3(1, -1, -1))) {
    			rl.cfopSolver.D(1);
    			rl.newDAngle = rl.oldDAngle + 90f;
    			rl.SwapD(1);
    		}
    		else if(Equal(rl.cube[18].transform.position, new Vector3(-1, -1, -1))) {
    			rl.cfopSolver.D(2);
    			rl.newDAngle = rl.oldDAngle + 180f;
    			rl.SwapD(2);
    		}
    		rl.oldDAngle = rl.newDAngle;
    		rl.dLayer.localRotation = Quaternion.Euler(180f, rl.newDAngle, 0f);
    	}

    	else if(layer.Contains(rl.cube[6]) && layer.Contains(rl.cube[16])) {
    		if(Equal(rl.cube[6].transform.position, new Vector3(-1, 1, -1))) {
    			rl.cfopSolver.F(0);
    			rl.newFAngle = rl.oldFAngle - 90f;
    			rl.SwapF(0);
    		}
    		else if(Equal(rl.cube[6].transform.position, new Vector3(-1, -1, 1))) {
    			rl.cfopSolver.F(1);
    			rl.newFAngle = rl.oldFAngle + 90f;
    			rl.SwapF(1);
    		}
    		else if(Equal(rl.cube[6].transform.position, new Vector3(-1, -1, -1))) {
    			rl.cfopSolver.F(2);
    			rl.newFAngle = rl.oldFAngle + 180f;
    			rl.SwapF(2);
    		}
    		rl.oldFAngle = rl.newFAngle;
    		rl.fLayer.localRotation = Quaternion.Euler(rl.newFAngle, 0f, 90f);
    	}

    	else if(layer.Contains(rl.cube[2]) && layer.Contains(rl.cube[10])) {
    		if(Equal(rl.cube[2].transform.position, new Vector3(1, 1, 1))) {
    			rl.cfopSolver.B(0);
    			rl.newBAngle = rl.oldBAngle + 90f;
    			rl.SwapB(0);
    		}
    		else if(Equal(rl.cube[2].transform.position, new Vector3(1, -1, -1))) {
    			rl.cfopSolver.B(1);
    			rl.newBAngle = rl.oldBAngle - 90f;
    			rl.SwapB(1);
    		}
    		else if(Equal(rl.cube[2].transform.position, new Vector3(1, -1, 1))) {
    			rl.cfopSolver.B(2);
    			rl.newBAngle = rl.oldBAngle + 180f;
    			rl.SwapB(2);
    		}
    		rl.oldBAngle = rl.newBAngle;
    		rl.bLayer.localRotation = Quaternion.Euler(rl.newBAngle, 0f, -90f);
    	}

    	else if(layer.Contains(rl.cube[0]) && layer.Contains(rl.cube[12])) {
    		if(Equal(rl.cube[0].transform.position, new Vector3(-1, 1, 1))) {
    			rl.cfopSolver.L(0);
    			rl.newLAngle = rl.oldLAngle - 90f;
    			rl.SwapL(0);
    		}
    		else if(Equal(rl.cube[0].transform.position, new Vector3(1, -1, 1))) {
    			rl.cfopSolver.L(1);
    			rl.newLAngle = rl.oldLAngle + 90f;
    			rl.SwapL(1);
    		}
    		else if(Equal(rl.cube[0].transform.position, new Vector3(-1, -1, 1))) {
    			rl.cfopSolver.L(2);
    			rl.newLAngle = rl.oldLAngle + 180f;
    			rl.SwapL(2);
    		}
    		rl.oldLAngle = rl.newLAngle;
    		rl.lLayer.localRotation = Quaternion.Euler(rl.newLAngle, 90f, 90f);
    	}

    	else if(layer.Contains(rl.cube[8]) && layer.Contains(rl.cube[14])) {
    		if(Equal(rl.cube[8].transform.position, new Vector3(1, 1, -1))) {
    			rl.cfopSolver.R(0);
    			rl.newRAngle = rl.oldRAngle - 90f;
    			rl.SwapR(0);
    		}
    		else if(Equal(rl.cube[8].transform.position, new Vector3(-1, -1, -1))) {
    			rl.cfopSolver.R(1);
    			rl.newRAngle = rl.oldRAngle + 90f;
    			rl.SwapR(1);
    		}
    		else if(Equal(rl.cube[8].transform.position, new Vector3(1, -1, -1))) {
    			rl.cfopSolver.R(2);
    			rl.newRAngle = rl.oldRAngle + 180f;
    			rl.SwapR(2);
    		}
    		rl.oldRAngle = rl.newRAngle;
    		rl.rLayer.localRotation = Quaternion.Euler(rl.newRAngle, -90f, 90f);
    	}

    	else if(layer.Contains(rl.cube[4]) && layer.Contains(rl.cube[16])) {
    		if(Equal(rl.cube[4].transform.position, new Vector3(-1, 0, 0))) {
    			rl.cfopSolver.M(0);
    			rl.newMAngle = rl.oldMAngle - 90f;
    			rl.SwapM(0);
    		}
    		else if(Equal(rl.cube[4].transform.position, new Vector3(1, 0, 0))) {
    			rl.cfopSolver.M(1);
    			rl.newMAngle = rl.oldMAngle + 90f;
    			rl.SwapM(1);
    		}
    		else if(Equal(rl.cube[4].transform.position, new Vector3(0, -1, 0))) {
    			rl.cfopSolver.M(2);
    			rl.newMAngle = rl.oldMAngle + 180f;
    			rl.SwapM(2);
    		}
    		rl.oldMAngle = rl.newMAngle;
    		rl.mLayer.localRotation = Quaternion.Euler(rl.newMAngle, 90f, 90f);
    	}

    	else if(layer.Contains(rl.cube[16]) && layer.Contains(rl.cube[14])) {
    		if(Equal(rl.cube[16].transform.position, new Vector3(0, 0, -1))) {
    			rl.cfopSolver.E(0);
    			rl.newEAngle = rl.oldEAngle - 90f;
    			rl.SwapE(0);
    		}
    		else if(Equal(rl.cube[16].transform.position, new Vector3(0, 0, 1))) {
    			rl.cfopSolver.E(1);
    			rl.newEAngle = rl.oldEAngle + 90f;
    			rl.SwapE(1);
    		}
    		else if(Equal(rl.cube[16].transform.position, new Vector3(1, 0, 0))) {
    			rl.cfopSolver.E(2);
    			rl.newEAngle = rl.oldEAngle + 180f;
    			rl.SwapE(2);
    		}
    		rl.oldEAngle = rl.newEAngle;
    		rl.eLayer.localRotation = Quaternion.Euler(180f, rl.newEAngle, 0f);
    	}

    	else if(layer.Contains(rl.cube[4]) && layer.Contains(rl.cube[14])) {
    		if(Equal(rl.cube[4].transform.position, new Vector3(0, 0, -1))) {
    			rl.cfopSolver.S(0);
    			rl.newSAngle = rl.oldSAngle - 90f;
    			rl.SwapS(0);
    		}
    		else if(Equal(rl.cube[4].transform.position, new Vector3(0, 0, 1))) {
    			rl.cfopSolver.S(1);
    			rl.newSAngle = rl.oldSAngle + 90f;
    			rl.SwapS(1);
    		}
    		else if(Equal(rl.cube[4].transform.position, new Vector3(0, -1, 0))) {
    			rl.cfopSolver.S(2);
    			rl.newSAngle = rl.oldSAngle + 180f;
    			rl.SwapS(2);
    		}
    		rl.oldSAngle = rl.newSAngle;
    		rl.sLayer.localRotation = Quaternion.Euler(rl.newSAngle, 0f, 90f);
    	}
        isRotating = false;
    }

    Vector2 ScreenDirection(Vector2 screenPoint, Vector3 worldPoint, Vector3 worldDirection) {
    	Vector2 shifted = Camera.main.WorldToScreenPoint(worldPoint + worldDirection);
    	return (shifted - screenPoint).normalized;
    }

    float DistanceAlong(Vector2 clickPosition, Vector2 currentPosition, Vector2 direction) {
    	return Vector2.Dot(currentPosition - clickPosition, direction);
    }

    void RotateGroup(float angle, Vector3 axis, int count) {
    	Quaternion rotation = Quaternion.AngleAxis(angle, axis);
    	for (int i = 0; i < count; i++) {
            var subCube = subCubes[i].transform;
            subCube.position = rotation * originalPositions[i];
            subCube.rotation = rotation * originalOrientations[i];
        }
    }
}