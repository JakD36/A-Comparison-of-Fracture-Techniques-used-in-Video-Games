using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float rotationSpeed = new float();
	public float speed = new float();
	public GameObject target;
	public float camX = new float();
	public float camY = new float();
	public float camZ = new float();
	public float yaw = new float();
	public float pitch = new float();
	public float radius = new float();
	// Use this for initialization
	void Start () {
		
	}
	
	
	// Update is called once per frame
	void LateUpdate () {
		if( Input.GetKey(KeyCode.UpArrow) ){
			camY +=  Time.deltaTime * speed;
		}
		if( Input.GetKey(KeyCode.DownArrow) && camY >= 0.5f ){
			camY -=  Time.deltaTime * speed;
		}
		if( Input.GetKey(KeyCode.LeftArrow) ){
			yaw += rotationSpeed * Time.deltaTime;
		}
		if( Input.GetKey(KeyCode.RightArrow) ){
			yaw -= rotationSpeed * Time.deltaTime;
		}
		
		float smoothedSpeed = 0.125f;
		
		camX = radius*Mathf.Sin(Mathf.Deg2Rad*yaw);
		camZ = radius*Mathf.Cos(Mathf.Deg2Rad*yaw);

		
		Vector3 desiredPosition = new Vector3(camX,camY,camZ);
		Vector3 smoothedPosition = Vector3.Lerp(transform.position,desiredPosition,smoothedSpeed);
		this.transform.position = smoothedPosition;
		
		this.transform.LookAt(new Vector3());
		
	}
}
