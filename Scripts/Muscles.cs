using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muscles : MonoBehaviour
{
	public float confidenceCoef;
    public float speed;
	public GameObject rightWheel, leftWheel;
	
	//[HideInInspector]
	public float leftNdForce;
	public float rightNdForce;
	
	private Rigidbody r_rb, l_rb;
	
	
    void Start()
    {
        r_rb = rightWheel.GetComponent<Rigidbody>();
		l_rb = leftWheel.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
		if (Mathf.Abs(leftNdForce)>confidenceCoef)
		{
			l_rb.AddRelativeTorque(Vector3.down * speed * leftNdForce);
		}
		if (Mathf.Abs(rightNdForce)>confidenceCoef)
		{
			r_rb.AddRelativeTorque(Vector3.down * speed * rightNdForce);
		}
    }
}

