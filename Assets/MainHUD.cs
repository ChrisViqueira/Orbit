using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainHUD : MonoBehaviour {

	public GameObject satellite;
	public shipBehavior shipScript;

	public Text orbitalElements;

	public Text warpScale;
	public Text shipAlt;
	public Text shipVel;
	public Text apoapsis;
	public Text periapsis;
	public Text timeApoapsis;
	public Text timePeriapsis;
	public Text inclination;
	public Text timeASC;
	public Text timeDESC;
	public Text eccentricity;
	public Text trueAnomaly;
	public Text passing;
	public Text orbPeriod;


	// Use this for initialization
	void Start () {
		warpScale.text = "warp x" + Time.timeScale;
		//satellite = GameObject.Find ("satellite");
		//shipScript = satellite.GetComponent<shipBehavior> ();
	}
	
	// Update is called once per frame
	void Update () {
		displayHUD ();
	}

	void displayHUD () {

		float shipHeight = shipScript.satelliteBody.position.magnitude - shipScript.PLANET_RADIUS;
		float actualAPA = shipScript.apoapsis - shipScript.PLANET_RADIUS;	// actual apoapsis
		float actualPEA = shipScript.periapsis - shipScript.PLANET_RADIUS;	// actual periapsis

		warpScale.text = "warp x" + Time.timeScale;
		shipAlt.text = "ship altitude: " + shipHeight + " km";
		shipVel.text = "ship velocity: " + shipScript.satelliteBody.velocity.magnitude + " km/sec";
		apoapsis.text = "apoapsis: " + actualAPA + " km";
		periapsis.text = "periapsis: " + actualPEA + " km";
		timeApoapsis.text = "time to apoapsis: " + shipScript.time2Apoapsis + " sec";
		timePeriapsis.text = "time to periapsis: " + shipScript.time2Periapsis + " sec";
		inclination.text = "inclination: " + shipScript.inclination + " degrees";
		timeASC.text = "time ASC: ";
		timeDESC.text = "time DESC: ";
		eccentricity.text = "eccentricity: " + shipScript.eccentricity;
		trueAnomaly.text = "true anomaly: " + shipScript.trueAnomaly + " degrees";
		orbPeriod.text = "orbital period: " + shipScript.orbitalPeriod + " sec";


//		orbitalElements.text = shipAlt + shipVel;
	}


	//dedicated to hiding the text
	void timeWarpText () {
		warpScale.text = string.Empty;
		Debug.Log ("timeWarpText called");
	}
}
