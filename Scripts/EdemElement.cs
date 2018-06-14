using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class EdemElement: MonoBehaviour{

    
    public List<PoreSpring> poreSprings = new List<PoreSpring>{}; // The springs attached to this element

    private Vector3 poreForce;

    private Vector3 rayForceDirection;
    private static float rayForce = 0;


    void start(){
        poreForce = new Vector3();
    } 

    public void addPoreForce(Vector3 force){
        poreForce += force;
    }

    void FixedUpdate(){
        GetComponent<Rigidbody>().AddForce(poreForce);
        poreForce = new Vector3();
        
        if(Input.GetKey(KeyCode.Space)){
            GetComponent<Rigidbody>().AddForce(rayForce*rayForceDirection);
        }
    }    

    public static void addRayForce(float rayForce){
        EdemElement.rayForce += rayForce;
    }

    public void setRayForceDirection(Vector3 dir){
        this.rayForceDirection = dir;
    }
}