using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class LoadSingleton{
	private static LoadSingleton instance;

	private enum mode {points, elements, surface, none}; // We have 3 types of data in a .vol file points, elements and surface, use this to determine what we are reading 
	// Use this for initialization
	private LoadSingleton() {}

	///<summary>
	/// Singleton getInstance method
	///<see cref="https://msdn.microsoft.com/en-gb/library/ff650316.aspx">
	///</summary>	
	public static LoadSingleton getInstance(){
		if (instance == null)
		{
			instance = new LoadSingleton();
		}
		return instance;
	}


	///<summary>
	/// Load the vertices and triangles from the file
	///</summary>
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
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
					if(outSplit.Length==3){ // We should have 3 floats for our vertex
						newVertices.Add(new Vector3(float.Parse(outSplit[0]),float.Parse(outSplit[1]),float.Parse(outSplit[2])));
					}
				}
				else if(myMode == mode.surface){ // Similar to points, but this time its the indices for each vertex on each triangle
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
					if(outSplit.Length==11){
						newTriangles.Add(int.Parse(outSplit[5])-1); // Array in file starts at 1, so need to subtract 1
						newTriangles.Add(int.Parse(outSplit[6])-1); // For some reason the triangle indices are start at position 5 in the string
						newTriangles.Add(int.Parse(outSplit[7])-1);
					}
				}
				else if(myMode == mode.elements){
					outSplit = Regex.Replace(line.Trim(), @"\s+", " ").Split(' ');
					if(outSplit.Length==6){
						newElements.Add(new Vector4(float.Parse(outSplit[2]),float.Parse(outSplit[3]),float.Parse(outSplit[4]),float.Parse(outSplit[5])));
					}
				}
			}
        }
        file.Close(); // always makes sure to close the file 

		
	}


}
