using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class envCanvas : MonoBehaviour {

	public shipBehavior shipScript;
	public Text message;
	public Transform signForm;

	const int SIGN_APA = 1;
	const int SIGN_PEA = 2;
	const int SIGN_ASC = 3;
	const int SIGN_DESC = 4;

	int signPosted = 0;


	// Use this for initialization
	void Start () {
		signForm = transform;
		message.text = message.name;

		if (message.name == "PeA")
			signPosted = SIGN_PEA;
		if (message.name == "ApA")
			signPosted = SIGN_APA;
		if (message.name == "ASC")
			signPosted = SIGN_ASC;
		if (message.name == "DESC")
			signPosted = SIGN_DESC;
	}
	
	// Update is called once per frame
	void Update () {
		float avg = (shipScript.semiMajorAxis + shipScript.semiMinorAxis) / 2;
		if (signPosted == SIGN_APA)
			signForm.position = shipScript.eccentricityVector.normalized * -(shipScript.apoapsis + 600);
		else if (signPosted == SIGN_PEA)
			signForm.position = shipScript.eccentricityVector.normalized * (shipScript.periapsis + 600);
		else if (signPosted == SIGN_ASC) {
			signForm.position = shipScript.ASCNodeVector.normalized * (avg + 600);
			signForm.Translate(new Vector3 (0, 100, 0));
		} else if (signPosted == SIGN_DESC) {
			signForm.position = -shipScript.ASCNodeVector.normalized * (avg + 600);
			signForm.Translate(new Vector3 (0, 100, 0));
		}

		signForm.LookAt (shipScript.satelliteBody.position);
	}
}
