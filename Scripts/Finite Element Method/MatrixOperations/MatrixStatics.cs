

public partial class Matrix{
    // Static methods

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public static Matrix identity(int size){
        Matrix identityMatrix = new Matrix(size,size);
        for(int n = 0; n < size; n++){
            identityMatrix[n,n] = 1;
        }
        return identityMatrix;
    }

    /// <summary>
    ///
    /// <para>
    /// 
    /// </para>
    /// </summary>
    public static string printMatrix(Matrix mat){
        string outputString = "";
        for(int n = 0; n < mat.getRows(); n++){
            outputString+="[ ";
            for(int m = 0; m < mat.getCols(); m++){
                outputString+=mat[n,m]+" ";
            }
            outputString+="]\n";
        }
        
        return outputString;
    }





    /// <summary>
    /// Finds the determinant of the matrix
    /// <para>
    /// Limited to the use of 2x2 and 3x3 matrices, due to complexity of nxn calculation, for the time being
    /// </para>
    /// </summary>

    public static float Det(Matrix mat){
        // HAS TO BE SQUARE
        float det = 0.0f;
        if(mat.getCols() == mat.getRows()){
            if(mat.getCols() == 1){
                det = mat[0,0];
            }else{
                for(int j = 0; j < mat.getCols(); j++){
                    int s;
                    if(j%2 == 0){
                        s = 1;
                    }else{s = -1;}
                    Matrix Aj = mat.exclude(0,j);
                    det += s * mat[0,j] * Matrix.Det(Aj);
                }
            }
        }
        else{
            throw new MatrixException("Has to be a square Matrix, ie number of columns equals the number of rows. Supplied matrix >> "+mat.getRows()+"x"+mat.getCols());
        }
        return det;
    }
}