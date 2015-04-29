using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class restartBoxScript : MonoBehaviour {

	public shipBehavior shipScript;

	//public Rigidbody ship;

	public InputField xComp;
	public InputField yComp;
	public InputField zComp;

	public Button reset;

	public Toggle crash;

	public Vector3 vel;

	public bool clicked = false;

	// Use this for initialization
	void Start () {
		reset.onClick.AddListener (resetSimClick);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void resetSimClick () {
		//Debug.Log ("click");
		clicked = true;
		if (xComp.text == string.Empty)
			xComp.text = "0";
		if (yComp.text == string.Empty)
			yComp.text = "0";
		if (zComp.text == string.Empty)
			zComp.text = "0";

		float number = 0;
		if (float.TryParse(xComp.text, out number))
		{
			vel.x = number;
		} 
		if (float.TryParse(yComp.text, out number))
		{
			vel.y = number;
		} 
		if (float.TryParse(zComp.text, out number))
		{
			vel.z = number;
		} 

		if (crash.isOn) {
			//Debug.Log("crash on");
			shipScript.dummyScript.startPos = new Vector3 (0, 0, 6771);
		} else {
			//Debug.Log("crash off");
			shipScript.dummyScript.startPos = new Vector3 (0, 0, -6771);
		}

		shipScript.dummyScript.dummyBegin ();
//		Debug.Log ("before: " + shipScript.startVelocity);
		shipScript.resetShips ();
		//Debug.Log ("after: " + shipScript.startVelocity);
		gameObject.SetActive (false);

		shipScript.state = shipBehavior.GameState.Start;
		
	}
}
