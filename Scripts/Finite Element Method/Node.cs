using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node{
    
    private Vector3 undeformedPosition;
    private Vector3 deformedPosition;
    private Vector3 velocity;

    private float mass;

    private FEMElement parent;

    public Vector3 uPosition{
        get{return undeformedPosition;}
        set{this.undeformedPosition = value;}
    }

    public Vector3 Position{
        get{return deformedPosition;}
        set{this.deformedPosition = value;}
    }

    public Vector3 Velocity{
        get{return velocity;}
        set{this.velocity = value;}
    }

    public Node(Vector3 vertexPosition){
        this.deformedPosition = vertexPosition;
        this.velocity = new Vector3(); // Zero velocity
    }


}