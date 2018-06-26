using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEMElement{
    private Node[] children;
    private Vector3 position;

    private Matrix beta;

    public FEMElement(Node nodeA, Node nodeB, Node nodeC, Node nodeD){
        children = new Node[4];
        // Add the 4 nodes to the FEM tetrahedron
        children[0] = nodeA;
        children[1] = nodeB;
        children[2] = nodeC;
        children[3] = nodeD;

        // Calculate Beta, that will be used to find deformation gradient
        beta = calculateBeta();

        // Get the position of the element, as the centre point of all 4 nodes
        position = new Vector3();
        for(int n = 0; n < 4; n++){
            position += children[n].uPosition;
            children[n].setParent(this); // As we are looping anyway tell the node which element it belongs to
        }
        position/=4;
    }

    private Matrix calculateBeta(){
        Matrix Du = new Matrix(3,3);

        for(int n = 0; n < 3; n++){
            Du[0,n] = children[n].uPosition.x-children[0].uPosition.x; 
            Du[1,n] = children[n].uPosition.y-children[0].uPosition.y;
            Du[2,n] = children[n].uPosition.z-children[0].uPosition.z;
        }
        return Du.invert();
    }

    Matrix calculateDeformationGradient(){
        Matrix Dx = new Matrix(3,3);
        Matrix beta = calculateBeta();
        for(int n = 0; n < 3; n++){
            Dx[0,n] = children[n].Position.x-children[0].Position.x; 
            Dx[1,n] = children[n].Position.y-children[0].Position.y;
            Dx[2,n] = children[n].Position.z-children[0].Position.z;
        }
        return Dx*beta; // Return the deformation gradient 
    }
    void calculations(){
        Matrix F = calculateDeformationGradient();
        // now use polar decomposition to split F into Q and A
        Matrix Q = F.polarDecomposition()[0];
        // Factor out Q from the deformation gradient F, this allows us to calculate the corotational strain
        F =  Q.transpose()*F;

        Matrix corotationalStrain = 0.5f * (F + F.transpose()) - Matrix.identity(3);
        float lambda = 1; // Material property 
        float mu = 1; // Material property 
        Matrix elementStress = lambda * corotationalStrain.trace()*Matrix.identity(3) + 2*mu*corotationalStrain;

        Vector[] elasticForceOnNode = new Vector[4];
        
        Matrix AWONormals = new Matrix(3,4);

        for(int n = 0; n < 4; n++){
            Vector3[] trianglePoints = new Vector3[3];
            Vector3 midPointOfTriangle = new Vector3();

            for(int m = 0; m < 4; m++){ // Grab the other 3 nodes! we need to calculate an area!
                if(m!=n){
                    trianglePoints[m] = children[m].Position;
                    midPointOfTriangle += children[m].Position;
                }
            }
            midPointOfTriangle/=3; // get the midpoint!
            // Find the area of the triangle, equation of area of triangle A = 0.5bh
            float triangleBase = (trianglePoints[0]-trianglePoints[1]).magnitude;
            float triangleHeight = (trianglePoints[2]-(trianglePoints[0]+trianglePoints[1])/2).magnitude; // find the length between the third point and the midpoint of point 1 and 2 
            float area = 0.5f * triangleBase * triangleHeight;

            Vector3 normal3 = Vector3.Cross( (trianglePoints[1]-trianglePoints[0]),(trianglePoints[2]-trianglePoints[0]) ).normalized;
            Vector normal = new Vector(normal3);
            // Check the normal is facing the right direction!
            if( (midPointOfTriangle-position).magnitude > (normal3+midPointOfTriangle-position).magnitude ){
                normal*=-1;
            }
            AWONormals.replaceColumn(normal,n);
            elasticForceOnNode[n] = Q * elementStress * area * normal;
        }
    }
}