using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public Simulation sim;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(sim.com.x,sim.com.y,sim.com.z - 15);
	}
}
