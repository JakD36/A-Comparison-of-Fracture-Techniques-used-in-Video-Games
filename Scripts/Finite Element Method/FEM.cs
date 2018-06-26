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
	public float youngsModulus;
	public float poissonsRatio;


	private Matrix stiffnessMatrix;
	private Matrix massMatrix;
	private Matrix dampeningMatrix;

	List<FEMElement> FEMElements;
	

	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Initiates the gameObjects mesh, using the createInitialMesh method!
	/// </para>
	/// </summary>
	void Start () {
		createInitialMesh();
		
		

		try{
			
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
		FEMElements = new List<FEMElement> {};
		Node[] Nodes;

		List<Vector3> vertices = new List<Vector3> { };
        List<int> surfaceTriangles = new List<int> { };
        List<Vector4> elements = new List<Vector4> { };

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

		Nodes = new Node[mesh.vertices.Length];

		for(int n = 0; n < mesh.vertices.Length; n++){
			Nodes[n] = (new Node(mesh.vertices[n]));
		}
		foreach (Vector4 element in elements){
			FEMElements.Add(new FEMElement(Nodes[(int)element.x],Nodes[(int)element.y],Nodes[(int)element.z],Nodes[(int)element.w]));
		}

	}
}
