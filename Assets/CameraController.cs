using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform spaceShip; // Holding the transformations of the spacecraft

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void LateUpdate() {
		gameObject.transform.LookAt (spaceShip.position);
	}
}
