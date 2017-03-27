using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Simulation : MonoBehaviour {

	public float deltat = 0.01f;	//Time division
	public float totalt = 3.0f;		//Total simulation time
	public float t = 0f;

	public float TEInit = 1;		//Initial total energy, used for Anderson Thermostat
	public int ExpNum = 1; 			//Experiment number, change this between experiments

	//Display outputs in the visualiser
	public Text timer;
	public Text kinEng;
	public Text potEng;
	public Text totEng;

	#region Cluster Initial Conditions
	public int cluster1_sz = 75;
	public int cluster2_sz = 23;

	//Note that these are not the values used in the simulation
	//The editor allows us to change these variables before running
	public Vector3 cluster1_r0 = Vector3.right * 5; 	//Initial displacements
	public Vector3 cluster2_r0 = Vector3.zero;

	public Vector3 cluser1_v0 = Vector3.zero;
	public Vector3 cluser2_v0 = Vector3.zero;
	#endregion

	public List<Particle> particles = new List<Particle>(); //Keep track of all particles from both clusters

	public GameObject particlePref;		//This is the object used in the visualiser as a particle, it is assigned through the editor.

	public float TempCorrection = 1f; 	//The fraction by which the system must adjust v to conserve total energy

	public float sigma = 1.0f; 			//Equilibrium bond length, set = 1 for all our purposes

	//Get centre of mass
	public Vector3 com { 
		get {
			Vector3 toRet = Vector3.zero;
			for(int i = 0; i < particles.Count; i++)
			{
				toRet += particles[i].position / particles.Count;
			}
			return toRet;
		}
	}
	//Get total kinetic energy
	public float KE { 
		get{
			float toRet = 0f;

			for(int i = 0; i < particles.Count; i++)
			{
				toRet += (0.5f * particles[i].velocity.sqrMagnitude);
			}

			return toRet;
		}
	}
	//Get total potential energy
	public float PE { 
		get{
			float toRet = 0f;

			for(int i = 0; i < particles.Count; i++)
			{
				toRet += particles[i].potential;
			}

			return toRet;
		}
	}
	//Get total energy
	public float TE { 
		get{return PE + KE;}
	}


	// Start() is a function intrinsic to the engine used, it is called before Update()
	void Start () {
		Init();
	}

	//Remove all of the particles from the visualiser to be ready to re-init at runtime
	public void Clear()
	{
		t = 0;
		for(int i = 0; i < particles.Count; i++)
		{
			Destroy(particles[i].display);
		}
		particles.Clear();
	}


	public void Init()
	{
		Clear(); //Make sure simulation is empty

		List<GameObject> gObjects = new List<GameObject>();

		particlePref.GetComponent<Renderer>().material = Resources.Load("Cluster1") as Material; //Set the material (appearance) of all particles in cluster 1

		for(int i = 0; i < cluster1_sz; i++)
		{
			gObjects.Add(Instantiate(particlePref) as GameObject);	
		}

		Cluster cluster1 = new Cluster(cluster1_sz,cluster1_r0,cluser1_v0, gObjects.ToArray());
		particles.AddRange(cluster1.particles);

		gObjects.Clear();

		particlePref.GetComponent<Renderer>().material = Resources.Load("Cluster2") as Material; //Set the material (appearance) of all particles in cluster 1

		for(int i = 0; i < cluster2_sz; i++)
		{
			gObjects.Add(Instantiate(particlePref) as GameObject);	
		}

		Cluster cluster2 = new Cluster(cluster2_sz,cluster2_r0,cluser2_v0, gObjects.ToArray());
		particles.AddRange(cluster2.particles);

		CalculatePotential();
		TEInit = TE;

		if(!System.IO.Directory.Exists(Application.streamingAssetsPath + "/Experiment" + ExpNum)) 
		{
			//If there is no experiment directory create one
			System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "/Experiment" + ExpNum);
		}
		else 
		{
			//If there is an experiment directory, clear it
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.streamingAssetsPath + "/Experiment" + ExpNum);

			foreach (System.IO.FileInfo file in di.GetFiles())
			{
			    file.Delete(); 
			}
		}

		SaveXYZ("Initial");
	}
	
	// Update is called once per frame
	void Update () {
		if(t < totalt)
		{
			VerletStep();
		}
	}

	void CalculatePotential()
	{
		for(int i = 0; i < particles.Count; i++)
		{
			//Calculate the acceleration at each interval for each particle
			Vector3 tempacc = Vector3.zero;
			float LJPot = 0f;

			for(int j = 0; j < particles.Count; j++)		//Iterating over each pair
			{
				if(i != j)
				{
					Vector3 disp = particles[j].position - particles[i].position;		//Distance between pair
					float r = disp.magnitude;

					LJPot += 4*(Mathf.Pow(sigma/r,12) - Mathf.Pow(sigma/r,6));
				}
			}

			particles[i].potential = LJPot;
		}
	}

	void VerletStep() {
		t += deltat;
		timer.text = "time : " + t;
		kinEng.text = "KE : " + KE;
		potEng.text = "PE : " + PE;
		totEng.text = "TE : " + TE;

		for(int i = 0; i < particles.Count; i++)
		{
			//Calculate the acceleration at each interval for each particle
			Vector3 tempacc = Vector3.zero;
			float LJPot = 0f;

			for(int j = 0; j < particles.Count; j++)		//Iterating over each pair
			{
				if(i != j)
				{
					Vector3 disp = particles[j].position - particles[i].position;		//Distance between pair
					float r = disp.magnitude;

					Vector3 LJacc = 24 * (2*Mathf.Pow(sigma/r,14) - Mathf.Pow(sigma/r,8)) * -1 * disp; //24Epsillon/sigma *((2(sig/r)^13) - (sig/r)^7)
					LJPot += 4*(Mathf.Pow(sigma/r,12) - Mathf.Pow(sigma/r,6));

					tempacc += LJacc;
				}
			}

			particles[i].potential = LJPot;
			particles[i].acceleration = tempacc;

			if(TE != 0)
			{
				TempCorrection =  1 + (Mathf.Sign(TE) * (Mathf.Sqrt(TEInit/TE) - 1));
			}
			particles[i].Step(deltat, TempCorrection);
		}

		SavePotAndEng();

		if(t >= totalt)
		{
			SaveXYZ("Final");
		}
	}

	public void SaveRadii()
	{
		System.IO.File.WriteAllLines(Application.streamingAssetsPath + "/Experiment" + ExpNum + "/Radii_" + t + ".txt", PrintRadii());
	}

	public void SavePotAndEng()
	{
		System.IO.File.AppendAllText(Application.streamingAssetsPath + "/Experiment" + ExpNum + "/Energy.txt",""+t+"      "+PE +"      "+KE + "\n");
	}

	public void SaveXYZ(string name)
	{
		System.IO.File.WriteAllLines(Application.streamingAssetsPath + "/Experiment" + ExpNum + "/" + name + ".xyz", PrintXYZ(name));
	}

	public string[] PrintXYZ(string name)
	{
		List<string> toRet = new List<string>();
		toRet.Add("" + particles.Count);
		toRet.Add(name + " : initial values-- clusters " + cluster1_sz + ":" + cluster2_sz + " -- relative velocity " + V3ToString(cluser1_v0 - cluser2_v0));

		for(int i = 0; i < particles.Count; i++)
		{
			toRet.Add("" + particles[i].position.x + "    " + particles[i].position.y + "    " + particles[i].position.z);
		}

		return toRet.ToArray(); 
	}

	public string V3ToString(Vector3 v3)
	{
		return "(" + v3.x + "," + v3.y + "," + v3.z + ")";
	}

	public string[] PrintRadii()
	{
		string[] toRet = new string[particles.Count];
		for(int i = 0; i < toRet.Length; i++)
		{
			toRet[i] = "" + (particles[i].position - com).magnitude;
			Debug.Log( "" + (particles[i].position - com).magnitude);
		}

		return toRet;
	}

}
