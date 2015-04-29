using UnityEngine;
using System.Collections;

public class dummyShip : MonoBehaviour {

	double GRAVITATIONAL_CONSTANT = 6.67384 * Mathf.Pow (10, -11);
	double EARTH_MASS = 5.972 * Mathf.Pow (10, 15);

	public shipBehavior shipScript;

	public Rigidbody satelliteBod;
	public Rigidbody planet;
	public Canvas dummyCanvas;

	public GameObject vectors;
	public GameObject signs;

	public Transform shipForm;

	public Vector3 startPos;
	public Vector3 startVel;

	public ParticleSystem explosion;

	//public shipBehavior shipScript;

	public void dummyBegin () {
		explosion.Stop ();
		satelliteBod = GetComponent<Rigidbody> ();
		shipForm = transform;
		//startPos = new Vector3 (0, 0, -6771);
		satelliteBod.AddForce (startVel, ForceMode.VelocityChange);
	}

	// Use this for initialization
	void Start () {
		if (shipScript.state == shipBehavior.GameState.Start) {
			dummyBegin ();
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate () {
		if (shipScript.state == shipBehavior.GameState.Start) {
			ApplyGravity ();
		}
	}

	// ApplyGravity - calculates the force of gravity then applies the force as an acceleration to the ship
	void ApplyGravity () {
		// Calculate the gravitation force using Newton's Law of Universal Gravitation Gmm/(d^2)
		// NOTE: since ship mass is so small it is negligible, therefore the second m is ignored
		double gravTemp = ((GRAVITATIONAL_CONSTANT * EARTH_MASS) / (double)shipForm.position.sqrMagnitude);
		float grav = (float)gravTemp;
		
		Vector3 gravityVector = satelliteBod.position.normalized * -grav;  // normalize the position vector of the ship then apply grav
		satelliteBod.AddForce (gravityVector, ForceMode.Acceleration); 	// add the new force to the ship as an acceleration
	}

	void OnTriggerEnter(Collider other) {
		explosion.gameObject.SetActive (true);
		explosion.transform.position = gameObject.transform.position;
		explosion.Play ();
		dummyCanvas.enabled = false;
		gameObject.SetActive (false);

	}

	void OnCollisionEnter(Collision collision) {
		vectors.SetActive (false);
		signs.SetActive (false);
	}
}
