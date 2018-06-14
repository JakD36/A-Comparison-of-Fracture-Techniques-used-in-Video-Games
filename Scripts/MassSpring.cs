using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSpring : MonoBehaviour {

	// Public Variables for inspector
	[Tooltip("The name of the file, containing the volumetric mesh information (in .vol format).")]
	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format).
	
	[Tooltip("Number of vertices that ")]    
	public int NumberOfCommonVertices;
	public float MaxSpringLengthRatio;

	public float poreStiffness;
	
	// Prefabs
	public GameObject EdemElementPrefab;
	

	// Private Variables
	private List<Vector3> vertices; // The vertices of the volumetric mesh, loaded from LoadSingleton.
	private List<Vector4> elements; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
    
    private List<int> surfaceTriangles; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

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

		float startTime = Time.realtimeSinceStartup;

		createEdemElements();
		createPoreSprings();

		Debug.Log(
			"Number of elements >> "+edemElements.ToArray().Length + // Number of tetrahedral elements
			" Number of Springs >> "+poreSprings.ToArray().Length + // Number of springs 
			" Time to create >> "+(Time.realtimeSinceStartup-startTime) // Time to create the elements and the springs
			);
    }

	/// <summary>
    /// FixedUpdate is called every fixed framerate frame.
	/// <para>
	/// Where we will be performing the calculations to get the spring force that holds the whole thing together
	/// 
	/// Using FixedUpdate instead of Update as 
	/// "FixedUpdate should be used instead of Update when dealing with Rigidbody. For example when adding a force to a rigidbody, 
	/// you have to apply the force every fixed frame inside FixedUpdate instead of every frame inside Update." - From Unity's own documentation 
	/// </para>
	/// </summary>
    void FixedUpdate()
    { 
		// Loop through all the active springs check if they are broken or if the force needs to be calculated.
		foreach (var poreSpring in poreSprings){
			if(poreSpring.getLengthRatio()>MaxSpringLengthRatio){ // If the spring has extended past its max then remove it
				poreSpring.breakSpring(); // record that its broken, so we can remove them from the list of active springs safely
			}else{
				poreSpring.update(); // calculate the force acting on each attached element
			}
		}
		// Check for each of the broken springs, and remove them safely by going through list backwards
		for(int n = poreSprings.Count-1; n >= 0; n--){
			if(poreSprings[n].isBroken()){
				poreSprings.Remove(poreSprings[n]);
				Debug.Log("Removed Pore Spring");
			}
		}
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
			GameObject edem = (GameObject)Instantiate<GameObject>(EdemElementPrefab);
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
			

			// transform.rotation.eulerAngles
			
			edem.transform.position = transform.position+transform.rotation*(tetCentre); 
			edem.transform.rotation = transform.rotation;



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
			
			SphereCollider sphereCollider = edem.GetComponent<SphereCollider>();
			MeshCollider meshCollider = edem.GetComponent<MeshCollider>();
			
			if(sphereCollider.enabled){
				sphereCollider.radius = minDistance;
				sphereCollider.contactOffset = 0.001f;
			}else{
				if(!meshCollider.enabled){
					meshCollider.enabled = true;
				}
				meshCollider.sharedMesh = mesh;
			}
			
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
				if(count >= NumberOfCommonVertices){
					List<PoreSpring> springs = edemElements[n].GetComponent<EdemElement>().poreSprings;
					PoreSpring newSpring = new PoreSpring(edemElements[n],edemElements[m],poreStiffness);
					springs.Add(newSpring);
					poreSprings.Add(newSpring);
				}
			}
		}		
	}
}