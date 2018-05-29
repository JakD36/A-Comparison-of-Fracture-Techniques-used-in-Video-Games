using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCanon : MonoBehaviour {

	public GameObject CanonBall;
	public float force;
	public float canonBallMass;
	public float canonBallRadius;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Input.GetMouseButtonDown(0)){
			GameObject canonBall = (GameObject)Instantiate<GameObject>(CanonBall);
			canonBall.transform.position = transform.position;
			canonBall.GetComponent<Rigidbody>().mass = canonBallMass;
			canonBall.GetComponent<SphereCollider>().radius = canonBallRadius;
			canonBall.transform.localScale*=canonBallRadius;
			canonBall.GetComponent<Rigidbody>().AddForce(Camera.main.ScreenPointToRay(Input.mousePosition).direction*force,ForceMode.Impulse);
		}
	}
}
