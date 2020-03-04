using System.Collections;
using System.Collections.Generic;
using static System.DateTime;
using UnityEngine;
using UnityEngine.Events;

public class ObstacleGenerator : MonoBehaviour
{
	[Header("Размер и плотность элементарной площадки")]
	public int range;
	public float density;
	public int deviationPercent;
	public GameObject obstacleParent;
	
	[Header("Размеры препятствий")]
	public float minDimension;
	public float maxDimension;
	
	[Header("Тип препятствий")]
	public int obstacleType;
	
	// Внутренние параметры
	private int NumbOfObst;
	
	void Awake()
	{
		Random.InitState(System.DateTime.Now.Millisecond);
	}
	
	public void SpawnObstacles(Vector3 center)
	{
		NumbOfObst = Random.Range((int)(range*range*density*(1-deviationPercent/100)), (int)(range*range*density*(1+deviationPercent/100)));
		
		GameObject primitive;
		for (int i = 0; i < NumbOfObst; i++)
		{
			Vector3 pos;
			Vector3 scale;
			if (Random.Range(0,2) == 0)
			{
				primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
				scale = new Vector3(Random.Range(minDimension, maxDimension), 3f, Random.Range(minDimension, maxDimension));
				pos = new Vector3(Random.Range(center.x-range, center.x+range), 1.5f, Random.Range(center.z-range, center.z+range));
			}
			else
			{
				primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				float diam = Random.Range(minDimension, maxDimension);
				scale = new Vector3(diam, 3f, diam);
				pos = new Vector3(Random.Range(center.x-range, center.x+range), 1.5f, Random.Range(center.z-range, center.z+range));
			}
			primitive.transform.localScale = scale;
			primitive.transform.position = pos;
			primitive.transform.parent = obstacleParent.transform;
		}
	}
	
	///////////////////////////////////////////////////////////////////////
    public void Start()
    {
		for (int i = 0; i < obstacleParent.transform.childCount; i++)
		{
			Destroy(obstacleParent.transform.GetChild(i).gameObject);
		}
		
		NumbOfObst = Random.Range((int)(range*range*density*(1-deviationPercent/100)), (int)(range*range*density*(1+deviationPercent/100)));
		
		GameObject primitive;
		for (int i = 0; i < NumbOfObst; i++)
		{
			Vector3 pos;
			Vector3 scale;
			if (Random.Range(0,2) == 0)
			{
				primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
				scale = new Vector3(Random.Range(minDimension, maxDimension), 3f, Random.Range(minDimension, maxDimension));
				pos = new Vector3(Random.Range(-range, range), 1.5f, Random.Range(-range, range));
			}
			else
			{
				primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				float diam = Random.Range(minDimension, maxDimension);
				scale = new Vector3(diam, 3f, diam);
				pos = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
			}
			if (pos.magnitude < Mathf.Sqrt(scale.x*scale.x+scale.z*scale.z)+2.84f)
				pos = pos.normalized * (Mathf.Sqrt(scale.x*scale.x+scale.z*scale.z)+2);
			primitive.transform.localScale = scale;
			primitive.transform.position = pos;
			primitive.transform.parent = obstacleParent.transform;
		}
    }
}