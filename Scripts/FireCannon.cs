using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCannon : MonoBehaviour {

	public GameObject CannonBall;
	public float force;
	public float cannonBallMass;
	public float cannonBallRadius;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			GameObject cannonBall = (GameObject)Instantiate<GameObject>(CannonBall);
			cannonBall.transform.position = transform.position;
			cannonBall.GetComponent<Rigidbody>().mass = cannonBallMass;
			cannonBall.GetComponent<SphereCollider>().radius = cannonBallRadius;
			cannonBall.transform.localScale*=cannonBallRadius;
			cannonBall.GetComponent<Rigidbody>().AddForce(Camera.main.ScreenPointToRay(Input.mousePosition).direction*force,ForceMode.Impulse);
			Destroy(cannonBall,5);
		}
	}
}
