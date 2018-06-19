using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///
/// <para>
/// 
/// </para>
/// </summary>
public class FEM : MonoBehaviour {

	// Files
	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format). Public so that it can be declared in Unity Inspector.
    
	// Variables
	public float materialToughness;

	public Matrix stiffnessMatrix;
	public Matrix massMatrix;
	public Matrix dampeningMatrix;

	
	private List<Vector3> vertices; // The vertices of the volumetric mesh, loaded from LoadSingleton.
	private List<Vector4> elements; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
    
    private List<int> surfaceTriangles; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Initiates the gameObjects mesh, using the createInitialMesh method!
	/// </para>
	/// </summary>
	void Start () {
		createInitialMesh();
		stiffnessMatrix = new Matrix(vertices.ToArray().Length, vertices.ToArray().Length);
		
		float[,] array = {{2,2}, {5,-1}};
		// float[,] array = {{1,-3,3},{3,-5,3},{6,-6,4}};//{{52,30,49,28},{30,50,8,44},{49,8,46,16},{28,44,16,22}};
		Matrix testMat = new Matrix(array);

		try{
			Debug.Log( Matrix.printMatrix(testMat));
			Debug.Log( Matrix.printMatrix(testMat.QRdecomp()[0]));
			Debug.Log( Matrix.printMatrix(testMat.QRdecomp()[1]));
			Debug.Log( Matrix.printMatrix(testMat.calculateEigen()[0]));
			Debug.Log( Matrix.printMatrix(testMat.calculateEigen()[1]));
			Debug.Log( Matrix.printMatrix(testMat));
		}catch(MatrixException e){
			Debug.Log(e.Message);
		}
		
	}
	
	/// <summary>
	///
	/// <para>
	/// 
	/// </para>
	/// </summary>
	void fixedUpdate () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		for(int n = 0; n < mesh.vertices.Length; n++){
			for(int m = 0; n < mesh.vertices.Length; m++){
				// Check to see if the two vertices are part of the same ELEMENT!
				bool sameElement = false;
				if(n!=m && sameElement){
					
				}
				else{
					stiffnessMatrix[n,m] = 0.0f;
				}

				// Now built the stiffness matrix 

				// Use conjugate gradient method to find v+ 
				// We know M C K need to find forces and v+ 

				// REALLY NEED TO LEARN FEM FUUUUUUUUCK
			}	
		}
	}

	/// <summary>
	/// OnCollisionEnter is called when this collider/rigidbody has begun
	/// touching another rigidbody/collider.
	/// </summary>
	/// <param name="other">The Collision data associated with this collision.</param>
	void OnCollisionEnter(Collision other)
	{
		
	}


	/// <summary>
	/// Creates the mesh from .vol file, and applies it to the mesh collider for the gameObject
	/// <para>
	/// Uses the LoadSingleton to get the vertices, surface triangles and tetrahedral elements.
	/// Once loaded it applies these vertices and surface triangles to the mesh's vertices and triangles before calculating the normals.
	/// Finally it adds the mesh to the mesh collider.
	/// </para>
	/// <see cref="https://docs.unity3d.com/ScriptReference/Mesh.html">For information on procedurally generating meshes in Unity</see>
	/// </summary>
	private void createInitialMesh(){

		// Initialise lists
		vertices = new List<Vector3> { };
        surfaceTriangles = new List<int> { };
        elements = new List<Vector4> { };

		// Grab an instance of the volumetric loader
		LoadSingleton instance = LoadSingleton.getInstance();
		// Get the vertices, surface triangles indices and the tetrahedral element indices
        instance.loadFile(meshFile, ref vertices, ref surfaceTriangles, ref elements);

        Mesh mesh = GetComponent<MeshFilter>().mesh; // Grab the mesh of the current game component.

        mesh.Clear(); // Clear the mesh to start fresh just in case.

        mesh.vertices = vertices.ToArray(); // Convert to an array and add to our mesh
        mesh.triangles = surfaceTriangles.ToArray(); // Convert to an array and add to our mesh

        mesh.RecalculateNormals(); // So that materials, and light displayed properly we need normals, use recalculate normals to get this

		// Pass the mesh to the mesh collider! and tell it that convex is true so that the mesh collider can be used with rigid bodies, otherwise rigidbodies has to be set to kinematic
		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshCollider>().convex = true;
		

		// Also want to set up the Matrices for the calculations


	}
}
