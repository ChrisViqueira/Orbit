using UnityEngine;
using System.Collections;

public class shipBehavior : MonoBehaviour {
	
	#region VARIABLES
	
	//public const float GRAVITATIONAL_CONSTANT = .0000000000667f;
	public const float GRAVITATIONAL_CONSTANT = .00667f;
	
	public Transform planet;
	
	public Transform UpRay; 	// casting a ray from the bottom of the ship
	public Transform FrontRay; 	// casting a ray toward the front of the ship
	public Transform LeftRay;  // casting a ray right of the ship
	
	public float yaw = 0.0f;
	public float pitch = 0.0f;
	public float roll = 0.0f;
	public float thrust = 0.0f;
	
	public bool killRotation = false;

	public Rigidbody satelliteBody;

	#endregion
	
	// Start - used for initilization at the start of the simulation
	void Start () {
		satelliteBody = GetComponent<Rigidbody> ();
		satelliteBody.maxAngularVelocity = 50;
		satelliteBody.AddForce (0, 0, 94.3f, ForceMode.VelocityChange);
	}
	
	// Update - Called every frame
	void Update () {
		ChangeTimeScale ();
		KillShipRotation ();
		if (Input.GetKeyDown (KeyCode.T))
			killRotation = true;
		
		
		Debug.DrawRay(transform.position, UpRay.position - transform.position, Color.blue); // DEBUG
		Debug.DrawRay(transform.position, FrontRay.position - transform.position, Color.blue); // DEBUG
		Debug.DrawRay(transform.position, LeftRay.position - transform.position, Color.blue); // DEBUG
	}
	
	// FixedUpdate - called every fixed framerate or every physics step, eliminates error from gravity calculations
	void FixedUpdate () {
		ApplyGravity ();
		ChangeShipOrientation ();
		AddSatelliteVelocity ();
	}
	
	// ApplyGravity - calculates the force of gravity then applies the force as an acceleration to the ship
	void ApplyGravity () {
		// Calculate the gravitation force using Newton's Law of Universal Gravitation Gmm/(d^2)
		// NOTE: since ship mass is so small it is negligible, therefore the second m is ignored
		float grav = ((GRAVITATIONAL_CONSTANT * planet.GetComponent<Rigidbody>().mass) / gameObject.transform.position.sqrMagnitude);
		
		Vector3 gravityVector = GetComponent<Rigidbody> ().position.normalized * -grav; // normalize the position vector of the ship then apply grav
		satelliteBody.AddForce (gravityVector, ForceMode.Acceleration); 	// add the new force to the ship as an acceleration
	}
	
	// GetShipVelocity - Gets the absoulte ship velocity (Sqrt(x^2 + y^2 + z^2)) where x, y, and z are the velocity components
	public float GetShipVelocity () {
		float xVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).x;
		float yVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).y;
		float zVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).z;
		Vector3 velocityVector = new Vector3 (xVel, yVel, zVel);
		
		return Mathf.Sqrt(velocityVector.sqrMagnitude);
	}
	
	// ChangeTimeScale - allows the simulation to speed up, helpful for longer and bigger orbits
	public void ChangeTimeScale () {
		float maxTimeScale = 8.0f;
		
		if (Input.GetKeyDown(KeyCode.Period)) {
			if (Time.timeScale >= maxTimeScale)
				Time.timeScale = maxTimeScale;
			else
				Time.timeScale *= 2.0f;
		}
		if (Input.GetKeyDown(KeyCode.Comma)) {
			if (Time.timeScale <= 1.0f)
				Time.timeScale = 1.0f;
			else
				Time.timeScale /= 2.0f;
		}
	}

	//********************************UNFINISHED NOT WORKING CORRECTLY****************************************//
	// ChangeShipOrientation - Handles the controls for rotation the ships yaw, pitch, and roll
	void ChangeShipOrientation () {

		//satelliteBody.centerOfMass = transform.position;
		//Debug.Log (satelliteBody.centerOfMass);
		
		/*float changeYPR = .000001f;
		yaw = 0.0f;
		pitch = 0.0f;
		roll = 0.0f;
		
		if (Input.GetKey (KeyCode.A))
			yaw -= changeYPR;
		if (Input.GetKey (KeyCode.D))
			yaw += changeYPR;
		if (Input.GetKey (KeyCode.W))
			pitch -= changeYPR;
		if (Input.GetKey (KeyCode.S))
			pitch += changeYPR;
		if (Input.GetKey (KeyCode.Q))
			roll -= changeYPR;
		if (Input.GetKey (KeyCode.E))
			roll += changeYPR;
		
		satelliteBody.AddRelativeTorque (new Vector3(pitch, yaw, roll) * satelliteBody.mass);
*/

		float YPRforce = .0000001f;

		if (Input.GetKey (KeyCode.A))
			satelliteBody.AddRelativeTorque (-Vector3.up * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.D))
			satelliteBody.AddRelativeTorque (Vector3.up * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.W))
			satelliteBody.AddRelativeTorque (-Vector3.right * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.S))
			satelliteBody.AddRelativeTorque (Vector3.right * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.Q))
			satelliteBody.AddRelativeTorque (-Vector3.forward * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.E))
			satelliteBody.AddRelativeTorque (Vector3.forward * YPRforce * satelliteBody.mass);

		Debug.Log (satelliteBody.angularVelocity.magnitude);
	}
	
	// KillShipRotation - eliminates the current torque on the ship to stabilize the ship
	public void KillShipRotation()
	{
		float dampenSpeed = .1f;
		
		if (killRotation) {
			if (yaw < 0.0f)
				yaw += dampenSpeed;
			if (pitch < 0.0f)
				pitch += dampenSpeed;
			if (roll < 0.0f)
				roll += dampenSpeed;
			
			if (yaw > 0.0f)
				yaw -= dampenSpeed;
			if (pitch > 0.0f)
				pitch -= dampenSpeed;
			if (roll > 0.0f)
				roll -= dampenSpeed;
			
			if (roll == 0.0f && yaw == 0.0f && pitch == 0.0f)
				killRotation = false;
		}
	}

	//********************************UNFINISHED NOT WORKING CORRECTLY****************************************//
	// AddSatelliteVelocity - Adds velocity directly forward from the front of the satellite
	public void AddSatelliteVelocity()
	{
		float thrustIncrease = 1.0f;
		Vector3 satelliteForward = FrontRay.position.normalized - transform.position.normalized;

		if (Input.GetKey (KeyCode.J))
			thrust = 0.0f;
		if (Input.GetKey (KeyCode.H))
			thrust += thrustIncrease;

		satelliteBody.AddRelativeForce (satelliteForward * thrust * satelliteBody.mass);

		Debug.Log (thrust);
	}
	
	
} // end ship behavior








