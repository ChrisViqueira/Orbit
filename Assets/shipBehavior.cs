using UnityEngine;
using System.Collections;

public class shipBehavior : MonoBehaviour {
	
	#region VARIABLES
	
	double GRAVITATIONAL_CONSTANT = 6.67384 * Mathf.Pow (10, -11);
	double EARTH_MASS = 5.972 * Mathf.Pow (10, 15);
	double SHIP_MASS = 1.1 * Mathf.Pow (10, 5);

	public ParticleSystem leftBoost;
	public ParticleSystem rightBoost;
	public ParticleSystem topBoost;

	/*
	public Transform planet;
	
	public Transform UpRay; 	// casting a ray from the bottom of the ship
	public Transform FrontRay; 	// casting a ray toward the front of the ship
	public Transform LeftRay;  // casting a ray right of the ship
	*/

	public float yaw = 0.0f;
	public float pitch = 0.0f;
	public float roll = 0.0f;
	public float thrust = 0.0f;
	
	bool killRotation = false;

	public Rigidbody satelliteBody;

	public Vector3 startPosition;
	public Vector3 startVelocity;


	#endregion


	#region KEPLARIAN ELEMENTS

	Vector3 angMomentum;			// orbital angular momentum of the ship around the planet
	Vector3 ASCNodeVector;			// vector pointing towards the ascending node
	Vector3 eccentricityVector;		// describes the shape of the orbit
	Vector3 referenceVector;		// unit vector (0, 0, 1)

	double SGP;						// standard gravitational parameter
	double ME;						// specific mechanical energy
	float eccentricity;				// magnitude of the eccentricity vector
	float semiMajorAxis;			// half of the major axis
	float semiLatusRectum;			// half the chord through focus, perpendicular to the semi major axis
	float inclination;				// angle between satellite orbital plane and equitorial plane
	float longitudeAscNode;			// angle between ship starting position and node vector
	float argPeriapsis;				// angle between ASCNodeVector and periapsis vector
	float trueAnomaly;				// angle between periapsis vector and position vector
									// NOTE: true anomaly measured in direction of the travel

	#endregion


	// Start - used for initilization at the start of the simulation
	void Start () {
		satelliteBody = GetComponent<Rigidbody> ();
		satelliteBody.maxAngularVelocity = 30;
		startPosition = transform.position;
		satelliteBody.AddForce (startVelocity, ForceMode.VelocityChange);

		/*Debug.Log ("pos mag: " + satelliteBody.position.magnitude);
		Debug.Log ("vel mag: " + startVelocity.magnitude);
		Debug.Log ("pos vec: " + satelliteBody.position);
		Debug.Log ("vel vec: " + startVelocity);
		*/

		initKeplarianElements ();
	}
	
	// Update - Called every frame
	void Update () {
		ChangeTimeScale ();
		KillShipRotation ();

		//debug

		//getEccentricity (); 
		//Debug.Log (eccentricity);

		//getMechanicalEnergy ();
		//Debug.Log (ME);

		//getTrueAnomaly ();
		//Debug.Log (trueAnomaly);

		getInclination ();
		Debug.Log (inclination);

		getAngularMomentum ();
		Debug.Log (angMomentum);

		//debug end

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
		double gravTemp = ((GRAVITATIONAL_CONSTANT * EARTH_MASS) / (double)transform.position.sqrMagnitude);
		float grav = (float)gravTemp;
		
		Vector3 gravityVector = satelliteBody.position.normalized * -grav;  // normalize the position vector of the ship then apply grav
		satelliteBody.AddForce (gravityVector, ForceMode.Acceleration); 	// add the new force to the ship as an acceleration
	}
	
	// ChangeTimeScale - allows the simulation to speed up, helpful for longer and bigger orbits
	public void ChangeTimeScale () {

		float maxTimeScale = 100.0f;
		
		if (Input.GetKeyDown(KeyCode.Period)) {
			if (Time.timeScale > maxTimeScale)
				Time.timeScale = maxTimeScale;
			else
				Time.timeScale *= 5.0f;
		}
		if (Input.GetKeyDown(KeyCode.Comma)) {
			if (Time.timeScale <= 1.0f)
				Time.timeScale = 1.0f;
			else
				Time.timeScale /= 5.0f;
		}

	}
	
	// ChangeShipOrientation - Handles the controls for rotation the ships yaw, pitch, and roll
	void ChangeShipOrientation () {

		//satelliteBody.centerOfMass = transform.position;
		//Debug.Log (satelliteBody.centerOfMass);

		float changeYPR = 20f;
		yaw = 0.0f;
		pitch = 0.0f;
		roll = 0.0f;
		//satelliteBody.angularVelocity = Vector3.zero;
		
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
		
		satelliteBody.AddRelativeTorque (new Vector3(pitch, yaw, roll));// * satelliteBody.mass);
		/*

		float YPRforce = .00001f;

		pitch = Input.GetAxis ("Vertical") * YPRforce * Time.deltaTime;

		if (Input.GetKey (KeyCode.A))
			satelliteBody.AddRelativeTorque (-Vector3.up * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.D))
			satelliteBody.AddRelativeTorque (Vector3.up * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.W))
			satelliteBody.AddRelativeTorque (-Vector3.right * pitch * satelliteBody.mass);
		if (Input.GetKey (KeyCode.S))
			satelliteBody.AddRelativeTorque (Vector3.right * pitch * satelliteBody.mass);
		if (Input.GetKey (KeyCode.Q))
			satelliteBody.AddRelativeTorque (-Vector3.forward * YPRforce * satelliteBody.mass);
		if (Input.GetKey (KeyCode.E))
			satelliteBody.AddRelativeTorque (Vector3.forward * YPRforce * satelliteBody.mass);

		Debug.Log ("x: " + satelliteBody.angularVelocity.x);
		Debug.Log ("y: " + satelliteBody.angularVelocity.y);
		Debug.Log ("z: " + satelliteBody.angularVelocity.z);
		Debug.Log ("magnitude: " + satelliteBody.angularVelocity.magnitude);
		*/
	}


	// KillShipRotation - eliminates the current torque on the ship to stabilize the ship
	public void KillShipRotation()
	{
		float dampenSpeed = .985f;
		float stillShip = .005f;
		if (Input.GetKey (KeyCode.T) && !killRotation)
			killRotation = true;

		if (killRotation) {
			satelliteBody.angularVelocity *= dampenSpeed;

			if (Mathf.Abs(satelliteBody.angularVelocity.magnitude) < stillShip){
				satelliteBody.angularVelocity = Vector3.zero;
				killRotation = false;
			}
		}

		// Debug.Log (satelliteBody.angularVelocity.magnitude); // USED FOR DEBUG
	}

	// flameOn - toggle the particle systems
	void flameOn () {
		rightBoost.Play ();
		leftBoost.Play ();
		topBoost.Play ();
	}

	// flameOff - toggle the particle systems
	void flameOff () {
		rightBoost.Stop ();
		leftBoost.Stop ();
		topBoost.Stop ();
	}

	
	// AddSatelliteVelocity - Adds velocity directly forward from the front of the satellite
	public void AddSatelliteVelocity()
	{
		float thrustIncrease = .01f;
		if (Input.GetKeyDown (KeyCode.H))
			flameOn ();

		if (Input.GetKey (KeyCode.H))
			thrust += thrustIncrease;
		else {
			thrust = 0.0f;
			flameOff ();
		}

		satelliteBody.AddForce (-transform.forward * thrust * Time.deltaTime, ForceMode.VelocityChange);
	}

	/***************************CHECK ALL OF THESE FUNCTIONS********************************/
	#region KEPLARIAN FUNCTIONS

	void initKeplarianElements() {
		// setting the reference direction
		referenceVector = new Vector3 (0, 0, 1);

		// initalizting mu or the standard gravitational parameter
		SGP = GRAVITATIONAL_CONSTANT * (EARTH_MASS + SHIP_MASS);

		// init angular momentum
		getAngularMomentum ();

		// Calculates the node vector which is a vector pointing towards the ascending node
		ASCNodeVector = Vector3.Cross (referenceVector, angMomentum);

		// init eccentricity
		getEccentricity ();

		//init mechanical energy
		getMechanicalEnergy ();

		// init semiMajorAxis
		getSemiMajorAxis ();

		// init semiLatusRectum
		getSemiLatusRectum ();

		// init inclination
		getInclination ();

		// init ascending node
		getASCNodeLong ();

		// init argument of periapsis
		getArgPeriapsis ();

		// init true anomaly
		getTrueAnomaly ();
	}

	// Calculates the orbital angular momentum of the satellite around the planet
	void getAngularMomentum () {
		angMomentum = Vector3.Cross (satelliteBody.position, startVelocity);
	}

	// Calculates the eccentricity (shape of the orbit)
	// NOTE: e = 0, perfect circle; 0 < e < 1, ellipse; e = 1, parabola; e > 1, hyperbola
	void getEccentricity () {
		float shipVel = satelliteBody.velocity.magnitude;
		float shipAlt = satelliteBody.position.magnitude;

		eccentricityVector = ((((shipVel * shipVel) - (float)(SGP / (double)shipAlt)) * satelliteBody.position) -
		        ((Vector3.Dot (satelliteBody.position, startVelocity)) * startVelocity)) / (float)SGP;
		eccentricity = eccentricityVector.magnitude;
	}

	void getMechanicalEnergy () {
		ME = ((satelliteBody.velocity.magnitude * satelliteBody.velocity.magnitude) / 2) /
			(float)(SGP / (double)satelliteBody.position.magnitude);
	}

	void getSemiMajorAxis () {
		getMechanicalEnergy ();

		if (eccentricity < 1)
			semiMajorAxis = -(float)(SGP / (double)(2 * ME));
		else
			semiMajorAxis = Mathf.Infinity;
	}

	void getSemiLatusRectum () {
		getSemiMajorAxis ();

		if (eccentricity < 1)
			semiLatusRectum = semiMajorAxis * (1 - (eccentricity * eccentricity));
		else
			semiLatusRectum = (float)((double)(angMomentum.magnitude * angMomentum.magnitude) / SGP);
	}

	void getInclination () {
		//NOTE: uses the z component in a normal coordinate system
		inclination = Mathf.Acos(angMomentum.y / angMomentum.magnitude);
	}

	void getASCNodeLong () {
		longitudeAscNode = Mathf.Acos (ASCNodeVector.x / ASCNodeVector.magnitude);
	}

	void getArgPeriapsis () {
		argPeriapsis = Mathf.Acos ((Vector3.Dot (ASCNodeVector, eccentricityVector)) / 
			(ASCNodeVector.magnitude * eccentricityVector.magnitude));
	}

	void getTrueAnomaly () {
		trueAnomaly = (Mathf.Acos ((Vector3.Dot (eccentricityVector, satelliteBody.position)) /
			(eccentricityVector.magnitude * satelliteBody.position.magnitude)));
	}

	#endregion

	
} // end ship behavior








