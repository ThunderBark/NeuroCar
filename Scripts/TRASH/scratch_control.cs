using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scratch_control : MonoBehaviour
{
	float motor;
	float steering;
    public float speed;
	public GameObject Right_Wheel, Left_Wheel;
	private Rigidbody r_rb, l_rb;
	
    // Start is called before the first frame update
    void Start()
    {
        r_rb = Right_Wheel.GetComponent<Rigidbody>();
		l_rb = Left_Wheel.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        motor = Input.GetAxis("Vertical");
        steering = Input.GetAxis("Horizontal");
		
		
		if (motor < 0) 
		{
			if (steering != 0) 
			{
				r_rb.AddRelativeTorque(Vector3.up * speed * steering);
				l_rb.AddRelativeTorque(Vector3.down * speed * steering);
			}
			else
			{
				r_rb.AddRelativeTorque(Vector3.up * speed);
				l_rb.AddRelativeTorque(Vector3.up * speed);
			}
		}
		else
		{
			if (steering != 0) 
			{
				r_rb.AddRelativeTorque(Vector3.up * speed * steering);
				l_rb.AddRelativeTorque(Vector3.down * speed * steering);
			}
			else
			{
				if (motor > 0)
				{
					r_rb.AddRelativeTorque(Vector3.down * speed);
					l_rb.AddRelativeTorque(Vector3.down * speed);
				}
			}
		}
    }
}
