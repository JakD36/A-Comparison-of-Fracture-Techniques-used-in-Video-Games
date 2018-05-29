using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


/// <summary>
/// A class to load the vertices, surface triangles and elements of a volumetric mesh from a .vol file.
/// <para>
/// For use with Unity environment, the class uses the singleton pattern and an instance of the class
/// can be retrieved with the use of the getInstance() method.
/// </para>
/// </summary>
public class LoadSingleton{
	
	// Instance of the LoadSingleton to be passed to other classes.
	private static LoadSingleton instance;

	// We have 3 types of data in a .vol file points, elements and surface, use this to determine what we are reading 
	private enum mode {points, elements, surface, none}; 
	
	/// <summary>
	///	Private Constructor to prevent a second instance being created.
	/// </summary>
	private LoadSingleton() {} 

	///<summary>
	/// Singleton getInstance method
	/// <para>
	///	Using lazy instantiation, checks to see if the instance has already been created before returning the instance, if it hasnt it creates a new one.
	/// </para>
	///<see cref="https://msdn.microsoft.com/en-gb/library/ff650316.aspx"> Uses a Property approach to returning the instance, changed to a simpler get method
	///</summary>	
	/// <returns>The instance of the LoadSingleton to be used by other classes.</returns>
	public static LoadSingleton getInstance(){
		if (instance == null)
		{
			instance = new LoadSingleton();
		}
		return instance;
	}


	/// <summary>
	/// Load the vertices, surface triangles and tetrahedral elements from the file provided.
	/// <para> 
	///	Reads each line of the file specified. Each block of data starts with a title declaring 
	/// what the next block refers to, be it the vertices, the surface triangles or the tetrahedral elements.
	/// Uses an enum to remember what the last title was and adds data to the corresponding list. 
	/// Checks each line to make sure it has the expected number of data points for the that section before trying to add the data, to avoid errors.
	/// </para>
	/// </summary>
	/// <param name="myFile">The filename of the .vol file (Including the .vol extension. For example "ico.vol") containing the volumetric mesh for the object to be fractured.</param>
	/// <param name="newVertices">A List of Vector3 passed by reference to store each of the vertices of the tetrahedral mesh.</param>
	/// <param name="newTriangles">A List of Integers passed by reference to store each of the indices of the vertices of the surface mesh.</param>
	/// <param name="newElements">A List of Vector4 passed by reference to store each of the indices of the vertices of each of the tetrahedral elements.</param>
	public void loadFile(string myFile, ref List<Vector3> newVertices, ref List<int> newTriangles, ref List<Vector4> newElements){
		string line;

		// This could be optimised if the number of vertices was read to create an array possibly
		newVertices = new List<Vector3> {}; 
		newElements = new List<Vector4> {};
		newTriangles = new List<int> {};

		string[] outSplit; // String array to store the numbers from the lines read

        StreamReader file = new StreamReader(Application.dataPath + "/"+myFile); //load text file with data
		mode myMode = mode.none; // As we do not know what will appear first assign none
		
        while ((line = file.ReadLine()) != null)
        { //while text exists.. repeat
			
			
			// Each section of numbers starts with a title, either points, volumeelements and surfaceelementsgi
			if( line.Equals("points") ){ // when we come across a line with the word points we know that the following numbers will be vertices
				myMode = mode.points;
			}
			else if( line.Equals("volumeelements")){ // similar to points 
				myMode = mode.elements;
			}
			else if( line.Equals("surfaceelementsgi")){ // similar to points
				myMode = mode.surface;
			}
			
			
			// If the line is not a title line then it could be either our numbers 
			else{  
				if(myMode == mode.points){ // now we need to read each points line and extract the 3 floats
					// There is an issue where theres a lot of extra white space, regex and trim sort this
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' '); // Regex might not be the fastest solution
					if(outSplit.Length==3){ // We should have 3 floats for our vertex
						newVertices.Add(new Vector3(float.Parse(outSplit[0]),float.Parse(outSplit[1]),float.Parse(outSplit[2])));
					}
				}
				else if(myMode == mode.surface){ // Similar to points, but this time its the indices for each vertex on each triangle
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
					if(outSplit.Length==11){ // Have 11 pieces of info for a surface, the indices are values at position 5,6 and 7
						newTriangles.Add(int.Parse(outSplit[5])-1); // Array in file starts at 1, so need to subtract 1
						newTriangles.Add(int.Parse(outSplit[6])-1); // For some reason the triangle indices are start at position 5 in the string
						newTriangles.Add(int.Parse(outSplit[7])-1);
					}
				}
				else if(myMode == mode.elements){
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
					if(outSplit.Length==6){ // Have 6 pieces of information for each element, the indices are the values at positions 2 to 5
						newElements.Add(new Vector4(float.Parse(outSplit[2]),float.Parse(outSplit[3]),float.Parse(outSplit[4]),float.Parse(outSplit[5])));
					}
				}
			}
        }
        file.Close(); // always makes sure to close the file 		
	}


}
