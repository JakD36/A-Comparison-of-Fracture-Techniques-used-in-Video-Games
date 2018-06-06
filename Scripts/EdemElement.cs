using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class EdemElement: MonoBehaviour{

    // Element specific information
    public float radius;
    public Vector4 elements;
    public List<PoreSpring> poreSprings = new List<PoreSpring>{};

    private Vector3 poreForce;

    public float normalContactStiffness = 1f;
    public float normalContanctDampening = 1f; 
    public float tangentialContactDampening = 1f;

    private bool added;
    void start(){
        poreForce = new Vector3();
    }

    // void OnTriggerStay(Collider collider){
    //     // (3) Move elements by contact forces 
	// 	Vector3 normalVector = (transform.position - collider.gameObject.transform.position); // Vector from other to this 
        
    //     EdemElement otherEdemScript = collider.gameObject.GetComponent<EdemElement>();
    //     if(otherEdemScript != null){ // If it is another edemElement!
            
    //         float penetrationDistance = (radius + otherEdemScript.radius) - normalVector.magnitude; // Calc how much the two spheres have overlapped
            
    //         if( penetrationDistance > 0 ){ // The two elements have collided! 
    //         // Calculate the collision force!
                
    //             Vector3 relativeSpeed = collider.gameObject.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity;
                
    //             Vector3 normalForce = normalContactStiffness * penetrationDistance * normalVector.normalized + Vector3.Dot(normalVector.normalized,relativeSpeed) * normalVector.normalized; 
                
    //             Vector3 tangentialForce = -tangentialContactDampening * (relativeSpeed - Vector3.Dot(normalVector.normalized,relativeSpeed) * normalVector.normalized);

    //             Vector3 contactForce = normalForce + tangentialForce;
    //             GetComponent<Rigidbody>().AddForce(contactForce,ForceMode.Impulse);
    //         }
    //     }else{
    //         Vector3 closestPoint = collider.ClosestPoint(gameObject.transform.position);
    //         if ( (closestPoint - gameObject.transform.position).magnitude < radius){
    //             float penetrationDistance = (radius - (closestPoint - gameObject.transform.position).magnitude);
    //             Vector3 otherVel;
    //             if(collider.gameObject.GetComponent<Rigidbody>()!=null){
    //                 otherVel = collider.gameObject.GetComponent<Rigidbody>().velocity;
    //             }
    //             else{
    //                 otherVel = new Vector3();
    //             }
    //             Vector3 relativeSpeed =  - GetComponent<Rigidbody>().velocity;
    //             Vector3 normalForce = normalContactStiffness * penetrationDistance * normalVector.normalized + Vector3.Dot(normalVector.normalized,relativeSpeed) * normalVector.normalized; 
                
    //             Vector3 tangentialForce = tangentialContactDampening * (relativeSpeed - Vector3.Dot(normalVector.normalized,relativeSpeed) * normalVector.normalized);

    //             Vector3 contactForce = normalForce + tangentialForce;
    //             GetComponent<Rigidbody>().AddForce(contactForce,ForceMode.Impulse);
    //         }
    //     }
    // }

    public void addPoreForce(Vector3 force){
        poreForce += force;
    }

    void FixedUpdate(){
        GetComponent<Rigidbody>().AddForce(poreForce);
        poreForce = new Vector3();
    }    

}