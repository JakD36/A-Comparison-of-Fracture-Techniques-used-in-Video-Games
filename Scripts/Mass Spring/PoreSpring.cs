using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// The pore spring object that attaches elements together.
/// <para>
/// Simulates an ideal spring that is between objects, has a stiffness, but does not take into account dampening
/// </para>
/// </summary>
public class PoreSpring{
    private float stiffness; // The stiffness k of the spring, the force is calculated through F = kdx, where dx is the change from rest length
    private float restLength; // The rest length is the length at which the force applied is zero
    private GameObject[] elements; // The edem Elements attached at either end of the spring

    private bool broken = false;

    /// <summary>
    /// The constructor,
    /// <para>
    /// Initialises the spring based on the elements provided to it.
    /// </para>
    /// <param name="elementA">The first element attached to this spring</param>
    /// <param name="elementB">The second element attached to this spring</param>
    /// <param name="stiffness">The stiffness of the spring</param>
    /// </summary>
    public PoreSpring(GameObject elementA, GameObject elementB, float stiffness){    
        // Assign the elements to the spring
        elements = new GameObject[2];
        elements[0] = elementA;
        elements[1] = elementB;
        // Calculate the rest length as the distance between the elements, before any movement
        this.restLength = (elementB.transform.position-elementA.transform.position).magnitude;
        this.stiffness = stiffness; // Apply the stiffness to the spring
    }

    /// <summary>
    /// Gets the ratio of current length to rest length of the spring
    /// </summary>
    /// <returns>A float of the ratio between the current distance between the elements, compared to their initial distance</returns>
    public float getLengthRatio(){
        Vector3 deltaLength = (elements[1].transform.position-elements[0].transform.position);
        return deltaLength.magnitude/restLength;
    }

    /// <summary>
    /// Calculates the force to restore elements back to their original position
    /// <para>
    /// Calculates the force based on the equation F = kdx, where k is the stiffness and dx is the change from the rest length
    /// </para>
    /// </summary>
    public void calculateRestorationForce(){
        // if delta length is < 0 then spring is stretch pull elements together
        // if delta length is > 0 spring is compressed push apart
        Vector3 deltaLength = (elements[1].transform.position-elements[0].transform.position);
        Vector3 forceOn0 = stiffness * (deltaLength.magnitude - restLength) * (deltaLength/deltaLength.magnitude); // This is the force in the direction 1 to 0
        Vector3 forceOn1 = forceOn0*-1;
        
        // Tell each element the force it will have to apply
        elements[0].GetComponent<EdemElement>().addPoreForce(forceOn0); 
        elements[1].GetComponent<EdemElement>().addPoreForce(forceOn1);
    }

    /// <summary>
    /// Allows for the breaking of the spring, so that it can no longer provide a restoring force
    /// <para>
    /// Calling this method assigns the spring as broken and cannot be reversed.
    /// </para>
    /// </summary>
    public void breakSpring(){
        this.broken = true;
    }

    /// <summary>
    /// Checks if the spring is broken or not
    /// </summary>
    /// <returns>Returns a boolean to represent if the spring is broken, true means it is broken</returns>
    public bool isBroken(){
        return this.broken;
    }

    

}