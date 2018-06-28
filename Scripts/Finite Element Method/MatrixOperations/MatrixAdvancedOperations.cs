using System;
using System.Collections;
using UnityEngine;


public partial class Matrix{
    

    public Matrix invert(){
        Matrix output;
        if(this.getCols() == this.getRows()){ // We have a square matrix
            float det = Matrix.Det(this);
            if(det != 0){ // FIX: will need to take into account floating point error on this check
                    Matrix adjoint = new Matrix(this.getRows(),this.getCols());
                    for(int n = 0; n < this.getRows(); n++){
                        for(int m = 0; m < this.getCols(); m++){
                            int s = 1;
                        
                            if(n%2 != 0){
                                s*=-1;
                            }
                            if(m%2 != 0){
                                s*=-1;
                            }   
                            adjoint[n,m] = s*Matrix.Det( this.exclude(n,m) );
                        }
                    }
                    adjoint = adjoint.transpose();

                    output = adjoint/det;
            }else{
                throw new MatrixException("To find the inverse the matrix has to be non singular, so det != 0");
            }
        }else{
            throw new MatrixException("To get the inverse the matrix has to be square, ie the same number of rows as columns");
        }
        return output;
    }

    public Matrix transpose(){
        Matrix output = new Matrix(this.getCols(),this.getRows());

        for(int n = 0; n < this.getCols(); n++){
            for(int m = 0; m < this.getRows(); m++){
                output[n,m] = matrix[m,n];
            }
        }

        return output;
    }

    public Matrix[] QRdecomp(){

        Matrix Q = new Matrix(this.getRows(),this.getCols());
        Matrix R = new Matrix(this.getRows(),this.getCols());

        // Matrix A is this.
        Vector[] e = new Vector[this.getCols()];
        Vector a = new Vector(this,0);
        e[0] = a.normalise();
        Q.replaceColumn(e[0],0);
        R[0,0] = Vector.Dot(a,e[0]);
        for(int n = 1; n < this.getCols(); n++){ // Since the first one is different
            a = new Vector(this,n); 
            Vector u = a;
            for(int m = 0; m < n; m++){
                float dot = Vector.Dot(a,e[m]);
                u -= (dot*e[m]);
                R[m,n] = dot;
            }
            e[n] = u.normalise();
            Q.replaceColumn(e[n],n);
            R[n,n] = Vector.Dot(e[n],a);
        }


        Matrix[] output = new Matrix[] {Q,R};
        return output;
    }

    public Matrix[] calculateEigen(){
        if(this.getRows() == this.getCols()){ 
            Matrix eigenvectors = new Matrix(getCols(),getCols());
            Matrix eigenvalues = new Matrix(getCols(),getCols());
            Matrix A = new Matrix(matrix);
            eigenvectors = Matrix.identity(getCols());
            for(int n = 0; n < 50; n++){ // Probs need a proper way of knowing when to stop iterations
                Matrix[] QR = A.QRdecomp();
                A = QR[1]*QR[0];
                eigenvectors *= QR[0];
            }
            
            for(int n = 0; n < A.getCols(); n++){
                eigenvalues[n,n] = A[n,n];
            }
            Matrix[] output = new Matrix[] {eigenvalues, eigenvectors};
            return output;
        }else{
            throw new MatrixException("Must be a square matrix");
        }
    }

    public Matrix[] polarDecomposition(){
        Matrix U;
        Matrix P;

         
        Matrix V = this.calculateEigen()[1];
        Matrix D = V.invert() * this.transpose()*this * V;
        for(int n = 0; n < getCols(); n++){
            D[n,n] = Mathf.Sqrt(D[n,n]);
        }
        P = V * D * V.invert();
        U = this*P.invert();
        Matrix[] output = new Matrix[] {U,P};
        return output;
    }

    public float trace(){
        float sum = 0;
        if(getCols() == getRows()){
            for(int n = 0; n < getCols(); n++){
                sum += matrix[n,n];
            }
        }
        else{
            throw new MatrixException("Must be square matrix");
        }
        return sum;   
    }
    

}
