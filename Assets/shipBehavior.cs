using UnityEngine;
using System.Collections;

public class shipBehavior : MonoBehaviour {

	#region Variables

	//public const float GRAVITATIONAL_CONSTANT = .0000000000667f;
	public const float GRAVITATIONAL_CONSTANT = .00667f;

	public Transform planet;

	public Transform UpRay; 	// casting a ray from the bottom of the ship
	public Transform FrontRay; 	// casting a ray toward the front of the ship
	public Transform LeftRay;  // casting a ray right of the ship

	public float yaw = 0.0f;
	public float pitch = 0.0f;
	public float roll = 0.0f;
	#endregion

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody>().AddForce (0, 0, 131.7f, ForceMode.VelocityChange);
	}

	void Update () {
		ChangeTimeScale ();
		ChangeShipOrientation ();
		if (Input.GetKeyDown (KeyCode.T) && (yaw != 0.0f && pitch != 0.0f && roll != 0.0f))
			KillShipRotation ();

		Debug.DrawRay(transform.position, UpRay.position - transform.position, Color.blue); // DEBUG
		Debug.DrawRay(transform.position, FrontRay.position - transform.position, Color.blue); // DEBUG
		Debug.DrawRay(transform.position, LeftRay.position - transform.position, Color.blue); // DEBUG
	}
	
	// FixedUpdate
	void FixedUpdate () {
	//	float shipVelocity = GetShipVelocity ();

		Vector3 gravityVector = Gravity ();
		GetComponent<Rigidbody>().AddForce (gravityVector, ForceMode.Acceleration);
		//gameObject.transform.position.x
	}

	//Returns the acceleration due to gravity
	public Vector3 Gravity () {
		float sqMag = gameObject.transform.position.sqrMagnitude;
		float grav = ((GRAVITATIONAL_CONSTANT * planet.GetComponent<Rigidbody>().mass) / sqMag);
		Vector3 ForceApplied = GetComponent<Rigidbody> ().position.normalized * -grav;

		return ForceApplied;
	}

	// Gets the absoulte ship velocity (Sqrt(x^2 + y^2 + z^2)) where x, y, and z are the velocity components
	public float GetShipVelocity () {
		float xVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).x;
		float yVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).y;
		float zVel = transform.InverseTransformDirection (GetComponent<Rigidbody> ().velocity).z;
		Vector3 velocityVector = new Vector3 (xVel, yVel, zVel);
	
		return Mathf.Sqrt(velocityVector.sqrMagnitude);
	}

	public void ChangeTimeScale () {
		if (Input.GetKeyDown(KeyCode.Period)) {
			if (Time.timeScale >= 8.0f)
				Time.timeScale = 8.0f;
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

	public void ChangeShipOrientation () {

		float changeYPR = 1.0f;

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

		Debug.Log (roll);

		transform.Rotate (Time.deltaTime * pitch, Time.deltaTime * yaw, Time.deltaTime * roll);
	}

	public void KillShipRotation()
	{
			float dampenSpeed = .05f;

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
	}

}








