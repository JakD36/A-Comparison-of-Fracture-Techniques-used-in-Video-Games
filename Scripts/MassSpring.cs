using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSpring : MonoBehaviour {


	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format). Public so that it can be declared in Unity Inspector.
    private List<Vector3> vertices; // The vertices of the volumetric mesh, loaded from LoadSingleton.
	private List<Vector4> elements; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
    
    private List<int> surfaceTriangles; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

	public GameObject EdemElement;

	public float stiffness;

	private List<PoreSpring> allPoreSprings;

	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Initiates the gameObjects mesh, using the createInitialMesh method!
	/// </para>
	/// </summary>
    void Start()
    {
		createInitialMesh(); // Mesh has been created next is to attach each neighbouring element with springs
    }

	/// <summary>
    /// Update is called once per frame
	/// <para>
	/// Unity's built in method called once per frame.
	/// </para>
	/// </summary>
    void FixedUpdate()
    {
        foreach (var poreSpring in allPoreSprings){
			if(poreSpring.getLengthRatio()>3f){
				allPoreSprings.Remove(poreSpring);
			}else{
				poreSpring.update();
			}
		}
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

		// Now we have the object as a whole time to split it into pieces

		List<GameObject> edemElements = new List<GameObject>{};
		allPoreSprings = new List<PoreSpring>{};

		foreach (Vector4 item in elements){
			GameObject edem = (GameObject)Instantiate<GameObject>(EdemElement);
			edemElements.Add(edem);
			
			
			edem.GetComponent<Rigidbody>().mass = GetComponent<Rigidbody>().mass/elements.ToArray().Length;

			Mesh mesh = edem.GetComponent<MeshFilter>().mesh;
			mesh.Clear();

			Vector3[] tetraVert = new Vector3[4];

			for(int n = 0;n < 4; n++){
				Vector3 verts = vertices.ToArray()[(int)item[n] - 1];
				tetraVert[n] = verts;
			}				

			Vector3 tetCentre = new Vector3();
			for(int n = 0; n < 4; n++){
				tetCentre += tetraVert[n]; // Sum the values of the vertices
			}
			tetCentre/=4; // Divide by the number of vertices to get the average position!
			for(int n = 0; n < 4; n++){
				tetraVert[n]-=tetCentre; // Make sure the verts are centred to this object, not the whole system
			}
			
			edem.transform.position = transform.position+tetCentre;

			int[] triangles = { 0, 1, 2,   1, 3, 2,    0, 2, 3,   0, 3, 1 };
				
			mesh.vertices = tetraVert;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();

			// Now to find the distance to each surface using half plane tests!
			float minDistance = Mathf.Abs(Vector3.Dot(-mesh.normals[0],mesh.vertices[mesh.triangles[0]]));
			for(int n = 1; n < 4; n++){
				float newVal = Mathf.Abs(Vector3.Dot(-mesh.normals[n],mesh.vertices[mesh.triangles[n*3]])); 
				if(newVal < minDistance){
					minDistance = newVal;
				}
			}
			
			edem.GetComponent<EdemElement>().radius = minDistance;
			// edem.GetComponent<SphereCollider>().radius = minDistance;
			edem.GetComponent<MeshCollider>().sharedMesh = mesh;
			edem.GetComponent<EdemElement>().elements = item;
		}
		

		for(int n = 0; n < elements.ToArray().Length; n++){ // Loop through tetrahedral elements
			for(int m = n+1; m < elements.ToArray().Length; m++){ // needs to loop through every other tetrahedral element, that hasnt already been checked
			
				int count = 0;
				// To be neighbours need to share 3 out of 4 of the tetrahedral neighbours
				// So loop through the 4 numbers twice to check the
				for(int nodeA = 0; nodeA < 4; nodeA++){
					for(int nodeB = 0; nodeB < 4; nodeB++){
						if(elements.ToArray()[n][nodeA] == elements.ToArray()[m][nodeB]){
							count++;break;
						}
					}	
				}
				if(count == 3){
					List<PoreSpring> springs = edemElements[n].GetComponent<EdemElement>().poreSprings;
					PoreSpring newSpring = new PoreSpring();
					newSpring.elements[0] = edemElements[n];
					newSpring.elements[1] = edemElements[m];
					newSpring.stiffness = this.stiffness;
					if(edemElements[n].transform.position == edemElements[m].transform.position){
						Debug.Log("Position matches at >> "+edemElements[n].transform.position.x +" "+edemElements[n].transform.position.y+ " "+edemElements[n].transform.position.z);
					}
					newSpring.restLength = (edemElements[n].transform.position - edemElements[m].transform.position).magnitude;
					Debug.Log(newSpring.restLength);
					springs.Add(newSpring);
					allPoreSprings.Add(newSpring);
				}
			}
		}		
		Debug.Log("Number of springs in mass spring system >>"+allPoreSprings.ToArray().Length);
		
		// Destroy(gameObject);
	}
}