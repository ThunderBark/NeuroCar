using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarConstructor : MonoBehaviour
{
	const float RtoU_scale = 0.01f;
	
	public int wheel_diam;
	public int wheel_width;
	public int Hull_length;
	public int Hull_width;
	public int Hull_thick;
	
	public int wheel_offset_Z;
	public int Sphere_offset_Z;
	
	private GameObject R, L, Sphere, Hull;
	private Transform tR, tL, tSphere, tHull;
	private List<ConfigurableJoint> wheelJoints;
	
	
	public void ResetTransform()
	{
		//Awake();		
		Rigidbody rHull = Hull.GetComponent<Rigidbody>();
		Rigidbody rR = R.GetComponent<Rigidbody>();
		Rigidbody rL = L.GetComponent<Rigidbody>();
		Rigidbody rSphere = Sphere.GetComponent<Rigidbody>();
		
		rHull.velocity = Vector3.zero;
		rR.velocity = Vector3.zero;
		rL.velocity = Vector3.zero;
		rSphere.velocity = Vector3.zero;
		
		rHull.isKinematic = true;
		rR.isKinematic = true;
		rL.isKinematic = true;
		rSphere.isKinematic = true;
		
		transform.position = new Vector3(0f, 0.5f*wheel_diam*RtoU_scale, 0f);
		transform.rotation = Quaternion.Euler(0, 0, 0);
		
		rR.position = new Vector3((Hull_width+wheel_width)*RtoU_scale/2, 0.6f*wheel_diam*RtoU_scale, ((-1)*wheel_offset_Z)*RtoU_scale);
		rL.position = new Vector3((wheel_width+Hull_width)*RtoU_scale/(-2), 0.6f*wheel_diam*RtoU_scale, ((-1)*wheel_offset_Z)*RtoU_scale);
		rSphere.position = new Vector3(0f, 1.2f*(wheel_diam-Hull_thick)*RtoU_scale/(-2), (-1)*Sphere_offset_Z*RtoU_scale);
		rHull.position = new Vector3(0f, 0.6f*wheel_diam*RtoU_scale, (-1)*(Hull_length/2.0f)*RtoU_scale);
		
		int rand = Random.Range(-180, 180);
		bool isRandomOrientation = GameObject.Find("Plane").GetComponent<UniverseManager>().enRandomCarOrientation;
		if (!isRandomOrientation)
			rand = 0;

		rR.rotation = Quaternion.Euler(0, rand, 90);
		rL.rotation = Quaternion.Euler(0, rand, 90);
		rSphere.rotation = Quaternion.Euler(0, rand, 0);
		rHull.rotation = Quaternion.Euler(0, rand, 0);

		rHull.velocity = Vector3.zero;
		rR.velocity = Vector3.zero;
		rL.velocity = Vector3.zero;
		rSphere.velocity = Vector3.zero;
		
		rHull.isKinematic = false;
		rR.isKinematic = false;
		rL.isKinematic = false;
		rSphere.isKinematic = false;
		
	}
	
    void Start()
    {
		transform.position = new Vector3(0f, 0.5f*wheel_diam*RtoU_scale, 0f);
		
		// Получение обьектов: колес, корпуса и флюгеля
        R = transform.Find("R").gameObject;
        L = transform.Find("L").gameObject;
        Sphere = transform.Find("Sphere").gameObject;
        Hull = transform.Find("Hull").gameObject;
		// Получение листа соединений
		wheelJoints = new List<ConfigurableJoint>();
		Hull.GetComponents(wheelJoints);
		// Получение значений положения и размеров
        tR = R.GetComponent<Transform>();
        tL = L.GetComponent<Transform>();
        tSphere = Sphere.GetComponent<Transform>();
        tHull = Hull.GetComponent<Transform>();
		tHull.eulerAngles = Vector3.zero;
		
		///////////////////////////////
		//
		Physics.IgnoreLayerCollision(8, 8, true);
		// Изменение размеров элементов
		tR.localScale = new Vector3(wheel_diam*RtoU_scale, wheel_width*RtoU_scale/2.0f, wheel_diam*RtoU_scale);
		tL.localScale = new Vector3(wheel_diam*RtoU_scale, wheel_width*RtoU_scale/2.0f, wheel_diam*RtoU_scale);
		tHull.localScale = new Vector3(Hull_width*RtoU_scale, Hull_thick*RtoU_scale, Hull_length*RtoU_scale);
		tSphere.localScale = new Vector3((wheel_diam-Hull_thick)*RtoU_scale/2,(wheel_diam-Hull_thick)*RtoU_scale/2,(wheel_diam-Hull_thick)*RtoU_scale/2);
		
		
		// Выставление координат частей
		tR.localPosition = new Vector3((Hull_width+wheel_width)*RtoU_scale/2, 0f, ((-1)*wheel_offset_Z)*RtoU_scale);
		tL.localPosition = new Vector3((wheel_width+Hull_width)*RtoU_scale/(-2), 0f, ((-1)*wheel_offset_Z)*RtoU_scale);
		tSphere.localPosition = new Vector3(0f, (wheel_diam-Hull_thick)*RtoU_scale/(-2), (-1)*Sphere_offset_Z*RtoU_scale);
		tHull.localPosition = new Vector3(0f, 0f, (-1)*(Hull_length/2.0f)*RtoU_scale);
		
		// Выставление координат якорей соединений
		wheelJoints[0].anchor = new Vector3(((Hull_width+wheel_width)/2.0f)/Hull_width, 0f, ((Hull_length-wheel_offset_Z*2)/2.0f)/Hull_length);
		wheelJoints[0].autoConfigureConnectedAnchor = false;
		wheelJoints[0].connectedAnchor = Vector3.zero;
		wheelJoints[1].anchor = new Vector3(((Hull_width+wheel_width)/(-2.0f))/Hull_width, 0f, ((Hull_length-wheel_offset_Z*2)/2.0f)/Hull_length);
		wheelJoints[1].autoConfigureConnectedAnchor = false;
		wheelJoints[1].connectedAnchor = Vector3.zero;
		wheelJoints[2].anchor = new Vector3(0f, (((float)wheel_diam-Hull_thick)*2)/((-3.0f)*Hull_thick), ((Hull_length/2.0f)-Sphere_offset_Z)/Hull_length);
		wheelJoints[2].autoConfigureConnectedAnchor = false;
		wheelJoints[2].connectedAnchor = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		
    }
}
