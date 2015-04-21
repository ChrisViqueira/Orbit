using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainHUD : MonoBehaviour {
	public Text warpScale;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	void displayHUD () {
		//warpScale.
		warpScale.text = "warp x" + Time.timeScale;
	}
}
