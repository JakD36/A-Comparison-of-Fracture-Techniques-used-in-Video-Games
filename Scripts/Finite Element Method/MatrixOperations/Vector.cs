using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector{

    float[] vec;

    public Vector(int size){
        vec = new float[size];
    }

    public Vector(Vector3 vec3){
        this.vec = new float[] {vec3.x, vec3.y, vec3.z};
    }
    
    public Vector(Matrix A, int col){
        vec = new float[A.getRows()];
        for(int n = 0; n < A.getRows(); n++){
            vec[n] = A[n,col];
        }
    }

    public Vector(float[] array){
        vec = (float[])array.Clone();
    }

    public Vector Clone(){
        return new Vector((float[])vec.Clone());
    }

    // Indexer!
    public float this[int row]{
        get { return vec[row]; }
        set { vec[row] = value; }
    }

    public int Length{
        get{return vec.Length;}
    }



        // Operator Overloading!
    public static Vector operator+(Vector a, Vector b){
        
        Vector output; 
        
        if(a.Length == b.Length){
            output = new Vector(a.Length);
            for(int n = 0; n < a.Length; n++){
                    output[n] = a[n] + b[n];
            }
        }else{
            throw new MatrixException("The size of Vector A must match the size of matrix B to complete the addition!");
        }
        return output;
    }

    public static Vector operator-(Vector a, Vector b){
        
        Vector output; 
        
        if(a.Length == b.Length){
            output = new Vector(a.Length);
            for(int n = 0; n < a.Length; n++){
                    output[n] = a[n] - b[n];
            }
        }else{
            throw new MatrixException("The size of Vector A must match the size of Vector B to complete the addition!");
        }
        return output;

    }

    public static Vector operator*(Matrix a, Vector b){
        
        Vector output; 
        
        if(a.getCols() == b.Length ){
            output = new Vector(b.Length);
            
            for(int n = 0; n < a.getRows(); n++){
                    for(int k = 0; k < b.Length; k++){
                        output[n] += a[n,k]*b[k];
                    }
            }
            
        }else{
            throw new MatrixException("The number of columns in matrix A must match the number of rows in Vector B");
        }
        return output;
    }

    public static Vector operator*(Vector a, float b){
        
        Vector output = new Vector(a.Length);
            
            for(int n = 0; n < a.Length; n++){
                    output[n] = a[n]*b;
            }
        return output;
    }

    public static Vector operator*(float a, Vector b){
        return b*a;
    }

    public static Vector operator/(Vector a, float b){
        
        Vector output = new Vector(a.Length);
            
            for(int n = 0; n < a.Length; n++){
                    output[n] = a[n]/b;
            }
        return output;
    }


    public static float Dot(Vector a, Vector b){
        float output = 0.0f;

        if(a.Length == b.Length){
            for(int n = 0; n < a.Length; n++){
                    output += a[n] * b[n];
            }
        }else{
            throw new MatrixException("The size of Vector A must match the size of matrix B to find the dot product");
        }
        return output;
    }


    public float magnitude(){
        float mag = 0.0f; 
        for(int n = 0; n < this.Length; n++){
            mag += this[n]*this[n];
        }
        return Mathf.Sqrt(mag);
    }

    public Vector normalise(){
        Vector output = new Vector(this.Length);
        output = this/this.magnitude();
        return output;
    }

    public static string printVector(Vector a){
        string output = "[ ";
        for(int n = 0; n < a.Length; n++){
            output += a[n]+" ";
        }
        output+="]";
        return output;
    }
}