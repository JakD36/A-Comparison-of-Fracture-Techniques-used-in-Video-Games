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

	private float lambda;
	private float mu;

	public int maxIterations;

	Node[] Nodes;

	public Matrix stiffnessMatrix;
	private Matrix massMatrix;
	private Matrix dampeningMatrix;

	List<FEMElement> FEMElements;
	
	Vector extForce;

	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Initiates the gameObjects mesh, using the createInitialMesh method!
	/// </para>
	/// </summary>
	void Start () {
		createInitialMesh();
		lambda = ( poissonsRatio * youngsModulus * (1 - 2 * poissonsRatio) )/(1 + poissonsRatio);
		mu = youngsModulus / ( 2*( 1+poissonsRatio ) );
		
	}
	
	/// <summary>
	///
	/// <para>
	/// 
	/// </para>
	/// </summary>
	void FixedUpdate () {
		
		
		extForce[0] = 0;
	}

	/// <summary>
	/// Called when the script is loaded or a value is changed in the
	/// inspector (Called in the editor only).
	/// </summary>
	void OnValidate()
	{
		lambda = ( poissonsRatio * youngsModulus * (1 - 2 * poissonsRatio) )/(1 + poissonsRatio);
		mu = youngsModulus / ( 2*( 1+poissonsRatio ) );
	}

	/// <summary>
	/// OnCollisionEnter is called when this collider/rigidbody has begun
	/// touching another rigidbody/collider.
	/// </summary>
	/// <param name="other">The Collision data associated with this collision.</param>
	void OnCollisionEnter(Collision other)
	{
		float totalForce = (other.impulse/Time.fixedDeltaTime).magnitude;
		float numOfContacts = other.contacts.Length;
		extForce[0] = totalForce;
		performFEA();
	}

	private void performFEA(){
		for(int n = 0; n < FEMElements.ToArray().Length; n++){
			FEMElements[n].FEA();
		}
		dampeningMatrix = massMatrix + stiffnessMatrix;
		// Now we have M, C and K, we should know v,x and u
		// need to use conjugate gradient to solve for v+ here
		conjugateGradientSolver();
	}

	private void conjugateGradientSolver(){
		Debug.Log("Ran Solver");
		// Set up velocities vector
		Vector nodeVelocities = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
		Vector nodeDisplacements = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
		Vector nodeVertex = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
		for(int i = 0, j = 0; i < Nodes.Length; i++, j+=3){
			nodeVelocities[j] = Nodes[i].Velocity.x;
			nodeVelocities[j+1] = Nodes[i].Velocity.y;
			nodeVelocities[j+2] = Nodes[i].Velocity.z;

			nodeDisplacements[j] = Nodes[i].Position.x;
			nodeDisplacements[j+1] = Nodes[i].Position.y;
			nodeDisplacements[j+2] = Nodes[i].Position.z;

			nodeVertex[j] = Nodes[i].uPosition.x;
			nodeVertex[j+1] = Nodes[i].uPosition.y;
			nodeVertex[j+2] = Nodes[i].uPosition.z;
		}

		Vector b = Time.fixedDeltaTime * extForce + massMatrix*nodeVelocities - Time.fixedDeltaTime * stiffnessMatrix * (nodeDisplacements - nodeVertex);
		Matrix A = massMatrix + Time.fixedDeltaTime*dampeningMatrix+Mathf.Pow(Time.fixedDeltaTime,2)*stiffnessMatrix;

		List<Vector> residuals = new List<Vector>() {};
		List<Vector> searchDirection = new List<Vector>() {};
		residuals.Add(b - A * nodeVelocities);
		searchDirection.Add(residuals[0]);
		float error = 99.0f;
		int n = 0;
		Vector newNodeVelocities = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
		do{
			float alpha = Vector.Dot(residuals[n],residuals[n])/Vector.Dot(searchDirection[n],A*searchDirection[n]);
			newNodeVelocities = nodeVelocities + alpha*searchDirection[n];
			residuals.Add(residuals[n] - alpha*A*searchDirection[n]);
			// Find error in residuals
			error = 10; // thatll do for now

			// Find new search direction
			float beta = Vector.Dot(residuals[n+1],residuals[n+1])/Vector.Dot(residuals[n],residuals[n]);
			searchDirection.Add(residuals[n+1] + beta*searchDirection[n]);
			
			// Progress forward
			n++;
			nodeVelocities = newNodeVelocities.Clone();

		}while(error>0.001 || n < maxIterations);
		
		// Give nodes velocity calculate new position!		

	}

	public float getLambda(){
		return lambda;
	}

	public float getMu(){
		return mu;
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
		stiffnessMatrix = new Matrix(mesh.vertices.Length*3,mesh.vertices.Length*3);
		dampeningMatrix = new Matrix(mesh.vertices.Length*3,mesh.vertices.Length*3);
		massMatrix = new Matrix(mesh.vertices.Length*3,mesh.vertices.Length*3);
		extForce = new Vector(mesh.vertices.Length*3);
		for(int n = 0; n < mesh.vertices.Length; n++){
			Nodes[n] = (new Node(mesh.vertices[n],n));
		}
		foreach (Vector4 element in elements){
			
			FEMElements.Add(new FEMElement(this,Nodes[(int)element.x-1],Nodes[(int)element.y-1],Nodes[(int)element.z-1],Nodes[(int)element.w-1]));
		}
		float nodeMass = GetComponent<Rigidbody>().mass/mesh.vertices.Length;
		for(int n = 0; n < mesh.vertices.Length*3; n++){
			massMatrix[n,n] = nodeMass;
		}

	}
}

