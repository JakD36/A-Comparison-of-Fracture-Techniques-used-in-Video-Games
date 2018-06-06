using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSpring : MonoBehaviour {


	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format). Public so that it can be declared in Unity Inspector.
    private List<Vector3> vertices; // The vertices of the volumetric mesh, loaded from LoadSingleton.
	private List<Vector4> elements; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
    
    private List<int> surfaceTriangles; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

	public GameObject EdemElement;


	public float poreStiffness;
	public float contactStiffness;
	public float normalContactDampening;
	public float tangentialContactDampening;

	private List<GameObject> edemElements;
	private List<PoreSpring> poreSprings;

	/// <summary>
	/// Use this for initialization, gets the mesh information from assigned file
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Gets the mesh information using the loadSingleton, before calling the createEdemElements to apply the mesh information to each tetrahedral element
	/// Once completed calls the createPoreSprings method to link all the elements together
	/// </para>
	/// </summary>
    void Start()
    {
		// Initialise lists
		vertices = new List<Vector3> { };
        surfaceTriangles = new List<int> { };
        elements = new List<Vector4> { };

		LoadSingleton instance = LoadSingleton.getInstance(); // Grab an instance of the volumetric loader
        instance.loadFile(meshFile, ref vertices, ref surfaceTriangles, ref elements); // Get the vertices, surface triangles indices and the tetrahedral element indices


		// Now we have the object as a whole time to split it into pieces

		edemElements = new List<GameObject>{};
		
		poreSprings = new List<PoreSpring>{};

		createEdemElements();
		createPoreSprings();
    }

	/// <summary>
    /// FixedUpdate is called every fixed framerate frame.
	/// <para>
	/// Where we will be performing the steps required to simulate the motion of all the edem elements in the system
	/// Following these steps
	/// (1) Calculate external forces
	///
	/// (2) Move elements by external forces
	///
	/// (3) Move elements by contact force
	///
	/// (4) Move elements by pore spring
	///
	/// (5) Perform collision detection and response with other EDEM objects
	///
	/// (6) Perform collision detection and response with polygonal objects for the floor
	/// 
	/// Using FixedUpdate instead of Update as 
	/// "FixedUpdate should be used instead of Update when dealing with Rigidbody. For example when adding a force to a rigidbody, 
	/// you have to apply the force every fixed frame inside FixedUpdate instead of every frame inside Update." - From Unity's own documentation 
	/// </para>
	/// </summary>
    void FixedUpdate()
    { 
        // (1) Calculate external forces - no need accelaration constant

		// (2) Move elements by external forces
		// foreach (var edem in edemElements){
		// 	Transform transform = edem.transform;
		// 	Rigidbody rigidbody = edem.GetComponent<Rigidbody>();
		// 	Vector3 velocity = rigidbody.velocity;

		// 	// External Force
		// 	Vector3 grav = new Vector3(0.0f,-9.81f,0.0f); // -9.81 is the acceleration due to gravity!
		// 	// New velocity is equal to old velocity + acceleration * time
		// 	velocity += grav * Time.fixedDeltaTime;
		// 	transform.Translate(velocity);
		// }

		
		// // With all contact forces calculated now move each element
		// foreach (var edem in edemElements){
		// 	edem.GetComponent<EdemElement>().updateByContact();
		// }

		// (4) Move Elements by pore spring
		foreach (var poreSpring in poreSprings){
			if(poreSpring.getLengthRatio()>3f){
				poreSprings.Remove(poreSpring);
			}else{
				poreSpring.update();
			}
		}

		// (6) Perform collision test with ground!

    }



	/// <summary>
	///
	/// <para>
	///
	/// </para>
	/// <see cref="https://docs.unity3d.com/ScriptReference/Mesh.html">For information on procedurally generating meshes in Unity</see>
	/// </summary>
	private void createEdemElements(){
		// Now Instantiate each edem element!
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
			tetCentre/=4; // Divide by the number of vertices to get the average position ie the centre!
			for(int n = 0; n < 4; n++){
				tetraVert[n]-=tetCentre; // Make sure the verts are centred to this object, not the whole system
			}
			
			edem.transform.position = transform.position+tetCentre; 
			// The centre is now 0!
			int[] triangles = { 0, 1, 2,   1, 3, 2,    0, 2, 3,   0, 3, 1 };
				
			mesh.vertices = tetraVert;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();

			// Now to find the distance to each surface using half plane tests!
			float minDistance = Mathf.Abs(Vector3.Dot(mesh.normals[0],mesh.vertices[mesh.triangles[0]]));
			for(int n = 1; n < 4; n++){
				float newVal = Mathf.Abs(Vector3.Dot(mesh.normals[n],mesh.vertices[mesh.triangles[n*3]])); 
				if(newVal < minDistance){
					minDistance = newVal;
				}
			}
			
			edem.GetComponent<EdemElement>().radius = minDistance;
			// edem.GetComponent<SphereCollider>().radius = minDistance;
			edem.GetComponent<MeshCollider>().sharedMesh = mesh;
			edem.GetComponent<EdemElement>().elements = item;
		}
	}


	/// <summary>
	///
	/// <para>
	///
	/// </para>
	/// </summary>
	private void createPoreSprings(){
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
					newSpring.stiffness = this.poreStiffness;
					if(edemElements[n].transform.position == edemElements[m].transform.position){
						Debug.Log("Position matches at >> "+edemElements[n].transform.position.x +" "+edemElements[n].transform.position.y+ " "+edemElements[n].transform.position.z);
					}
					newSpring.restLength = (edemElements[n].transform.position - edemElements[m].transform.position).magnitude;
					Debug.Log(newSpring.restLength);
					springs.Add(newSpring);
					poreSprings.Add(newSpring);
				}
			}
		}		
		Debug.Log("Number of springs in mass spring system >>"+poreSprings.ToArray().Length);
	}
}