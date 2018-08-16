using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// The Finite element Gameobject script
/// <para>
/// Handles looping through the finite element simulation, running the conjugate gradient solver for each Fixed update 
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


	// GPU shader stuff
	public ComputeShader shader;
    private int kernel;

	elementData[] data;
	elementData[] output;

	CoordinateList[] COOStiffness;


	

	struct elementData{
		public Matrix4x4 unDeformedPos;
		public Matrix4x4 deformedPos;
		public Matrix4x4 elasticForceOnNode;
		public Matrix4x4 areaWeightNormals;		
		public Vector4 elementIndices;
		
	};

	struct CoordinateList{
		Matrix4x4 jacobian;
		float row;
		float col;
	};

	struct compressedSparseRow{ // CRS ideal for row vector multiplications // will be the length of the number of non-zeros
		Matrix4x4 jacobian;
		float JA; // columns for each member in the CRS structure
	};


	// Correct p value 


	struct compressedSparseRow2{ // Contains the start point along the compressed sparse row where row i begins, will be length of m+1 where m is number of rows, therefore requires its own struct
		float rowIndices; 
	};

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
		kernel = shader.FindKernel("FEA");
		Debug.Log(sizeof(float));
		Debug.Log( (256-sizeof(float)*12*4)/4 );
	}
	
	/// <summary>
	///
	/// <para>
	/// 
	/// </para>
	/// </summary>
	void FixedUpdate () {
		
		performFEA();
	
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
		
	}

	private void performFEA(){
		
		
		ComputeBuffer buffer = new ComputeBuffer(data.Length, 272); // 640 Bytes! PER ELEMENT!4
		
        
		shader.SetFloat("mass",1);
		shader.SetFloat("lambda",lambda);
		shader.SetFloat("mu",mu);
		
		buffer.SetData(data);
		shader.SetBuffer(kernel,"dataBuffer",buffer);
		
		ComputeBuffer KBuffer = new ComputeBuffer(COOStiffness.Length,72);
        KBuffer.SetData(COOStiffness);
		shader.SetBuffer(kernel,"KBuffer",KBuffer);
        
		shader.Dispatch(kernel,data.Length,1,1);
		
		buffer.GetData(output);
        buffer.Dispose();
		
		KBuffer.GetData(COOStiffness);

		// Now we have the unsorted coordinate list 
		// need to sort it into row major order, summing any potential duplicates 




		// Debug memory allocation and retrieval
		Debug.Log(output.Length);
		Debug.Log("Set 1");
		Debug.Log(output[0].unDeformedPos.ToString());
		Debug.Log(output[0].deformedPos.ToString());
		Debug.Log(output[0].elasticForceOnNode.ToString());
		Debug.Log(output[0].areaWeightNormals.ToString());
		Debug.Log("Set 2");
		Debug.Log(output[1].unDeformedPos.ToString());
		Debug.Log(output[1].deformedPos.ToString());
		Debug.Log(output[1].elasticForceOnNode.ToString());
		Debug.Log(output[1].areaWeightNormals.ToString());
		
		// Craft our row compressed stiffness matrix!
	}

	// private void conjugateGradientSolver(){
	// 	Debug.Log("Running Solver");
	// 	// Set up velocities vector
	// 	Vector nodeVelocities = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
	// 	Vector nodeDisplacements = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
	// 	Vector nodeVertex = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
		

	// 	Vector b = Time.fixedDeltaTime * extForce + massMatrix*nodeVelocities - Time.fixedDeltaTime * stiffnessMatrix * (nodeDisplacements - nodeVertex);
	// 	Matrix A = massMatrix + Time.fixedDeltaTime*dampeningMatrix+Mathf.Pow(Time.fixedDeltaTime,2)*stiffnessMatrix;

	// 	List<Vector> residuals = new List<Vector>() {};
	// 	List<Vector> searchDirection = new List<Vector>() {};
	// 	residuals.Add(b - A * nodeVelocities);
	// 	searchDirection.Add(residuals[0]);
	// 	float error = 99.0f;
	// 	int n = 0;
	// 	Vector newNodeVelocities = new Vector(GetComponent<MeshFilter>().mesh.vertices.Length*3);
	// 	do{
	// 		float alpha = Vector.Dot(residuals[n],residuals[n])/Vector.Dot(searchDirection[n],A*searchDirection[n]);
	// 		newNodeVelocities = nodeVelocities + alpha*searchDirection[n];
	// 		residuals.Add(residuals[n] - alpha*A*searchDirection[n]);
	// 		// Find error in residuals
	// 		error = 10; // thatll do for now

	// 		// Find new search direction
	// 		float beta = Vector.Dot(residuals[n+1],residuals[n+1])/Vector.Dot(residuals[n],residuals[n]);
	// 		searchDirection.Add(residuals[n+1] + beta*searchDirection[n]);
			
	// 		// Progress forward
	// 		n++;
	// 		nodeVelocities = newNodeVelocities.Clone();

	// 	}while(error>0.001 || n < maxIterations);
		
	// 	// Give nodes velocity calculate new position!		

	// }

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
		// FEMElements = new List<FEMElement> {};
		

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

		
		//-------------------------------- Deformation --------------------------------
		
		data = new elementData[elements.ToArray().Length];
		output = new elementData[elements.ToArray().Length];
		COOStiffness = new CoordinateList[elements.ToArray().Length*10];
		


		
		for(int n = 0; n < data.Length; n++){
			
			data[n].unDeformedPos = new Matrix4x4();
			data[n].deformedPos = new Matrix4x4();
			data[n].areaWeightNormals = new Matrix4x4();
			data[n].elasticForceOnNode = new Matrix4x4();
			
			data[n].elementIndices = elements[n];


		
			for(int m = 0; m < 3; m++){
				data[n].unDeformedPos[m,0] = mesh.vertices[(int)elements[n].x-1][m];
				data[n].unDeformedPos[m,1] = mesh.vertices[(int)elements[n].y-1][m];
				data[n].unDeformedPos[m,2] = mesh.vertices[(int)elements[n].z-1][m];
				data[n].unDeformedPos[m,3] = mesh.vertices[(int)elements[n].w-1][m];
			}
			data[n].deformedPos = data[n].unDeformedPos;
		}


		float nodeMass = GetComponent<Rigidbody>().mass/mesh.vertices.Length;
	}
}

