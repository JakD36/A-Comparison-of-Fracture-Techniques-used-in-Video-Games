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

    void start(){
        poreForce = new Vector3();
    }

    void Update(){
        
        // (1) Calculate external forces

        // (2) Move elements by external forces

        // (3) Move elements by contact force

        // (4) Move elements by pore spring

        // (5) Perform collision detection and response with other EDEM objects

        // (6) Perform collision detection and response with polygonal objects for the floor
        
    }

    void FixedUpdate(){
        float mass = GetComponent<Rigidbody>().mass;
        Vector3 accel = poreForce/mass;
        poreForce = new Vector3();
        GetComponent<Rigidbody>().AddForce(accel,ForceMode.Acceleration);
        
        Vector3 grav = new Vector3(0.0f,-9.81f,0.0f);
        GetComponent<Rigidbody>().AddForce(grav,ForceMode.Acceleration);
    }
    public void addPoreForce(Vector3 force){
        poreForce += force;
    }

    
    
}