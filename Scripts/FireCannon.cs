using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Allows for the firing of cannonballs, from the cameras position.
/// <para>
/// Allows for testing of the destruction of the objects in the scene, by causing collisions from objects of variable size, mass and velocity
/// </para>
/// </summary>
public class FireCannon : MonoBehaviour {

	// Prefabs
	public GameObject CannonBallPrefab;
	
	// Variables
	public float force; // The force to fire the cannonball
	public float cannonBallMass; // The mass of the cannonballs when fired
	public float cannonBallRadius; // The size of the cannonballs
	
	/// <summary>
	/// Update is called once per frame
	/// <para>
	/// Is a MonoBehaviour method,
	/// 
	/// Here we check if the user has clicked the left mouse button to fire the cannon ball
	/// Then instantiate a new cannonball and provided it the correct, position, size, and apply the force to its rigid body
	/// This cannonball is assigned to be destroyed after 5 seconds 
	/// </para>
	/// </summary>
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			GameObject cannonBall = (GameObject)Instantiate<GameObject>(CannonBallPrefab);
			cannonBall.transform.position = transform.position;
			cannonBall.GetComponent<Rigidbody>().mass = cannonBallMass;
			cannonBall.transform.localScale*=cannonBallRadius;
			cannonBall.GetComponent<SphereCollider>().radius = 0.5f; 
			cannonBall.GetComponent<Rigidbody>().AddForce(Camera.main.ScreenPointToRay(Input.mousePosition).direction*force,ForceMode.Impulse);
			Destroy(cannonBall,3); // Destroy the cannonball after 5 seconds 
		}
	}
}
