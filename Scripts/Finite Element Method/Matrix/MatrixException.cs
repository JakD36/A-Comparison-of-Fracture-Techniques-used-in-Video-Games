using System;

public class MatrixException : System.Exception { 
    
    /// <summary>
    /// Basic constructor to take the message that is causing the exception
    /// </summary>
    public MatrixException(string msg) : base(msg){}
}