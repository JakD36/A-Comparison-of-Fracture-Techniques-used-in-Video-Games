using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


public class PoreSpring{
    private float stiffness;
    private float restLength;
    public GameObject[] elements;

    private bool broken = false;

    public PoreSpring(GameObject elementA, GameObject elementB, float stiffness){    
        elements = new GameObject[2];
        elements[0] = elementA;
        elements[1] = elementB;
        this.restLength = (elementB.transform.position-elementA.transform.position).magnitude;
        this.stiffness = stiffness;
    }

    public float getLengthRatio(){
        Vector3 deltaLength = (elements[1].transform.position-elements[0].transform.position);
        return deltaLength.magnitude/restLength;
    }

    public void update(){
        // if delta length is < 0 then spring is stretch pull elements together
        // if delta length is > 0 spring is compressed push apart
        Vector3 deltaLength = (elements[1].transform.position-elements[0].transform.position);
        Vector3 forceOn0 = stiffness * (deltaLength.magnitude - restLength) * (deltaLength/deltaLength.magnitude); // This is the force in the direction 1 to 0
        Vector3 forceOn1 = forceOn0*-1;
        
        elements[0].GetComponent<EdemElement>().addPoreForce(forceOn0);
        elements[1].GetComponent<EdemElement>().addPoreForce(forceOn1);
    }

    public void breakSpring(){
        this.broken = true;
    }

    public bool isBroken(){
        return this.broken;
    }

    

}