using System.Collections;
using System.Collections.Generic;
using static System.DateTime;
using UnityEngine;
using UnityEngine.Events;

public class ObstacleGenerator : MonoBehaviour
{
	[Header("Тип препятствий")]
	public int obstacleType;
	GameObject obstacleParent;
	
	[Header("0 - Случайное распределение")]
	[Header("Плотность распределения")]
	public float density;
	public int deviationPercent;

	[Header("1 - Сетка")]

	public float stepNum;
	
	[Header("Размеры препятствий")]
	public int range;
	public float minDimension;
	public float maxDimension;
	
	
	// Внутренние параметры
	private int NumbOfObst;
	
	void Awake()
	{
		Random.InitState(System.DateTime.Now.Millisecond);
	}
	
	public void SpawnObstacles(Vector3 center)
	{
		if (obstacleType == 0)
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
		else if (obstacleType == 1)
		{
			float stepWidth = 2*((float)range)/stepNum;
			GameObject primitive;
			for (int i = 0; i < stepNum; i++)
			{
				for (int j = 0; j < stepNum; j++)
				{
					Vector3 pos;
					Vector3 scale;
					if (Random.Range(0,2) == 0)
					{
						primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
						scale = new Vector3(Random.Range(minDimension, maxDimension), 3f, Random.Range(minDimension, maxDimension));
						pos = new Vector3(center.x-range+(stepWidth/2)+(i*stepWidth), 1.5f, center.z-range+(stepWidth/2)+(j*stepWidth));
					}
					else
					{
						primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
						float diam = Random.Range(minDimension, maxDimension);
						scale = new Vector3(diam, 3f, diam);
						pos = new Vector3(center.x-range+(stepWidth/2)+(i*stepWidth), 1.5f, center.z-range+(stepWidth/2)+(j*stepWidth));
					}
					primitive.transform.localScale = scale;
					primitive.transform.position = pos;
					primitive.transform.parent = obstacleParent.transform;
				}
			}
		}
	}
	
	///////////////////////////////////////////////////////////////////////
    public void Start()
    {
		obstacleParent = GameObject.Find("Obstacles");

		for (int i = 0; i < obstacleParent.transform.childCount; i++)
		{
			Destroy(obstacleParent.transform.GetChild(i).gameObject);
		}
		
		SpawnObstacles(Vector3.zero);

		for (int i = 0; i < obstacleParent.transform.childCount; i++)
			if (obstacleParent.transform.GetChild(i).gameObject.GetComponent<Transform>().position.magnitude < 5)
				Destroy(obstacleParent.transform.GetChild(i).gameObject);
		/*NumbOfObst = Random.Range((int)(range*range*density*(1-deviationPercent/100)), (int)(range*range*density*(1+deviationPercent/100)));
		
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
		}*/
    }
}