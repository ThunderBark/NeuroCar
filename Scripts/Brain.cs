using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class Brain : MonoBehaviour
{
	GameObject target;
	public bool isDead;
	public int SCORE;
	
	[Header("Настраиваемые параметры")]
	public Muscles muscles;
	
	[Header("Параметры мозга (ИНС)")]
	public int numOfEyes;						// Количество глаз - входных нейронов
	public float sightRange;					// Дальность зрения
	public int sightAngle;						// Угол обзора машинки
	public float thinkPeriod;					// Период активации мыслительного процесса
	public List<int> numOfLayers;				// Кол-во элементов - кол-во скрытых слоев, значение элемента - кол-во нейронов в элементе(слое)	
	[HideInInspector]
	public float[][][] weights;
	public float[][][] biases;
	
	[Header("Настройки инициализации")]
	public float maxWeight;
	public float minWeight;
	public float maxBias;
	public float minBias;
	
	[Header("Отладка")]
	public float prevTime;
	public float turnCheck;
	public float turn;
	public HitInfo[] hitInfos;
	
	// Параметры пингов
	private RaycastHit[] hit;
	private int ignoreMask = 1<<8;
	
	// Другие внутренние параметры
	private int maxNeuronCnt;
	
	//////////////////////////////////////////////////////////////////////
	// Расчет расстояний
	void PingInit()
	{
		// Выставление маски - игнорировать 8:car
		ignoreMask = ~ignoreMask;
		// Инициализация элементов
		hitInfos = new HitInfo[numOfEyes];
		hit = new RaycastHit[numOfEyes];
		for (int i = 0; i < numOfEyes; i++)
		{
			hitInfos[i] = new HitInfo();
			hit[i] = new RaycastHit();
		}
	}
	
	void Ping()
	{
		float nextPingAngle = 360 - sightAngle/2.0f;
		float pingAngleStep = sightAngle/((float)numOfEyes-1);
		Vector3 direction;
		for (int i = 0; i < numOfEyes; i++)
		{
			direction = Quaternion.Euler(0, nextPingAngle, 0)*this.transform.forward;
			if (Physics.Raycast(this.transform.position, direction, out hit[i], sightRange, ignoreMask))
			{
				hitInfos[i].hitDistance = hit[i].distance;
				Debug.DrawRay(transform.position, direction*hitInfos[i].hitDistance, Color.red);
				hitInfos[i].HIT = true;
			}
			else
			{
				hitInfos[i].hitDistance = sightRange;
				hitInfos[i].HIT = false;
			}
			nextPingAngle += pingAngleStep;	
		}
	}
	
	///////////////////////////
	// Инициализация мозгов
	public void BrainInit()
	{		
		muscles = this.GetComponent<Muscles>();
		target = GameObject.Find("Target");
		weights = new float[numOfLayers.Count][][];
		biases = new float[numOfLayers.Count][][];
		
		int numOfConnections = numOfEyes + 1;
		for (int i = 0; i < numOfLayers.Count; i++)
		{
			weights[i] = new float[numOfLayers[i]][];
			biases[i] = new float[numOfLayers[i]][];
			for(int j = 0; j < numOfLayers[i]; j++)
			{
				weights[i][j] = new float[numOfConnections];
				biases[i][j] = new float[numOfConnections];
				for (int k = 0; k < numOfConnections; k++)
				{
					weights[i][j][k] = Random.Range(minWeight, maxWeight);
					biases[i][j][k] = Random.Range(minBias, maxBias);
				}
			}
			if (numOfConnections > maxNeuronCnt){
				maxNeuronCnt = numOfConnections;
			}
			if (i < numOfLayers.Count - 1){
				numOfConnections = numOfLayers[i];
			}
		}
	}
	
	public void Think()
	{
		// Получение входного вектора значений
		Ping();
		
		float[] x = new float[maxNeuronCnt];
		float[] y = new float[maxNeuronCnt];
		// Получение значений дальностей
		for (int i = 0; i < numOfEyes; i++)
		{
			bool revert = true;
			if (!revert)
				x[i] = hitInfos[i].hitDistance/sightRange;
			else
				x[i] = 1 - hitInfos[i].hitDistance/sightRange;
		}
		// Получение значения угла поворота относительно вектора до пункта назначения
		Vector3 carToTarVect = target.GetComponent<Transform>().position - transform.position;
		x[numOfEyes] = Vector3.SignedAngle(carToTarVect, transform.forward, Vector3.up)/180f;
		turn = x[numOfEyes];

		// Расчет нейронной сети
		int numOfConnections = weights[0][0].Length;
		for (int i = 0; i < numOfLayers.Count; i++)
		{
			for(int j = 0; j < numOfLayers[i]; j++)
			{
				y[j] = ActivationFunc(x, weights[i][j], biases[i][j]);
			}
			x = y;
		}
		muscles.leftNdForce = y[0];
		muscles.rightNdForce = y[1];
	}
	
	float ActivationFunc(float[] x, float[] weight, float[] bias)
	{
		float y = 0.0f;
		for (int i = 0; i < weight.Length; i++)
		{
			y += weight[i]*x[i] + bias[i];
		}
		return ReLU_mod(y, 1f);
	}
	
	///////////////////
	// ReLU
	float ReLU(float x)
	{
		return Mathf.Max(0, x);
	}

	///////////////////
	// Тождественная с ограничением
	float ReLU_mod(float x, float magnitude)
	{
		if (x < -magnitude)
			x = -magnitude;
		if (x > magnitude)
			x = magnitude;
		return x;
	}
	///////////////////////////////////////////
	public void Upload_Brain(string path)
	{
        // string path = "C:\\Users\\User\\Desktop\\WeightsAndBiases.txt";
		StreamWriter wr = new StreamWriter(path, false);
		
		wr.WriteLine("Кол-во входов: {0}, Кол-во слоев: {1}", numOfEyes + 1, numOfLayers.Count);
		for (int i = 0; i < numOfLayers.Count; i++)
		{
			wr.WriteLine("Кол-во нейронов в слое {0}: {1} ", i+1, numOfLayers[i]);
		}
		int numOfConnections = numOfEyes + 1;
		for (int i = 0; i < numOfLayers.Count; i++)
		{
			wr.WriteLine("Слой {0}", i+1);
			for (int j = 0; j < numOfLayers[i]; j++)
			{
				wr.WriteLine("Нейрон {0}", j+1);
				for (int k = 0; k < numOfConnections; k++)
				{
					wr.Write(" {0};{1}", weights[i][j][k], biases[i][j][k]);
				}
				wr.WriteLine("");
			}
			if (i < numOfLayers.Count - 1){
				numOfConnections = numOfLayers[i];
			}
		}
		wr.Close();
	}
	
	public void ResetStats()
	{
		isDead = false;
		SCORE = 0;
	}
	
	///////////////////////////////////////////////////////////////////////////
    void Start()
    {
		// Инициализация глаз и мозгов
		PingInit();
		//BrainInit();
    }
	
    void FixedUpdate()
    {		
		// Думаем с определенным периодом, если не мертвы
		if (Time.time - prevTime > thinkPeriod && !isDead){
			prevTime = Time.time;
			Think();
		}
    }
	
	public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 0)
		{
			muscles.leftNdForce = 0.0f;
			muscles.rightNdForce = 0.0f;
			isDead = true;
		}
    }
}

[System.Serializable]
public class HitInfo {
    public float hitDistance;
    public bool HIT;
}