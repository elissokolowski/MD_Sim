using UnityEngine;
using System.Collections;

public class Particle {
	public Vector3 velocity = Vector3.zero;
	public Vector3 acceleration = Vector3.zero; 
	public float potential = 0f;

	public GameObject display;

	private Vector3 _position;

	public Vector3 position {
		get {return _position;}
		set {
			_position = value;
			if(display != null)
			{
				display.transform.position = value;
			}
		}
	}

	public float kEnergy {
		get {
			return 0.5f * Vector3.Dot(velocity,velocity);
		}
	}

	public Particle()
	{
		//Empty constructor
	}

	public void Step(float deltat, float Tfact)
	{
		position += (velocity * deltat) + 0.5f * Mathf.Pow(deltat,2) * acceleration;
		velocity = (velocity * Tfact) + (0.5f * deltat * acceleration);
	}
}
