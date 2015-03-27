using UnityEngine;
using System.Collections;

public class SpaceshipCameraController : MonoBehaviour {

	#region Variables

	public Transform spaceShip; // Holds the position of the spaceship

	public float theta;  	// Radians around y-axis (horizontal).
	public float psy;	 			// Radians around x-axis (vertical).
	public float radius; 				// Distance from satellite.
	
	#endregion

	// Use this for initialization
	void Start () {
//		radius = Vector3.Distance(transform.position, spaceShip.position);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate() {
		gameObject.transform.position = GetShipSphericalPosition();
		gameObject.transform.LookAt (spaceShip.position);
	}

	// GetSphericalPosition - Return spherical coordinate of camera
	Vector3 GetShipSphericalPosition() {
		Vector3 retPos = new Vector3();
		
		// These are all using radians.
		retPos.x = radius * Mathf.Cos (psy) * Mathf.Cos (theta) + spaceShip.position.x;
		retPos.y = radius * Mathf.Sin (psy) + spaceShip.position.y;
		retPos.z = radius * Mathf.Cos (psy) * Mathf.Sin (theta) + spaceShip.position.z;
		
		return retPos;
	}
}
