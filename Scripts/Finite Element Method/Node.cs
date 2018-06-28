using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Node{
    
    private Vector3 undeformedPosition;
    private Vector3 deformedPosition;
    private Vector3 velocity;

    private float mass;
    private int index;
    private FEMElement parent;

    public Node(Vector3 vertex, int index){
        this.index = index;
        this.undeformedPosition = vertex;
        this.deformedPosition = vertex;
        this.velocity = new Vector3();
        this.mass = 1; // NEED TO SET THE MASS PROPERLY
    }

    public Vector3 uPosition{
        get{return undeformedPosition;}
        set{this.undeformedPosition = value;}
    }

    public int Index{
        get{return this.index;}
        set{;}
    }

    public Vector3 Position{
        get{return deformedPosition;}
        set{this.deformedPosition = value;}
    }

    public Vector3 Velocity{
        get{return velocity;}
        set{this.velocity = value;}
    }

    public void setParent(FEMElement parent){
        this.parent = parent;
    }
}
