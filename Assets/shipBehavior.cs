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

	int[] warpScale;
	int warpPtr;


	#endregion


	#region KEPLARIAN ELEMENTS

	Vector3 angMomentum;			// orbital angular momentum of the ship around the planet
	Vector3 ASCNodeVector;			// vector pointing towards the ascending node
	Vector3 eccentricityVector;		// describes the shape of the orbit
	Vector3 referenceVector;		// unit vector (0, 0, 1)

	double SGP;						// standard gravitational parameter
	double ME;						// specific mechanical energy
	float periapsis;				// height of the lowest point in orbit
	float apoapsis;					// height of the highest point in orbit
	float eccentricity;				// magnitude of the eccentricity vector
	float semiMajorAxis;			// half of the major axis
	float semiLatusRectum;			// half the chord through focus, perpendicular to the semi major axis
	float inclination = 0;				// angle between satellite orbital plane and equitorial plane
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

		warpScale = new int[6];
		warpPtr = 0;
		warpScale [0] = 1;
		warpScale [1] = 5;
		warpScale [2] = 10;
		warpScale [3] = 25;
		warpScale [4] = 50;
		warpScale [5] = 100;

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

		//getEccentricity (); 							// eccentricity works
		Debug.Log ("eccentricity: " + eccentricity);
		Debug.Log ("eccentricity vec: " + eccentricityVector);

		//getMechanicalEnergy ();						// ME works
		//Debug.Log (ME);

		//getSemiMajorAxis ();							// SMA works
		//Debug.Log ("sma: " + semiMajorAxis);

		getTrueAnomaly ();
		Debug.Log ("true anomaly: " + trueAnomaly);

		//getASCNodeLong ();							// ascnodelong works
		//Debug.Log ("ASCnode Vector: " + ASCNodeVector);
		//Debug.DrawRay (new Vector3 (0, 0, 0), ASCNodeVector, Color.blue, Mathf.Infinity);
		//Debug.Log ("ASCnodelong: " + longitudeAscNode);

		//getPeriapsis ();								// periapsis works
		//Debug.Log ("periapsis: " + periapsis);

		//getApoapsis ();								//apoapsis works
		//Debug.Log ("apoapsis: " + apoapsis);

		//getArgPeriapsis ();							// arg of periapsis work (we think, little testing done)
		//Debug.Log ("arg of periapsis: " + argPeriapsis);
	
		//getInclination ();								// inclination works
		//Debug.Log ("inclincation: " + inclination);
		//Debug.Log (angMomentum.y);
		//Debug.Log (angMomentum.magnitude);

		//getAngularMomentum ();							// angular momentum Works kinda
		//Debug.Log ("ang momentum: " + angMomentum);
		//Debug.Log ("y: " + angMomentum.y);
		//Debug.Log ("magnitude: " + angMomentum.magnitude);

		//Debug.Log ("x: " + satelliteBody.velocity.x);
		//Debug.Log ("y: " + satelliteBody.velocity.y);
		//Debug.Log ("z: " + satelliteBody.velocity.z);

		//Debug.Log ("x: " + satelliteBody.centerOfMass.x);
		//Debug.Log ("y: " + satelliteBody.centerOfMass.y);
		//Debug.Log ("z: " + satelliteBody.centerOfMass.z);
		//Debug.Log ("magnitude: " + satelliteBody.centerOfMass.magnitude);

		//Debug.Log ("x: " + satelliteBody.angularVelocity.x);
		//Debug.Log ("y: " + satelliteBody.angularVelocity.y);
		//Debug.Log ("z: " + satelliteBody.angularVelocity.z);

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

		if (Input.GetKeyDown (KeyCode.Period) && warpPtr < warpScale.Length - 1) {
			warpPtr++;
		}
		if (Input.GetKeyDown (KeyCode.Comma) && warpPtr > 0) {
			warpPtr--;
		}

		Time.timeScale = warpScale[warpPtr];

	}
	
	// ChangeShipOrientation - Handles the controls for rotation the ships yaw, pitch, and roll
	void ChangeShipOrientation () {

		float changeYPR = 20f;
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
		
		satelliteBody.AddRelativeTorque (new Vector3(pitch, yaw, roll));
	}


	// KillShipRotation - eliminates the current torque on the ship to stabilize the ship
	public void KillShipRotation()
	{
		float dampenSpeed = .985f;
		float stillShip = .055f;
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
		referenceVector = new Vector3 (1, 0, 0);

		// initalizting mu or the standard gravitational parameter
		SGP = GRAVITATIONAL_CONSTANT * (EARTH_MASS + SHIP_MASS);

		// init angular momentum
		getAngularMomentum ();

		// Calculates the node vector which is a vector pointing towards the ascending node
		getASCNodeVector ();

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
		angMomentum = Vector3.Cross (satelliteBody.position, satelliteBody.velocity);
	}

	// Calculates the eccentricity (shape of the orbit)
	// NOTE: e = 0, perfect circle; 0 < e < 1, ellipse; e = 1, parabola; e > 1, hyperbola
	void getEccentricity () {
		float shipVel = satelliteBody.velocity.magnitude;
		float shipAlt = satelliteBody.position.magnitude;

		eccentricityVector = ((satelliteBody.position * ((shipVel * shipVel) - (float)(SGP / (double)shipAlt))) -
		        (satelliteBody.velocity * (Vector3.Dot(satelliteBody.position, satelliteBody.velocity)))) / (float)SGP;
		eccentricity = eccentricityVector.magnitude;
	}

	void getMechanicalEnergy () {
		ME = ((satelliteBody.velocity.magnitude * satelliteBody.velocity.magnitude) / 2) -
			(float)(SGP / (double)satelliteBody.position.magnitude);
	}

	void getASCNodeVector () {
		getAngularMomentum ();
		//Debug.Log ("ang momentum: " + angMomentum);
		//Debug.Log ("ref vec: " + referenceVector);
		//ASCNodeVector = Vector3.Cross (referenceVector, angMomentum);
		ASCNodeVector = new Vector3 (angMomentum.z, 0, -angMomentum.x);
	}

	void getSemiMajorAxis () {
		getEccentricity ();
		getMechanicalEnergy ();

		if (eccentricity < 1f)
			semiMajorAxis = -(float)(SGP / (double)(2 * ME));
		else
			semiMajorAxis = Mathf.Infinity;
	}

	void getSemiLatusRectum () {
		getSemiMajorAxis ();
		getAngularMomentum ();

		if (eccentricity < 1)
			semiLatusRectum = semiMajorAxis * (1 - (eccentricity * eccentricity));
		else
			semiLatusRectum = (float)((double)(angMomentum.magnitude * angMomentum.magnitude) / SGP);
	}

	void getInclination () {
		getAngularMomentum ();
		//NOTE: uses the z component in a normal coordinate system
		inclination = Mathf.Acos(Mathf.Abs(angMomentum.y / angMomentum.magnitude));
		//inclination = Mathf.PI - inclination; // making it normal to a RH coordinate system
		inclination = inclination * Mathf.Rad2Deg;

	}

	void getASCNodeLong () {
		getASCNodeVector ();
		//Debug.Log ("ASCNodeVector: " + ASCNodeVector);
		//Debug.Log ("ASCNodeVec x: " + ASCNodeVector.x);

		if (ASCNodeVector.z >= 0f)
			longitudeAscNode = Mathf.Acos (ASCNodeVector.x / ASCNodeVector.magnitude);	
		else
			longitudeAscNode = (Mathf.PI * 2) - Mathf.Acos (ASCNodeVector.x / ASCNodeVector.magnitude);

		longitudeAscNode = (longitudeAscNode * Mathf.Rad2Deg);

	}

	void getArgPeriapsis () {
		getASCNodeVector ();
		getEccentricity ();

		argPeriapsis = Mathf.Acos ((Vector3.Dot (ASCNodeVector, eccentricityVector)) / 
			(ASCNodeVector.magnitude * eccentricityVector.magnitude));

		if (eccentricityVector.y < 0) {
			argPeriapsis = (Mathf.PI * 2) - argPeriapsis;
		}

		argPeriapsis = argPeriapsis * Mathf.Rad2Deg;
	}

	void getTrueAnomaly () {
		getEccentricity ();

		trueAnomaly = (Mathf.Acos ((Vector3.Dot (eccentricityVector, satelliteBody.position)) /
			(eccentricityVector.magnitude * satelliteBody.position.magnitude)));

		if (Vector3.Dot (satelliteBody.position, satelliteBody.velocity) < 0)
			trueAnomaly = (Mathf.PI * 2) - trueAnomaly;

		trueAnomaly = trueAnomaly * Mathf.Rad2Deg;
	}

	void getPeriapsis () {
		getSemiMajorAxis ();
		getEccentricity ();
		float tempSMA;
		if (eccentricity >= 1) {
			tempSMA = -(float)(SGP / (double)(2 * ME));
			periapsis = tempSMA * (1 - eccentricity); 
		}
		else
			periapsis = semiMajorAxis * (1 - eccentricity);
	}

	void getApoapsis () {
		getSemiMajorAxis ();
		getPeriapsis ();

		apoapsis = (2 * semiMajorAxis) - periapsis;
	}

	#endregion

	
} // end ship behavior








