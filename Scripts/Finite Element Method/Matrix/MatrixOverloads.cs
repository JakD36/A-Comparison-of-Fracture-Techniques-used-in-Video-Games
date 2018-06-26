using System;
using System.Collections;
using UnityEngine;

public partial class Matrix{

        // Operator Overloading!
    public static Matrix operator+(Matrix a, Matrix b){
        
        Matrix output; 
        
        if(a.getCols() == b.getCols() && a.getRows() == b.getRows()){
            output = new Matrix(a.getRows(),a.getCols());
            for(int n = 0; n < a.getRows(); n++){
                for(int m = 0; m < a.getCols(); m++){
                    output[n,m] = a[n,m] + b[n,m];
                }
            }
        }else{
            throw new MatrixException("The size of matrix A must match the size of matrix B to complete the addition!");
        }

        return output;

    }

    public static Matrix operator-(Matrix a, Matrix b){
        
        Matrix output; 
        
        if(a.getCols() == b.getCols() && a.getRows() == b.getRows()){
            output = new Matrix(a.getRows(),a.getCols());
            for(int n = 0; n < a.getRows(); n++){
                for(int m = 0; m < a.getCols(); m++){
                    output[n,m] = a[n,m] - b[n,m]; 
                }
            }
        }else{
            throw new MatrixException("The size of matrix A must match the size of matrix B to complete the subtraction!");
        }

        return output;

    }

    public static Matrix operator*(Matrix a, Matrix b){
        
        Matrix output; 
        
        if(a.getCols() == b.getRows() ){
            output = new Matrix(a.getRows(),b.getCols());
            
            for(int n = 0; n < a.getRows(); n++){
                for(int m = 0; m < b.getCols(); m++){
                    for(int k = 0; k < b.getRows(); k++){
                        output[n,m] += a[n,k]*b[k,m];
                    }
                }
            }
            
        }else{
            throw new MatrixException("The number of columns in matrix A must match the number of rows in matrix B");
        }

        return output;

    }

    public static Matrix operator*(Matrix a, float b){
        
        Matrix output = new Matrix(a.getRows(),a.getCols());
            
            for(int n = 0; n < a.getRows(); n++){
                for(int m = 0; m < a.getCols(); m++){
                    output[n,m] = a[n,m]*b;
                }
            }
        return output;
    }

    public static Matrix operator*(float a, Matrix b){
        return b*a;
    }

    public static Matrix operator/(Matrix a, float b){
        
        Matrix output = new Matrix(a.getRows(),a.getCols());
            
            for(int n = 0; n < a.getRows(); n++){
                for(int m = 0; m < a.getCols(); m++){
                    output[n,m] = a[n,m]/b;
                }
            }
        return output;
    }


}