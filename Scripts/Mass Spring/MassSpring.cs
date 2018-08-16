using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The MassSpring script, handles the simulation of the mass spring system.
/// <para>
/// Initialises system through createElements and createSprings
/// Updates the system by looping through all springs to get them to calculate forces, before calling each element to apply these forces
/// </para>
/// </summary>
public class MassSpring : MonoBehaviour {

	// Public Variables for inspector
	
	//Files
	[Tooltip("The name of the file, containing the volumetric mesh information (in .vol format).")]
	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format).

	// Prefabs
	public GameObject ElementPrefab;


	// Main Variables for inspector
	[Range(1,3)] // Make sure springs are added by either vertex, edge or surface, as <0 will result in every element being attached to every other and >3 will result in no springs being added
	[Tooltip("Number of vertices that must match between elements to generate a spring between them,\n1: a spring between elements with a joining vertex,\n2: a spring between elements with a joining edge,\n3: a spring between elements with a joining surface")]    
	public int NumberOfCommonVertices; // Number of vertices that must match between elements to generate a spring between them,\n1: a spring between elements with a joining vertex,\n2: a spring between elements with a joining edge,\n3: a spring between elements with a joining surface
	
	
	[Tooltip("The ratio of current length to rest length at which the spring breaks and stops providing a restoring force")]
	public float MaxSpringLengthRatio; // The ratio of current length to rest length at which the spring breaks and stops providing a restoring force

	[Tooltip("The stiffness k of each spring, providing a restoring force in the form F = kdx, where dx is the change in length from the rest length of the spring and the force is applied in the direction from the centre point between the two joined elements to the element")]
	public float stiffness; 

	public bool clearDebris;
	
	
	// Private Variables
	List<Vector4> volumeIndices; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
	private List<GameObject> elements;
	private List<Spring> springs;

	/// <summary>
	/// Use this for initialization, gets the mesh information from assigned file
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Call the createElements to apply the mesh information to each tetrahedral element
	/// Once completed calls the createSprings method to link all the elements together
	/// </para>
	/// </summary>
    void Start()
    {
		// Now we have the object as a whole time to split it into pieces

		elements = new List<GameObject>{};
		springs = new List<Spring>{};

		float startTime = Time.realtimeSinceStartup; // So we know how long it takes to load the mass spring system on startup!

		createElements();
		createSprings();

		// So we know how long it takes to load the mass spring system on startup!
		Debug.Log(
			"Number of elements >> "+elements.ToArray().Length + // Number of tetrahedral elements
			" Number of Springs >> "+springs.ToArray().Length + // Number of springs 
			" Time to create >> "+(Time.realtimeSinceStartup-startTime) // Time to create the elements and the springs
			);
    }

	/// <summary>
    /// FixedUpdate is called every fixed framerate frame.
	/// <para>
	/// Where we will be performing the calculations to get the spring force that holds the whole thing together, as well as apply them only once all forces from springs are calculated
	/// 
	/// Using FixedUpdate instead of Update as 
	/// "FixedUpdate should be used instead of Update when dealing with Rigidbody. For example when adding a force to a rigidbody, 
	/// you have to apply the force every fixed frame inside FixedUpdate instead of every frame inside Update." - From Unity's own documentation 
	/// </para>
	/// </summary>
    void FixedUpdate()
    { 
		// Loop through all the active springs check if they are broken or if the force needs to be calculated.
		foreach (var spring in springs){
			if(spring.getLengthRatio()>MaxSpringLengthRatio){ // If the spring has extended past its max then remove it
				spring.breakSpring(); // record that its broken, so we can remove them from the list of active springs safely
			}else{
				spring.calculateRestorationForce(); // calculate the force acting on each attached element
			}
		}
		// Check for each of the broken springs, and remove them safely by going through list backwards
		for(int n = springs.Count-1; n >= 0; n--){
			if(springs[n].isBroken()){
				springs.Remove(springs[n]); 
				// Debug.Log("Removed Spring");
			}
		}

		// // Now all the forces from all the springs is calculated apply them to each element.
		// foreach (var element in elements)
		// {
		// 	Element elementScript = element.GetComponent<Element>();
		// 	elementScript.applySpringForce();

		// }
    }



	/// <summary>
	/// Creates the elements of the mass spring system using the volumetric information of the volumetric mesh file
	/// <para>
	/// Gets the mesh information using the loadSingleton, to get the vertices, surface triangles and the tetrahedral elements.
	/// Then loops through the elements creating a new gameobject for each and every tetrahedral element.
	/// Assigning the vertices, and surface triangles for each. As well as the sorting the colliders.
	/// </para>
	/// <see cref="https://docs.unity3d.com/ScriptReference/Mesh.html">For information on procedurally generating meshes in Unity</see>
	/// </summary>
	private void createElements(){

		List<Vector3> vertices = new List<Vector3> { }; // The vertices of the volumetric mesh, loaded from LoadSingleton.
		
    
    	List<int> surfaceTriangles = new List<int> { }; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

        volumeIndices = new List<Vector4> { };

		LoadSingleton instance = LoadSingleton.getInstance(); // Grab an instance of the volumetric loader
        instance.loadFile(meshFile, ref vertices, ref surfaceTriangles, ref volumeIndices); // Get the vertices, surface triangles indices and the tetrahedral element indices

		// Now Instantiate each element!
		foreach (Vector4 elementIndices in volumeIndices){
			GameObject element = (GameObject)Instantiate<GameObject>(ElementPrefab); // Create a new gameobject from the prefab
			elements.Add(element); // Add to our list of  elements
			
			element.GetComponent<Rigidbody>().mass = GetComponent<Rigidbody>().mass/elements.ToArray().Length; // Give the element a fraction of the total mass

			Mesh mesh = element.GetComponent<MeshFilter>().mesh; // Grab the mesh object of the element, so we can assign the vertices and surface triangles
			mesh.Clear();

			Vector3[] tetraVert = new Vector3[4]; // Each tetrahedral element will have 4 vertices, so we will chuck them in this array

			for(int n = 0;n < 4; n++){
				Vector3 verts = vertices.ToArray()[(int)elementIndices[n] - 1]; // So the indices of vertices belonging to each tetrahedral element are stored in the Vector4 element, so grab each one and subtract 1 as these indices start at 1 not 0.
				tetraVert[n] = verts; // We can assign the vertex to one of the 4 in the tetrahedral element.
			}				


			// We want to find the centre point of the tetrahedral element, so find the average point of the vertices
			Vector3 tetCentre = new Vector3(); 
			for(int n = 0; n < 4; n++){
				tetCentre += tetraVert[n]; // Sum the values of the vertices
			}
			tetCentre/=4; // Divide by the number of vertices to get the average position ie the centre!

			// By subtracting the centre point from each vertexm they will now be centred around their own origin, rather than origin of the entire system
			for(int n = 0; n < 4; n++){
				tetraVert[n]-=tetCentre; // Make sure the verts are centred to this object, not the whole system
			}
			// The centre is now 0!
			
			// Now move the element, to its correct position and rotation, 
			// This is the position of the whole system, plus the distance to the centre of the vertices for this element adjusted to take into account of any rotation
			element.transform.position = transform.position+transform.rotation*(tetCentre); 
			element.transform.rotation = transform.rotation; // Simply assigning the rotation of the element.
			if(clearDebris){
				Destroy(element,4);
			}
			

			int[] triangles = { 0, 1, 2,   1, 3, 2,    0, 2, 3,   0, 3, 1 }; // The indices for surface triangles for each element 
				
			// Assign the mesh variables
			mesh.vertices = tetraVert;
			mesh.triangles = triangles;
			mesh.RecalculateNormals(); // Rather than assign the normals, let unity do it for us

			
			// Original EDEM used spherical elements, however object tends to turn into a puddle, so going to use mesh colliders
			// SphereCollider sphereCollider = element.GetComponent<SphereCollider>();
			MeshCollider meshCollider = element.GetComponent<MeshCollider>();
			
			// // if(sphereCollider.enabled){
			// // 	// Now to find the distance to each surface using half plane tests!
			// // 	float minDistance = Mathf.Abs(Vector3.Dot(mesh.normals[0],mesh.vertices[mesh.triangles[0]]));
			// // 	for(int n = 1; n < 4; n++){
			// // 		float newVal = Mathf.Abs(Vector3.Dot(mesh.normals[n],mesh.vertices[mesh.triangles[n*3]])); 
			// // 		if(newVal < minDistance){
			// // 			minDistance = newVal;
			// // 		}
			// // 	}
			// // 	sphereCollider.radius = minDistance;
			// // 	sphereCollider.contactOffset = 0.001f;
			// // 	meshCollider.enabled = false;
			// }else{
				if(!meshCollider.enabled){
					meshCollider.enabled = true;
				}
				meshCollider.sharedMesh = mesh;
			// }
		}
		if(clearDebris){
			Destroy(gameObject,4);
		}
	}


	/// <summary>
	/// Attaches the springs between the tetrahedral elements.
	/// <para>
	/// Does so by looping through all elements, and for each looping through all other elements not already checked.
	/// Uses the tetrahedral element vertex indices from the mesh file, to compare if the two elements n and m have any matching vertices.
	/// 
	/// 1 matching vertex means they are joined by a vertex
	/// 2 matching vertices means they are joined by an edge
	/// 3 matching vertices means they are joined by a surface
	/// 
	/// Uses the variable, NumberOfCommonVertices, chosen in the inspector to decide which of the 3 choices above determines if a spring is added between elements n and m.
	/// </para>
	/// </summary>
	private void createSprings(){
		for(int n = 0; n < elements.ToArray().Length; n++){ // Loop through tetrahedral elements
			for(int m = n+1; m < elements.ToArray().Length; m++){ // needs to loop through every other tetrahedral element, that hasnt already been checked
			
				int count = 0; // Number of vertices the elements share
				
				for(int nodeA = 0; nodeA < 4; nodeA++){
					for(int nodeB = 0; nodeB < 4; nodeB++){
						if(volumeIndices.ToArray()[n][nodeA] == volumeIndices.ToArray()[m][nodeB]){
							count++;break; // If an indices matches add to the count and start on the next indices by breaking out of this nested loop
						}
					}	
				}
				if(count >= NumberOfCommonVertices){ // Now add the spring if the element is at least joined by the number of vertices as specified in the inspector
					
					Spring newSpring = new Spring(elements[n],elements[m],stiffness); // create the spring, with the two elements it is between and its stiffness
					
					List<Spring> elementSprings = elements[n].GetComponent<Element>().springs; // Get the list of springs attached to the element
					elementSprings.Add(newSpring); // Add this new spring to the list of springs attached to the element

					springs.Add(newSpring); // Add the new spring to the list of springs in the system that will be looped through in FixedUpdate to do the simulation
				}
			}
		}		
		elements.Clear(); // Since we no longer need this information we can get rid of it
	}
}