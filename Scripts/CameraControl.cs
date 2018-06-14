using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float rotationSpeed = new float();
	public float speed = new float();
	public GameObject target;
	private float camX = new float();
	public float camY = new float();
	private float camZ = new float();
	public float angle = new float();
	public float radius = new float();

	
	
	
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
		this.transform.position = new Vector3(camX,camY,camZ);
		if(target!=null){
			this.transform.LookAt(target.transform);
		}else{
			this.transform.LookAt(new Vector3());
		}
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
			angle += rotationSpeed * Time.deltaTime;
		}
		if( Input.GetKey(KeyCode.RightArrow) ){
			angle -= rotationSpeed * Time.deltaTime;
		}
		
		float smoothedSpeed = 0.125f;
		
		camX = radius*Mathf.Sin(Mathf.Deg2Rad*angle);
		camZ = radius*Mathf.Cos(Mathf.Deg2Rad*angle);

		
		Vector3 desiredPosition = new Vector3(camX,camY,camZ);
		Vector3 smoothedPosition = Vector3.Lerp(transform.position,desiredPosition,smoothedSpeed);
		this.transform.position = smoothedPosition;
		if(target!=null){
			this.transform.LookAt(target.transform);
		}else{
			this.transform.LookAt(new Vector3());
		}
	}
}
