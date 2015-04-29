using UnityEngine;
using System.Collections;

public class shipBehavior : MonoBehaviour {
	
	#region VARIABLES

	public enum GameState {
		Start,
		Stop
	}
	
	public GameState state;

	public dummyShip dummyScript;
	public restartBoxScript boxScript;

	public GameObject startBox;
	public GameObject vectors;
	public GameObject signs;

	public bool settingPrograde = false;
	public bool resetCalled = false;
	
	double GRAVITATIONAL_CONSTANT = 6.67384 * Mathf.Pow (10, -11);
	double EARTH_MASS = 5.972 * Mathf.Pow (10, 15);
	double SHIP_MASS = 1.1 * Mathf.Pow (10, 5);

	public float PLANET_RADIUS = 6371f;	// measured in km
	
	//main thrust
	public ParticleSystem leftBoost;
	public ParticleSystem rightBoost;
	public ParticleSystem topBoost;

	// vectors to various points
	public LineRenderer periLine;
	public LineRenderer apoLine;
	public LineRenderer ASCLine;
	public LineRenderer DESCLine;

	public float yaw = 0.0f;
	public float pitch = 0.0f;
	public float roll = 0.0f;
	public float thrust;
	public float thrustTrans;
	
	bool killRotation = false;

	// for setting prograde
	//bool isPrograde = true;
	//float setProSpeed = .1f;

	public Rigidbody satelliteBody;
	public Rigidbody planet;

	public Vector3 startPosition;
	public Vector3 startVelocity;

	int[] warpScale;
	int warpPtr;

	Transform shipform;


	#endregion


	#region KEPLARIAN ELEMENTS

	Vector3 transDir;
	Vector3 angMomentum;			// orbital angular momentum of the ship around the planet
	public Vector3 ASCNodeVector;			// vector pointing towards the ascending node
	public Vector3 eccentricityVector;		// describes the shape of the orbit
	//Vector3 referenceVector;		// unit vector (0, 0, 1)

	double SGP;						// standard gravitational parameter
	double ME;						// specific mechanical energy
	float eccentricAnomaly;			// position along an elliptical orbit relative to circular orbit
	float meanAnomaly;				// relates position and time in a keplarian orbit
	float deltaAnomaly;				// the change in mean anomaly
	float parabolicAnomaly;
	float meanMotion;				// mean motion
	public float time2Apoapsis;			// time 2 apoapsis
	public float time2Periapsis;			// time to periapsis
	//float tempT2P;
	public float timeSincePeriapsis;
	public float orbitalPeriod;			// time to make a complete orbit
	public float periapsis;			// height of the lowest point in orbit
	public float apoapsis;			// height of the highest point in orbit
	public float eccentricity;		// magnitude of the eccentricity vector
	public float semiMajorAxis;			// half of the major axis
	float semiLatusRectum;			// half the chord through focus, perpendicular to the semi major axis
	public float semiMinorAxis;			// half of minor axis
	public float inclination;	// angle between satellite orbital plane and equitorial plane
	float longitudeAscNode;			// angle between ship starting position and node vector
	//public float ASCNodeMag;				//magnitude of the asc node vector
	//public float DESCNodeMag;				// magnitude of desc node vector
	float argPeriapsis;				// angle between ASCNodeVector and periapsis vector
	public float trueAnomaly;		// angle between periapsis vector and position vector
									// NOTE: true anomaly measured in direction of the travel

	#endregion


	// Start - used for initilization at the start of the simulation
	void Start () {
		satelliteBody = GetComponent<Rigidbody> ();
		shipform = transform;
		satelliteBody.maxAngularVelocity = 30;

		warpScale = new int[6];
		warpPtr = 0;
		warpScale [0] = 1;
		warpScale [1] = 5;
		warpScale [2] = 10;
		warpScale [3] = 25;
		warpScale [4] = 50;
		warpScale [5] = 100;

		//Time.timeScale = 0;
		//startPosition = new Vector3 (6771, 0, 0);

		// setting the reference direction
		//referenceVector = new Vector3 (1, 0, 0);
		
		// initalizting mu or the standard gravitational parameter
		SGP = GRAVITATIONAL_CONSTANT * (EARTH_MASS + SHIP_MASS);

		mainThrustOff ();
		//if(state == GameState.Start)
		//	shipBegin ();
	}

	public void shipBegin () {
		//Debug.Log ("shipbegin called");
		startPosition = new Vector3 (6771, 0, 0);
		//Debug.Log ("b4 add force " + satelliteBody.velocity);
		satelliteBody.AddForce (startVelocity, ForceMode.VelocityChange);
	}

	// Update - Called every frame
	void Update () {
		//Debug.Log (startPosition);
		//Debug.Log ("dummy vel " + dummyScript.satelliteBod.velocity);
		//Debug.Log ("startvel " + startVelocity);
		//Debug.Log ("pos " + satelliteBody.position);
		//Debug.Log ("ship vel" + satelliteBody.velocity);
		//Debug.Log ("ang " + angMomentum);
		//Debug.Log ("asc " + ASCNodeVector);
		//Debug.Log ("ecc " + eccentricityVector);
		//Debug.Log ("sgp " + SGP);
		//Debug.Log ("me " + ME);
		//Debug.Log ("time: " + Time.timeScale);



		if (state == GameState.Start) {
			ChangeTimeScale ();
			getKeplarianElements ();
			drawKeplarianElements ();
			if (Input.GetKeyDown (KeyCode.R) && !resetCalled) {
				//Time.timeScale = 0;
				startVelocity = Vector3.zero;
				startBox.SetActive(true);
				state = shipBehavior.GameState.Stop;
				satelliteBody.velocity = Vector3.zero;
				dummyScript.satelliteBod.velocity = Vector3.zero;
				satelliteBody.angularVelocity = Vector3.zero;
				dummyScript.satelliteBod.angularVelocity = Vector3.zero;
			}
			if (boxScript.clicked){
				//Debug.Log("click vel: " + startVelocity);
				shipBegin ();
				resetShips ();
				resetSimulation ();
			}
			/*
			Debug.Log ("velocity norm: " + satelliteBody.velocity.normalized);
			Debug.Log ("ship forward norm: " + -shipform.forward.normalized);
			*/
		}

	}
	
	// FixedUpdate - called every fixed framerate or every physics step, eliminates error from gravity calculations
	void FixedUpdate () {
		if (state == GameState.Start) {
			ApplyGravity ();

			if (Time.timeScale == 1) { // if in time warp, put ship on rails, eliminates a lot of lag
				rotateShip ();
				moveShip ();

			}
		}
	}

	void rotateShip () {
		/*
		if (Input.GetKeyDown (KeyCode.P) && isPrograde) { //set to prograde
			satelliteBody.angularVelocity = Vector3.zero;
			isPrograde = false;
		}
		if (!isPrograde && !settingPrograde) {
			StartCoroutine ("setPrograde");
			Debug.Log ("called set prograde");
		}
		*/

		ChangeShipOrientation ();
		KillShipRotation ();
	}

	void moveShip () {
		AddSatelliteVelocity ();
		translateSatellite ();
	}

	// ApplyGravity - calculates the force of gravity then applies the force as an acceleration to the ship
	void ApplyGravity () {
		// Calculate the gravitation force using Newton's Law of Universal Gravitation Gmm/(d^2)
		// NOTE: since ship mass is so small it is negligible, therefore the second m is ignored
		double gravTemp = ((GRAVITATIONAL_CONSTANT * EARTH_MASS) / (double)shipform.position.sqrMagnitude);
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

		float changeYPR = .1f;
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
	}


	#region TOGGLE FLAMES

	// mainThrustOn - toggle the particle systems
	void mainThrustOn () {
		rightBoost.Play ();
		leftBoost.Play ();
		topBoost.Play ();
	}

	// mainThrustOff - toggle the particle systems
	void mainThrustOff () {
		rightBoost.Stop ();
		leftBoost.Stop ();
		topBoost.Stop ();
	}
	

	#endregion

	
	// AddSatelliteVelocity - Adds velocity directly forward from the front of the satellite
	public void AddSatelliteVelocity()
	{
		if (Input.GetKeyDown (KeyCode.Z))
			mainThrustOn ();

		if (Input.GetKey (KeyCode.Z)) {
			satelliteBody.AddForce (-shipform.forward * thrust * Time.deltaTime, ForceMode.VelocityChange);
		}
		else {
			mainThrustOff ();
		}
	}

	/*************************************************************************************/

	public void translateSatellite () {
		transDir = Vector3.zero;

		////////////////////////Movement//////////////////////////

		if (Input.GetKey (KeyCode.K)) {	// translate up
			transDir = shipform.up;
		}
		if (Input.GetKey (KeyCode.I)) { // translate down
			transDir = -shipform.up;
		}
		if (Input.GetKey (KeyCode.J)) { // translate left
			transDir = shipform.right;
		}
		if (Input.GetKey (KeyCode.L)) { // translate right
			transDir = -shipform.right;
		}
		if (Input.GetKey (KeyCode.H)) { // translate forward
			transDir = -shipform.forward;
		}
		if (Input.GetKey (KeyCode.N)) { // translate back
			transDir = shipform.forward;
		}

		////////////////////////////////////////////////////////////

		satelliteBody.AddForce (transDir * thrustTrans * Time.deltaTime, ForceMode.VelocityChange);

	}

	// sets the ship in the direction of the velocity vector
	/*public IEnumerator setPrograde() {
		settingPrograde = true;
		Quaternion shipTrans = shipform.rotation;
		Quaternion endRot = Quaternion.FromToRotation(shipform.forward, satelliteBody.velocity);

		//Vector3 endEuler = endRot.eulerAngles;
		//endEuler.z = 0;
		//endRot = Quaternion.Euler (endEuler);

		for (int x = 0; x < 1000; x++) {

			if (x < 400)
				x++;

			shipform.rotation = Quaternion.Lerp(shipTrans, endRot, x/1000.0f);
			yield return new WaitForFixedUpdate();
		}

		isPrograde = true;
		settingPrograde = false;
	
	}*/


	public void resetShips () {
		satelliteBody.rotation = new Quaternion (0, 1, 0, 0);
		satelliteBody.velocity = Vector3.zero;
		satelliteBody.angularVelocity = Vector3.zero;
		satelliteBody.position = startPosition;
		startVelocity = boxScript.vel;

		//Debug.Log (startPosition);
		dummyScript.satelliteBod.velocity = Vector3.zero;
		dummyScript.satelliteBod.angularVelocity = Vector3.zero;
		dummyScript.satelliteBod.position = dummyScript.startPos;
	}

	// resets the simulation to the original starting values of a (basically) circular orbit
	public void resetSimulation () {
		//Debug.Log ("reset called");
		dummyScript.explosion.gameObject.SetActive (false);
		dummyScript.dummyCanvas.enabled = true;
		//dummyScript.gameObject.SetActive (true);

		//resetShips ();

		angMomentum = Vector3.zero;
		ASCNodeVector = Vector3.zero;
		//eccentricityVector = Vector3.zero;
		//referenceVector = Vector3.zero
							
		ME = 0;						
		eccentricAnomaly = 0.0f;
		meanAnomaly = 0.0f;			
		deltaAnomaly = 0.0f;				
		parabolicAnomaly = 0.0f;
		meanMotion = 0.0f;				
		time2Apoapsis = 0.0f;			
		time2Periapsis = 0.0f;			
		//tempT2P = 0.0f;
		timeSincePeriapsis = 0.0f;
		orbitalPeriod = 0.0f;			
		periapsis = 0.0f;			
		apoapsis = 0.0f;			
		eccentricity = 0.0f;		
		semiMajorAxis = 0.0f;			
		semiLatusRectum = 0.0f;			
		inclination = 0.0f;	
		longitudeAscNode = 0.0f;			
		argPeriapsis = 0.0f;				
		trueAnomaly = 0.0f;
		semiMinorAxis = 0.0f;
		//ASCNodeMag = 0.0f;
		//DESCNodeMag = 0.0f;

		boxScript.clicked = false;

		vectors.SetActive (true);
		signs.SetActive (true);
	

		resetCalled = false;

		//Time.timeScale = 1;
		warpPtr = 0;
	}

	/***************************CHECK ALL OF THESE FUNCTIONS********************************/
	#region KEPLARIAN FUNCTIONS

	void getKeplarianElements() {
		//Debug.Log ("called");
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

		// init semiMinorAxis
		getSemiMinorAxis ();

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

		// init eccentric anomaly
		getEccentricAnomaly ();

		// init mean anomaly
		getMeanAnomaly ();

		//init get true anomaly
		getParabolicAnomaly ();

		// init mean motion
		getMeanMotion ();

		// init time to apoapsis
		getTime2Apoapsis ();

		// init orbital period
		getOrbitalPeriod ();

		// init time since periapsis
		getTimeSincePeriapsis ();

		// init time to periapsis
		getTime2Periapsis ();

		// init periapsis
		getPeriapsis ();

		// init apoapsis;
		getApoapsis ();

		//init nodemag
		//getNodeMag ();
	}

	#region DRAW KEPLARIAN ELEMENTS

	void drawKeplarianElements () {
		periLine.SetPosition (1, eccentricityVector.normalized * periapsis);
		apoLine.SetPosition (1, eccentricityVector.normalized * -apoapsis);
		ASCLine.SetPosition (1, ASCNodeVector.normalized * 8000);
		DESCLine.SetPosition (1, -ASCNodeVector.normalized * 8000);
		//Debug.Log (ASCNodeVector.magnitude);
	}

	#endregion


		#region GET KEPLARIAN ELEMENTS

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
			//getAngularMomentum ();
			//Debug.Log ("ang momentum: " + angMomentum);
			//Debug.Log ("ref vec: " + referenceVector);
			//ASCNodeVector = Vector3.Cross (referenceVector, angMomentum);
			ASCNodeVector = new Vector3 (angMomentum.z, 0, -angMomentum.x);
		}

		void getSemiMajorAxis () {
			//getEccentricity ();
			//getMechanicalEnergy ();

			if (eccentricity < 1f)
				semiMajorAxis = -(float)(SGP / (double)(2 * ME));
			else
				semiMajorAxis = Mathf.Infinity;
		}

		void getSemiLatusRectum () {
			//getSemiMajorAxis ();
			//getAngularMomentum ();

			if (eccentricity < 1)
				semiLatusRectum = semiMajorAxis * (1 - (eccentricity * eccentricity));
			else
				semiLatusRectum = (float)((double)(angMomentum.magnitude * angMomentum.magnitude) / SGP);
		}

		void getInclination () {
			//getAngularMomentum ();
			//NOTE: uses the z component in a normal coordinate system
			inclination = Mathf.Acos(Mathf.Abs(angMomentum.y / angMomentum.magnitude));
			//inclination = Mathf.PI - inclination; // making it normal to a RH coordinate system
			inclination = inclination * Mathf.Rad2Deg;

		}

		void getASCNodeLong () {
			//getASCNodeVector ();
			//Debug.Log ("ASCNodeVector: " + ASCNodeVector);
			//Debug.Log ("ASCNodeVec x: " + ASCNodeVector.x);

			if (ASCNodeVector.z >= 0f)
				longitudeAscNode = Mathf.Acos (ASCNodeVector.x / ASCNodeVector.magnitude);	
			else
				longitudeAscNode = (Mathf.PI * 2) - Mathf.Acos (ASCNodeVector.x / ASCNodeVector.magnitude);

			longitudeAscNode = (longitudeAscNode * Mathf.Rad2Deg);

		}

		void getArgPeriapsis () {
			//getASCNodeVector ();
			//getEccentricity ();

			argPeriapsis = Mathf.Acos ((Vector3.Dot (ASCNodeVector, eccentricityVector)) / 
				(ASCNodeVector.magnitude * eccentricityVector.magnitude));

			if (eccentricityVector.y < 0) {
				argPeriapsis = (Mathf.PI * 2) - argPeriapsis;
			}

			//argPeriapsis = argPeriapsis * Mathf.Rad2Deg;
		}

		void getTrueAnomaly () {
			//getEccentricity ();

			trueAnomaly = (Mathf.Acos ((Vector3.Dot (eccentricityVector, satelliteBody.position)) /
				(eccentricityVector.magnitude * satelliteBody.position.magnitude)));

			if (Vector3.Dot (satelliteBody.position, satelliteBody.velocity) < 0)
				trueAnomaly = (Mathf.PI * 2) - trueAnomaly;

			trueAnomaly = trueAnomaly * Mathf.Rad2Deg;
		}

		void getPeriapsis () {
			//getSemiMajorAxis ();
			//getEccentricity ();
			float tempSMA;
			if (eccentricity >= 1) {
				tempSMA = -(float)(SGP / (double)(2 * ME));
				periapsis = tempSMA * (1 - eccentricity); 
			}
			else
				periapsis = semiMajorAxis * (1 - eccentricity);
		}

		void getApoapsis () {
			//getSemiMajorAxis ();
			//getPeriapsis ();
			apoapsis = (2 * semiMajorAxis) - periapsis;
		}

		void getEccentricAnomaly () {
			eccentricAnomaly = 2 * Mathf.Atan (Mathf.Sqrt ((1 - eccentricity) / (1 + eccentricity)) * 
				Mathf.Tan ((trueAnomaly * Mathf.Deg2Rad) / 2));
		}

		void getMeanAnomaly () {
			meanAnomaly = eccentricAnomaly - (eccentricity * Mathf.Sin (eccentricAnomaly));
			deltaAnomaly = Mathf.PI - meanAnomaly;
		}

		void getParabolicAnomaly () {
			parabolicAnomaly = Mathf.Tan (trueAnomaly / 2);
		}

		void getMeanMotion () {
			meanMotion = Mathf.Sqrt ((float)(SGP / Mathf.Pow (semiMajorAxis, 3)));
		}

		void getTime2Apoapsis () {
			time2Apoapsis = deltaAnomaly / meanMotion;
		}

		void getOrbitalPeriod () {
			orbitalPeriod = 2 * Mathf.PI * Mathf.Sqrt ((float)((double)Mathf.Pow (semiMajorAxis, 3) / SGP));
			
		}

		void getTime2Periapsis () {
			if (meanAnomaly > 0)
				time2Periapsis = orbitalPeriod - timeSincePeriapsis;
			else
				time2Periapsis = (timeSincePeriapsis * -1);
		}

		void getTimeSincePeriapsis () {

			if (eccentricity >= 1) 
				timeSincePeriapsis = .5f * Mathf.Sqrt((float)((double) Mathf.Pow(semiLatusRectum, 3) / SGP)) *
					(parabolicAnomaly + (Mathf.Pow(parabolicAnomaly, 3) / 3));
			else
				timeSincePeriapsis = Mathf.Sqrt ((float)((double)Mathf.Pow (semiMajorAxis, 3) / SGP)) * meanAnomaly;

		}

	void getSemiMinorAxis () {
		semiMinorAxis = semiMajorAxis * Mathf.Sqrt (1 - (eccentricity * eccentricity));
	}

	/*void getNodeMag () {
		float theta = (2 * Mathf.PI) - argPeriapsis;

		ASCNodeMag = (semiMajorAxis * semiMinorAxis) / 
			(Mathf.Sqrt ( ( (semiMajorAxis * semiMajorAxis) * Mathf.Pow (Mathf.Sin (theta), 2) ) + 
			( (semiMinorAxis * semiMinorAxis) * Mathf.Pow (Mathf.Cos (theta), 2 ) ) ) );

		DESCNodeMag = (semiMajorAxis * semiMinorAxis) / 
			(Mathf.Sqrt ( ( (semiMajorAxis * semiMajorAxis) * Mathf.Pow (Mathf.Sin (argPeriapsis), 2) ) + 
			( (semiMinorAxis * semiMinorAxis) * Mathf.Pow (Mathf.Cos (argPeriapsis), 2 ) ) ) );
	}*/

		#endregion

	#endregion

	
} // end ship behavior





