using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class targetScript : MonoBehaviour {

	public shipBehavior shipScript;
	public Text target;
	public Rigidbody satelliteBody;
	public Transform dummyForm;

	// Use this for initialization
	void Start () {
		target.text = target.name;
	}
	
	// Update is called once per frame
	void Update () {
		dummyForm.position = satelliteBody.position;
		dummyForm.Translate (new Vector3 (0, 10, 0));
		transform.LookAt (shipScript.satelliteBody.position);
	}
}
