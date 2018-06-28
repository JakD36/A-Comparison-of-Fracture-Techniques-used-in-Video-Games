using System;
using System.Collections;
using UnityEngine;


/// <summary>
///
/// <para>
/// 
/// </para>
/// </summary>
public partial class Matrix{

    private float[,] matrix {get; set;}

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public Matrix(int rows, int cols){
        matrix = new float[rows,cols];
    }
    public Matrix(Vector vec){
        matrix = new float[1,vec.Length];
        for(int n = 0; n < vec.Length; n++){
            matrix[1,n] = vec[n];
        }
    }

    public Matrix(Vector3 vec){
        matrix = new float[1,3];
        matrix[0,0] = vec.x;
        matrix[0,1] = vec.y;
        matrix[0,2] = vec.z;
    }

    public Matrix(float[,] array){ // Preferebly would allow initializer like with arrays, need to read into Enumerable interface
        matrix = (float[,])array.Clone(); // Just by reference will need fixed FIX: Actually copy it   
    }

    // Indexer!
    public float this[int row,int col]{
        get { return matrix[row,col]; }
        set { matrix[row,col] = value; }
    }

    // Accessors

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public int[] getSize(){
        return new int[2] {matrix.GetLength(0),matrix.GetLength(1)};
    }

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public int getRows(){
        return matrix.GetLength(0);
    }

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public int getCols(){
        return matrix.GetLength(1);
    }

    
    public Matrix Clone(){
        return new Matrix((float[,])matrix.Clone());
    }

    public Matrix exclude(int row, int col){
        Matrix output = new Matrix(this.getRows()-1,this.getCols()-1);
        for(int n = 0, N = 0; n < this.getRows()-1; n++, N++){
            for(int m = 0, M = 0; m < this.getCols()-1; m++, M++){
                if(N == row){
                    N++; // If row to be excluded move to next
                }
                if(M == col){
                    M++; // if Col to be exclude move to next
                }
                output[n,m] = this.matrix[N,M];
            }   
        }
        return output;
    }


    public void replaceColumn(Vector a, int col){
        if(this.getCols() == a.Length){
            for(int n = 0; n < a.Length; n++){
                this[n,col] = a[n];
            }
        }
    }
}
