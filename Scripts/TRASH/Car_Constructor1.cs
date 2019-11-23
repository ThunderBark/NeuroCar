using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Constructor1 : MonoBehaviour
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
	private Vector3 R_pos, L_pos, Hull_pos, Sphere_pos;
	
    // Start is called before the first frame update
    void Start()
    {
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
		
		///////////////////////////////
		// Изменение размеров элементов
		tR.localScale = new Vector3(wheel_diam*RtoU_scale, wheel_width*RtoU_scale/2.0f, wheel_diam*RtoU_scale);
		tL.localScale = new Vector3(wheel_diam*RtoU_scale, wheel_width*RtoU_scale/2.0f, wheel_diam*RtoU_scale);
		tHull.localScale = new Vector3(Hull_width*RtoU_scale, Hull_thick*RtoU_scale, Hull_length*RtoU_scale);
		tSphere.localScale = new Vector3((wheel_diam-Hull_thick)*RtoU_scale/2,(wheel_diam-Hull_thick)*RtoU_scale/2,(wheel_diam-Hull_thick)*RtoU_scale/2);
		
		
		// Выставление координат частей
		R_pos = new Vector3((Hull_width+wheel_width)*RtoU_scale/2, 0f, (-1)*wheel_offset_Z*RtoU_scale);
		L_pos = new Vector3((wheel_width+Hull_width)*RtoU_scale/(-2), 0f, (-1)*wheel_offset_Z*RtoU_scale);
		Hull_pos = new Vector3(0f, 0f, (-1)*Hull_length*RtoU_scale/2);
		Sphere_pos = new Vector3(0f, (wheel_diam-Hull_thick)*RtoU_scale/(-2), (-1)*Sphere_offset_Z*RtoU_scale);
		tR.localPosition = R_pos;
		tL.localPosition = L_pos;
		tHull.localPosition = Hull_pos;
		tSphere.localPosition = Sphere_pos;
		
		// Выставление координат якорей соединений
		wheelJoints[0].autoConfigureConnectedAnchor = false;
		wheelJoints[0].anchor = new Vector3(Hull_width*RtoU_scale/2, 0f, (-1)*wheel_offset_Z*RtoU_scale);
		wheelJoints[0].connectedAnchor = new Vector3(Hull_width*RtoU_scale/2, 0f, (-1)*wheel_offset_Z*RtoU_scale);
		wheelJoints[1].autoConfigureConnectedAnchor = false;
		wheelJoints[1].anchor = new Vector3(Hull_width*RtoU_scale/(-2), 0f, (-1)*wheel_offset_Z*RtoU_scale);
		wheelJoints[1].connectedAnchor = new Vector3(Hull_width*RtoU_scale/(-2), 0f, (-1)*wheel_offset_Z*RtoU_scale);
		wheelJoints[2].autoConfigureConnectedAnchor = false;
		wheelJoints[2].anchor = Sphere_pos;
		wheelJoints[2].connectedAnchor = Sphere_pos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
