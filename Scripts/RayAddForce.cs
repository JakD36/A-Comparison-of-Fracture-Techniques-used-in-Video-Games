using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses Raycasts to apply a force direction to edem elements, so that a force can be applied upon pressing the space bar.
/// </summary>
public class RayAddForce : MonoBehaviour {

	/// <summary>
	/// Update is called once per frame
	/// <para>
	/// Checks if the user has clicked the left mouse button, if they have use a raycast to apply a rayForceDirection to the element hit
	/// Also checks for the key presses W and S to see if the force to be applied to selected elements should be raised or lowered
	/// </para>
	/// </summary>
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit; // Use a raycast to select a space
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit,100)){
				if(hit.rigidbody!=null){
					hit.rigidbody.gameObject.GetComponent<Element>().setRayForceDirection(hit.normal);
					hit.rigidbody.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
				}
			}
		}
		// Could do a check if they right click to remove the selection

		if(Input.GetKey(KeyCode.W)){
            Element.addRayForce(-1f);
        }
        if(Input.GetKey(KeyCode.S)){
			Element.addRayForce(-1f);
        }
	}
}
