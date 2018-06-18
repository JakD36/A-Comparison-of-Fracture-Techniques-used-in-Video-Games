using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// The EdemElement Script attached to each EdemElement gameObject
/// <para>
/// Handles the addition of forces to the rigidbody from springs and through raycast selection through the methods
/// </para>
/// </summary>
public class EdemElement: MonoBehaviour{

    
    public List<PoreSpring> poreSprings = new List<PoreSpring>{}; // The springs attached to this element

    private Vector3 poreForce; // The force vector from the attached springs to be applied to this element, in the FixedUpdate

    
    // A force can also be applied to individual elements, through a raycast from a script attached to the camera
    // This is only applied when the user presses a button. This allows for opposing forces to be placed on opposite sides of the object to hopefully see it be torn apart.
    private Vector3 rayForceDirection; // So the direction that the force will be applied is stored.
    
    private static float rayForce = 0; // The force to be applied along rayForceDirection, this is a static variable and the same across all Edem elements. 
    // FIX: for multiple elements may want to make this force a variable of the mass spring system, this way can have different forces applied to different objects. But this will do fine for now.

   
    /// <summary>
	/// Use this for initialization, of each edem element gameObject
	/// <para>
    /// Unity's built in method called once an object is created. Initialises the pore force on the object
	/// </para>
	/// </summary>
    void start(){
        poreForce = new Vector3();
    } 

    
    /// <summary>
    /// FixedUpdate is called every fixed framerate frame.
	/// <para>
	/// If user is pressing space bar the raycast force is applied, to test pulling object apart. 
	/// 
	/// Using FixedUpdate instead of Update as 
	/// "FixedUpdate should be used instead of Update when dealing with Rigidbody. For example when adding a force to a rigidbody, 
	/// you have to apply the force every fixed frame inside FixedUpdate instead of every frame inside Update." - From Unity's own documentation 
	/// </para>
	/// </summary>
    void FixedUpdate(){
        // If the space bar is hit, apply the ray force in the direction stored
        if(Input.GetKey(KeyCode.Space)){
            
            GetComponent<Rigidbody>().AddForce(rayForce*rayForceDirection);
        }
    }    

    
    /// <summary>
    ///  Applies the forces from each of the springs attached.
    /// <para>
    /// Adds the poreForce calculated from the spring to rigid body attached to this gameObject, This way all the forces from any springs still in operation are calculated before applying them to the elements.
    /// </para>
    /// </summary>
    public void applySpringForce(){
        GetComponent<Rigidbody>().AddForce(poreForce);
        poreForce = new Vector3(); // Reset our force coming from the springs
    }

    
    
    /// <summary>
    /// Adds to the poreforce to be applied to this element
    /// <para>
    /// The pore force is the restorative force applied by the spring to put the element back to its original position.
    /// Store the force before applying it in the 
    /// </para>
    /// </summary>
    public void addPoreForce(Vector3 force){
        poreForce += force;
    }

    
    
    /// <summary>
    /// Adds to the rayForce to be applied to the elements
    /// <para>
    /// is a static method, as rayforce is a static variable, each and every element is applied the same magnitude of force they are just applied in different directions
    /// </para>
    /// </summary>
    public static void addRayForce(float rayForce){
        EdemElement.rayForce += rayForce;
    }

    
    
    /// <summary>
    /// Sets the direction for the ray force to be applied
    /// <para>
    /// This allows other objects like the RayAddForce script attached to the camera, to assign the direction based on the raycast
    /// </para>
    /// </summary>
    public void setRayForceDirection(Vector3 dir){
        this.rayForceDirection = dir;
    }
}