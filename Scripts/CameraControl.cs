﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for the movement of the camera
/// <para>
/// Uses arrow keys to move the camera around the origin and up and down in the scene while always looking at the centre of the scene
/// </para>
/// </summary>
public class CameraControl : MonoBehaviour {

	public float rotationSpeed = new float();
	public float speed = new float();
	private float camX = new float();
	public float camY = new float();
	private float camZ = new float();
	public float angle = new float();
	public float radius = new float();

	// public GameObject CannonBallPrefab;

	// // Variables
	// public float force; // The force to fire the cannonball
	// public float cannonBallMass; // The mass of the cannonballs when fired
	// public float cannonBallRadius; // The size of the cannonballs
	// public GameObject subject;
	// GameObject cannonBall;

	
	
	
	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Moves the camera to the start position!
	/// </para>
	/// </summary>
	void Start() {
		camX = radius*Mathf.Sin(Mathf.Deg2Rad*angle);
		camZ = radius*Mathf.Cos(Mathf.Deg2Rad*angle);
		this.transform.position = new Vector3(camX+3,camY,camZ);
		this.transform.LookAt(new Vector3(3,0,0));
	}

	void Update(){
		// if(Input.GetKeyDown(KeyCode.Space)){
		// 	cannonBall = (GameObject)Instantiate<GameObject>(CannonBallPrefab);
				
		// 		cannonBall.transform.position = new Vector3(-10,cannonBallRadius+0.3f,0);
				
		// 		cannonBall.GetComponent<Rigidbody>().mass = cannonBallMass;
		// 		cannonBall.transform.localScale*=cannonBallRadius;
		// 		cannonBall.GetComponent<SphereCollider>().radius = 0.5f; 
		// 		cannonBall.GetComponent<Rigidbody>().AddForce(
		// 			(subject.transform.position+new Vector3(0,0.5f,0)-cannonBall.transform.position).normalized*force, 
		// 			ForceMode.Impulse);
		// 		Destroy(cannonBall,3); 
		// }
	}


	/// <summary>
	/// Applies changes, after Update has been called on all objects
	/// <para>
	/// Moves the camera based on the users input, currently hard coded, to arrow keys for moving up and down and rotating left and right around the object
	/// </para>
	/// </summary>
	void LateUpdate () {
		// Apply changes based on user input
		if( Input.GetKey(KeyCode.UpArrow) ){
			camY +=  Time.deltaTime * speed;
		}
		if( Input.GetKey(KeyCode.DownArrow) && camY >= 0.5f ){
			camY -=  Time.deltaTime * speed;
		}
		if( Input.GetKey(KeyCode.LeftArrow) ){
			angle += rotationSpeed * Time.deltaTime;
		}
		if( Input.GetKey(KeyCode.RightArrow) ){
			angle -= rotationSpeed * Time.deltaTime;
		}
		
		// Using LERP to smooth the movement of the camera
		float smoothedSpeed = 0.125f;
		
		camX = radius*Mathf.Sin(Mathf.Deg2Rad*angle); // Calculate the new position on the x-axis
		camZ = radius*Mathf.Cos(Mathf.Deg2Rad*angle); // Calculate the new position on the z-axis

		
		Vector3 desiredPosition = new Vector3(camX+3,camY,camZ);
		Vector3 smoothedPosition = Vector3.Lerp(transform.position,desiredPosition,smoothedSpeed);
		this.transform.position = smoothedPosition; // Move the camera
		this.transform.LookAt(new Vector3(3,0,0)); // Make sure looking at the centre of the scene
	}
}
