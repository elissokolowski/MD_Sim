using UnityEngine;
using System.Collections;

public class Cluster {

	public Particle[] particles;
	private Vector3 _position;
	private Vector3 _velocity;

	public Vector3 position{
		get{return _position;}
		set{
			for(int i = 0; i < particles.Length; i++)
			{
				particles[i].position += (value - _position);
			}
			_position = value;
		}
	}

	public Cluster(int parts, Vector3 pos, Vector3 vel, GameObject[] prefabs)
	{
		_position = pos;
		_velocity = vel;

		particles = new Particle[parts];

		//Load positions in from file
		TextAsset file = Resources.Load("Clusters/"+parts) as TextAsset;

		//Break the text up into individual lines
		string[] lines = file.text.Split(new char[]{'\r','\n'},System.StringSplitOptions.RemoveEmptyEntries);

		if(lines.Length < parts)
		{
			//Error out if the file is too short
			Debug.LogError("Files does not contain enough positions to initialise");
			return;
		}

		//Create a particle for each line and assign values
		for(int i = 0; i < particles.Length; i++)
		{
			particles[i] = new Particle();
			particles[i].display = prefabs[i];		//Give each particle a reference to the object used to represent it in the visualiser
			particles[i].position = PosFromText(lines[i]) + position; //add the clusters position to the relative position to find world space position of each particle
			particles[i].velocity = _velocity;		//Initial velocity of each particle is equal to the cluster velocity
		}
	}

	//Convert a line from the initial text file into a coordinate set.
	public Vector3 PosFromText(string txt)
	{
		Vector3 toRet = Vector3.zero;
		string[] coordinates = txt.Split(new char[]{' '},System.StringSplitOptions.RemoveEmptyEntries); //Use empty spaces as deliminators and remove 0 length strings

		if(coordinates.Length != 3)
		{
			Debug.LogError("Coordinate array is wrong length for v3");
			return Vector3.zero;
		}

		//Convert strings to floats
		toRet.x = System.Convert.ToSingle(coordinates[0]);
		toRet.y = System.Convert.ToSingle(coordinates[1]);
		toRet.z = System.Convert.ToSingle(coordinates[2]);

		return toRet;
	}
}
