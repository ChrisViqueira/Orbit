using UnityEngine;
using System.Collections;

public class SpaceshipCameraController : MonoBehaviour {
	
	public Transform spaceShip; // Holding the transformations of the spacecraft
	
	public float defTheta = Mathf.PI / 2;   // Default value of theta.
	public float defPsy = 0.5f;				// Default value of psy.
	public float defRadius = 30f;			// Default radius from ship.
	
	public float theta;  					// Radians around y-axis (horizontal).
	public float psy;	 					// Radians around x-axis (vertical).
	public float radius; 					// Distance from marble.
	public float shipRadius;				// Player preferred distance from ship.
	public float shipPsy;					// Player preferred psy. Currently not adjustable.
	
	public const float PSYMAX = (Mathf.PI / 2) - 0.1f; 		// Maximum value for psy. Camera inverts at Pi/2+.
	public const float PSYMIN = -(Mathf.PI / 2) + 0.1f;		// Minimum value for psy.
	public const float RADMIN = 35f;					    // Minimum distance from ship
	public const float RADMAX = 500f;
	
	public float keyboardSensitivity; 	// Keyboard sensitivity.
	
	// Use this for initialization
	void Start () {
		radius = defRadius;
		shipRadius = radius;
		shipPsy = psy;
		
		keyboardSensitivity = 1f;
	}
	
	// Update is called once per frame
	void Update () {
		CameraControls ();

		float cameraPos = transform.position.magnitude - spaceShip.position.magnitude;
		// Allows zooming in and out.
		if(Input.GetAxis("Mouse ScrollWheel") != 0) {
			radius -= Input.GetAxis("Mouse ScrollWheel") * .005f * Mathf.Pow(cameraPos, 2);
			shipRadius = radius;	// Changes player preferred radius
		} 

		if (radius < RADMIN)
			radius = RADMIN;
		if (radius > RADMAX)
			radius = RADMAX;
	}
	
	//
	void LateUpdate () {
		transform.position = GetSphericalPosition ();
		transform.LookAt (spaceShip.position);
	}
	
	// Assigning buttons to certain camera movements
	void CameraControls () {


		if (Input.GetKey (KeyCode.UpArrow))
			MoveUp ();
		if (Input.GetKey (KeyCode.DownArrow))
			MoveDown ();
		if (Input.GetKey (KeyCode.LeftArrow))
			MoveLeft ();
		if (Input.GetKey (KeyCode.RightArrow))
			MoveRight ();
	}
	
	
	// GetSphericalPosition - Return spherical coordinate of camera
	Vector3 GetSphericalPosition() {
		Vector3 retPos = new Vector3();
		
		// These are all using radians.
		retPos.x = radius * Mathf.Cos (psy) * Mathf.Cos (theta) + spaceShip.position.x;
		retPos.y = radius * Mathf.Sin (psy) + spaceShip.position.y;
		retPos.z = radius * Mathf.Cos (psy) * Mathf.Sin (theta) + spaceShip.position.z;
		
		return retPos;
	}
	
	// Control Functions
	#region Control Functions
	// Moves camera up.
	public void MoveUp () {
		psy = Mathf.Clamp(psy + (keyboardSensitivity * Time.deltaTime * (1/Time.timeScale)), PSYMIN, PSYMAX);
	}
	
	// Moves camera down.
	public void MoveDown() {
		psy = Mathf.Clamp(psy - (keyboardSensitivity * Time.deltaTime * (1/Time.timeScale)), PSYMIN, PSYMAX);
		
	}
	
	// Moves camera left.
	public void MoveLeft() {
		theta -= keyboardSensitivity * Time.deltaTime * (1/Time.timeScale);
		
	}
	
	// Moves camera right.
	public void MoveRight() {
		theta += keyboardSensitivity * Time.deltaTime * (1/Time.timeScale);
		
		
	}
	
	#endregion
}
