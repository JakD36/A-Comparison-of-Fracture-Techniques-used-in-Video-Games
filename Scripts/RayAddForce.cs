using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayAddForce : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit; // Use a raycast to select a space
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit,100)){
				if(hit.rigidbody!=null){
					hit.rigidbody.gameObject.GetComponent<EdemElement>().setRayForceDirection(hit.normal);
					hit.rigidbody.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
				}
			}
		}
		if(Input.GetKey(KeyCode.W)){
            EdemElement.addRayForce(-1f);
        }
        if(Input.GetKey(KeyCode.S)){
			EdemElement.addRayForce(-1f);
        }
	}
}
