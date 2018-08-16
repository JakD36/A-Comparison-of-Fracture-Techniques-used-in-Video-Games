using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System.IO;

public class Test : MonoBehaviour {

	// Prefabs
	public GameObject TestObjectPrefab;	
	public GameObject CannonBallPrefab;

	
	
	// Variables
	public float force; // The force to fire the cannonball
	public float cannonBallMass; // The mass of the cannonballs when fired
	public float cannonBallRadius; // The size of the cannonballs
	GameObject subject;
	GameObject cannonBall;

	public string filename;
    StreamWriter sw;
	StreamWriter mw;
	int count = 0;
	bool setup = false;
	
	float experimentTime = 10.0f;

    public void writeToFile(string txt){
        sw.Write(txt+" ");
        sw.Flush();
    }

    public void newLine(){
        sw.Write("\n");
        sw.Flush();
		// mw.Write("\n");
        // mw.Flush();
    }

    void Start(){
        sw = File.AppendText(filename+".txt");
		// mw = File.AppendText("mem"+filename+".txt");
    }

	// Update is called once per frame
	void Update () {	
		if(count<100){
			experimentTime += Time.deltaTime;
			
			if(experimentTime > 5 && !setup){
				newLine();
				
				subject = (GameObject)Instantiate<GameObject>(TestObjectPrefab);
				setup = true;
				cannonBall = (GameObject)Instantiate<GameObject>(CannonBallPrefab);
				
				cannonBall.transform.position = new Vector3(-20,10,-20);
				
				cannonBall.GetComponent<Rigidbody>().mass = cannonBallMass;
				cannonBall.transform.localScale*=cannonBallRadius;
				cannonBall.GetComponent<SphereCollider>().radius = 0.5f; 
				cannonBall.GetComponent<Rigidbody>().AddForce(
					(subject.transform.position+new Vector3(0,0.5f,0)-cannonBall.transform.position).normalized*force, 
					ForceMode.Impulse);
				Destroy(cannonBall,3); 
				experimentTime = 0;
			}

			writeToFile(Time.deltaTime.ToString());
			// mw.Write(Profiler.GetTotalAllocatedMemory()+" ");
        	// mw.Flush();
		}
		if(subject == null && cannonBall == null){
			setup = false;
			count++;
		}
	}
}
