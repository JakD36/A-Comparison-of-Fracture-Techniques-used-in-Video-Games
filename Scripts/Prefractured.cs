using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 
public class Prefractured : MonoBehaviour
{
	public bool useSphereCollider;
	public string meshFile; // The name of the file, containing the volumetric mesh information (in .vol format). Public so that it can be declared in Unity Inspector.
    private List<Vector3> vertices; // The vertices of the volumetric mesh, loaded from LoadSingleton.
	private List<Vector4> elements; // Each Vector4 stores the indices of the vertices that make up that tetrahedral element.
    
    private List<int> surfaceTriangles; // Each group of 3 ints in the List correspond to the indices of the surface triangles.

	public float breakForce;
	

	/// <summary>
	/// Use this for initialization
	/// <para>
    /// Unity's built in method called once an object is created.
	/// Initiates the gameObjects mesh, using the createInitialMesh method!
	/// </para>
	/// </summary>
    void Start()
    {
		createInitialMesh();
    }

	/// <summary>
    /// Update is called once per frame
	/// <para>
	/// Unity's built in method called once per frame.
	/// </para>
	/// </summary>
    void Update()
    {
        // If we press space we will delete the current gameObject and create lots of little fragments
        if (Input.GetKey(KeyCode.Space))
        {
			fracture();		
        }
    }




	/// <summary>
	/// When a collision occurs this function is executed!
	/// <para>
	/// Calculates the total force from the impulse, if the total force is above the break value, starts fracture!
	/// </para>
	/// </summary>
	/// <param name="collision">Unity object containing information on the contact points, the impact velocity, impulse etc</param>
	void OnCollisionEnter(Collision collision){
		float totalForce = (collision.impulse/Time.fixedDeltaTime).magnitude;
		// Debug.Log(gameObject.name + " hit with force >> "+ totalForce);
		if(totalForce > breakForce){
			fracture();
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

        Mesh mesh = GetComponent<MeshFilter>().mesh; // Grab the mesh of the current game component.

        mesh.Clear(); // Clear the mesh to start fresh just in case.

        mesh.vertices = vertices.ToArray(); // Convert to an array and add to our mesh
        mesh.triangles = surfaceTriangles.ToArray(); // Convert to an array and add to our mesh

        mesh.RecalculateNormals(); // So that materials, and light displayed properly we need normals, use recalculate normals to get this

		// Pass the mesh to the mesh collider! and tell it that convex is true so that the mesh collider can be used with rigid bodies, otherwise rigidbodies has to be set to kinematic
		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshCollider>().convex = true;
	}





	/// <summary>
	/// Fractures the current gameObject, into tetrahedral fragments.
	/// <para>
	/// Fractures the current gameObject, into the tetrahedral elements loaded from the .vol file previously.
	/// Each fragment is its own GameObject with its own mesh and mesh collider.
	/// This method gets the vertices for each element and supplies them and the indices for the triangles to the gameObjects mesh, 
	/// calculates its normals before passing the mesh to the gameObjects mesh collider.
	/// The final step is to delete the original gameObject as it has been fully replaced.
	/// </para>
	/// </summary>
	private void fracture(){
		foreach (Vector4 item in elements)
			{
				GameObject frag = new GameObject();
				frag.AddComponent<MeshFilter>();
				frag.AddComponent<MeshRenderer>();
				frag.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse")); // Add a material to make it pretty
				frag.AddComponent<Rigidbody>();

				Mesh fragMesh = frag.GetComponent<MeshFilter>().mesh;
				fragMesh.Clear();

				// Now we just need the vertices of the tetrahedron!

				Vector3[] tetraVert = new Vector3[4];

				for(int n = 0;n < 4; n++){
					Vector3 verts = vertices.ToArray()[(int)item[n] - 1];
					tetraVert[n] = verts;
				}				

				int[] tetraTriangles = { 0, 1, 2,   1, 3, 2,    0, 2, 3,   0, 3, 1 };
				fragMesh.vertices = tetraVert; // Convert to an array and add to our mesh
				fragMesh.triangles = tetraTriangles; // Convert to an array and add to our mesh

				fragMesh.RecalculateNormals(); // So that materials, and light displayed properly we need normals, use recalculate normals to get this


				if(useSphereCollider){ // Use a sphere collider!
					/* 
					To use the sphere collider we need to know the size and position of the sphere.
					The centre of the tetrahedron will be the centre of the sphere,
					the radius will be the minimum distance to a surface of the tet or the average, i'm undecided at this point
					*/
					// TODO: decide on average or shortest distance for sphere radius!

					// Find the centre of this tetrahedron
					Vector3 tetCentre = new Vector3();
					for(int n = 0; n < 4; n++){
						tetCentre += tetraVert[n]; // Sum the values of the vertices
					}
					tetCentre/=4; // Divide by the number of vertices to get the average position!

					// Now to find the distance to each surface using half plane tests!
					float avgDistance = new float();
					for(int n = 0; n < 4; n++){
						avgDistance += Vector3.Dot(-fragMesh.normals[n],tetCentre-fragMesh.vertices[fragMesh.triangles[n*3]]);	// sum distances!
					}
					avgDistance /= 4; // Find average!
					
					
					frag.AddComponent<SphereCollider>();
					SphereCollider collider = frag.GetComponent<SphereCollider>();
					collider.center = tetCentre;
					collider.radius = avgDistance;
				}else{ // Use a mesh Collider!
					frag.AddComponent<MeshCollider>();
					frag.GetComponent<MeshCollider>().sharedMesh = fragMesh;
					frag.GetComponent<MeshCollider>().convex = true;
				}
				
				
				
				// Set the fragments position to that of the original object, its vertex cordinates will offset it accordingly
				frag.transform.position = transform.position;
				frag.transform.rotation = transform.rotation; // Needs the appropriate rotation as well
				frag.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;
				frag.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
				frag.GetComponent<Rigidbody>().mass = GetComponent<Rigidbody>().mass/elements.ToArray().Length; // Will use the average mass for each fragment!
				/*
				It may be required to alter the vertices so that they are centre around their own origin, 
				rather than that of the original object. 
				To do this would require adding the difference between centre of the 4 vertices and the origin to the position of the fragment,
				before this is added to the position this difference vector would have to be rotated accordingly to take into account its actual position in the world space 
				*/

			}
			// Mesh mesh = GetComponent<MeshFilter>().mesh;
			// mesh.Clear();
			Destroy(gameObject); // Destroys the current object as all fragments have been created!
	}
}
