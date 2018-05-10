using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefractured : MonoBehaviour
{

    public string meshFile;
    private List<Vector4> elements;
    private List<Vector3> vertices;
    private List<int> surfaceTriangles;

    public List<GameObject> fragments;

    bool broken = false;


    // Use this for initialization
    void Start()
    {
        vertices = new List<Vector3> { };
        surfaceTriangles = new List<int> { };
        elements = new List<Vector4> { };

        LoadSingleton instance = LoadSingleton.getInstance();
        instance.loadFile(meshFile, ref vertices, ref surfaceTriangles, ref elements);

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        mesh.vertices = vertices.ToArray(); // Convert to an array and add to our mesh
        mesh.triangles = surfaceTriangles.ToArray(); // Convert to an array and add to our mesh

        mesh.RecalculateNormals(); // SO that materials, and light displayed properly we need normals, use recalculate normals to get this

		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshCollider>().convex = true;
    }

    // Update is called once per frame
    void Update()
    {

        // If we press space we will delete the old surface mesh and create lots of little fragments
        if (Input.GetKey(KeyCode.Space))
        {

            if (!broken)
            {
                fragments = new List<GameObject> { };

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

                    List<Vector3> tetraVert = new List<Vector3> { };

					for(int n = 0;n < 4; n++){
						Vector3 verts = vertices.ToArray()[(int)item[n] - 1];
						tetraVert.Add(verts);
						
					}				

                    int[] tetraTriangles = { 0, 1, 2,   1, 3, 2,    0, 2, 3,   0, 3, 1 };
                    fragMesh.vertices = tetraVert.ToArray(); // Convert to an array and add to our mesh
                    fragMesh.triangles = tetraTriangles; // Convert to an array and add to our mesh

                    fragMesh.RecalculateNormals(); // So that materials, and light displayed properly we need normals, use recalculate normals to get this

					frag.AddComponent<MeshCollider>();
					frag.GetComponent<MeshCollider>().sharedMesh = fragMesh;
					frag.GetComponent<MeshCollider>().convex = true;

                    frag.transform.position = transform.position;
                    fragments.Add(frag);
                }
                Mesh mesh = GetComponent<MeshFilter>().mesh;
                mesh.Clear();
				Destroy(GetComponent<MeshCollider>());
				
				broken = true;
            }
        }
    }
}
