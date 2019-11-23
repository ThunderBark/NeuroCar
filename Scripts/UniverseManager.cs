using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniverseManager : MonoBehaviour
{
	
	[Header("Общая информация")]
	public int generationNumber;
	public int bestPrevGenScore;
	public int avgPrevGenScore;
	public int gain;
	
	[Header("Основные параметры")]
	public float maxDurationOfGen;
	public int scaleOfGeneration;
	public float speedOfTime;
	public float escapeCheckPeriod;
	
	public Object sampleCar;
	
	[Header("Настройки естественного отбора")]
	public int winGenesSpreadRatio;
	public int maxSurvivorCnt;
	public float maxSurviveProb;
	public float minSurviveProb;
	public float mutationChance;
	public float mutationRate;
	public bool generateObsNextGen;
	
	[Header("Настройки расчета приспособленности")]
	public float distanceImportance;
	public float rotationImportance;
	public float dumbBrainCheckTime;
	public int minScore;	
	
	[Header("Камера")]
	public GameObject bigBrother;
	public float bigBrotherSpeed;
	public bool freeCam;
	private Vector3 dragOrigin;
	
	[Header("Отладка")]
	public float startOfLastGeneration;
	public int[] scoreList;
	public float scoreCheckPeriod;
	
	
	// Внутренние настройки
	private List<GameObject> car;
	private Brain[] brain;
	
	// Основные и вспомогательные переменные генератора препятствий
	private List<Vector3> sectorPosMap;
	private ObstacleGenerator obsGenerator;
	
	private float lastEscapeCheck;
	
	// Основные и вспомогательные информационные переменные
	private float[] travelDists;
	private float[] fullRotats;
	private float[] radVectM;
	private float[] maxRadVectM;
	
	private Vector3[] prevPos;
	private Vector3[] prevRot;
	
	private float lastCheck;
	
	/****************************************************************/
	//////////////////////////////////////////////////////////////////
	/****************************************************************/
	
	void NaturalSelection()
	{
		// Определение выживших по определенному распределению
		int parentCount = 0;
		
		SortCarsByScore();
		
		gain = avgPrevGenScore;
		avgPrevGenScore = 0;
		for (int i = 0; i < scaleOfGeneration; i++)
		{	
			scoreList[i] = car[i].transform.GetChild(3).GetComponent<Brain>().SCORE;
			avgPrevGenScore += scoreList[i];
			
			if (isSurvived(i, scaleOfGeneration, minSurviveProb, maxSurviveProb))
				if (parentCount+1 > maxSurvivorCnt)
					brain[i].SCORE = 0;
				else 
					parentCount += 1;
			else
				brain[i].SCORE = 0;
		}
		avgPrevGenScore /= (int)scoreList.Length;
		gain = avgPrevGenScore - gain;
		bestPrevGenScore = scoreList[0];
		
		SortCarsByScore();
		
		// Кроссоверинг и мутация
		int numOfConnections;
		int parent;
		for (int i = parentCount; i < scaleOfGeneration; i++)
		{
			brain[i] = car[i].transform.GetChild(3).GetComponent<Brain>();
			numOfConnections = brain[i].numOfEyes;
			for (int k = 0; k < brain[i].numOfLayers.Count; k++)
			{
				for (int n = 0; n < brain[i].numOfLayers[k]; n++)
				{
					for (int m = 0; m < numOfConnections; m++)
					{
						parent = DonorNumber(parentCount, winGenesSpreadRatio);
						brain[i].weights[k][n][m] = brain[parent].weights[k][n][m];
						brain[i].biases[k][n][m] = brain[parent].biases[k][n][m];
						if (Random.Range(0.0f, 100.0f) <= mutationChance)
						{
							brain[i].weights[k][n][m] = Mutate(brain[i].weights[k][n][m]);
							brain[i].biases[k][n][m] = Mutate(brain[i].biases[k][n][m]);
						}
					}
				}
				numOfConnections = brain[i].numOfLayers[k];
			}
		}
	}
	
	void StartNextGeneration()
	{
		// Сортировка бесполезных мозгов
		CheckUselessBrains();
		
		// Процедура естественного отбора и размножения
		NaturalSelection();
		
		// Респавн препятствий
		if (generateObsNextGen)
		{
			sectorPosMap.Clear();
			obsGenerator.Start();
			sectorPosMap.Add(Vector3.zero);
			generateObsNextGen = false;
		}

		// Сброс всех машин
		ResetCars();		
		
		startOfLastGeneration = Time.time;
		generationNumber++;
	}
	
	void CheckSimulationEscape()
	{
		int[] carPosition = new int[2];
		Vector3 spawnPosition;
		GameObject[] hull = new GameObject[scaleOfGeneration];
		
		for(int i = 0; i < scaleOfGeneration; i++)
		{
			hull[i] = car[i].transform.GetChild(3).gameObject;
			
			if ((int)(car[i].transform.position.x)%(2*obsGenerator.range) < obsGenerator.range)
				carPosition[0] = (int)(hull[i].transform.position.x - (int)(hull[i].transform.position.x)%(2*obsGenerator.range));
			else
				carPosition[0] = (int)(hull[i].transform.position.x + (int)(hull[i].transform.position.x)%(2*obsGenerator.range));
			if ((int)(car[i].transform.position.z)%(2*obsGenerator.range) < obsGenerator.range)
				carPosition[1] = (int)(hull[i].transform.position.z - (int)(hull[i].transform.position.z)%(2*obsGenerator.range));
			else
				carPosition[1] = (int)(hull[i].transform.position.z + (int)(hull[i].transform.position.z)%(2*obsGenerator.range));
			
			if (!isOcupated(carPosition, 1, 0))
			{
				spawnPosition = new Vector3(carPosition[0]+2*obsGenerator.range, 0.0f, carPosition[1]);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, 1, 1))
			{
				spawnPosition = new Vector3(carPosition[0]+2*obsGenerator.range, 0.0f, carPosition[1]+2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, 0, 1))
			{
				spawnPosition = new Vector3(carPosition[0], 0.0f, carPosition[1]+2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, -1, 1))
			{
				spawnPosition = new Vector3(carPosition[0]-2*obsGenerator.range, 0.0f, carPosition[1]+2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, -1, 0))
			{
				spawnPosition = new Vector3(carPosition[0]-2*obsGenerator.range, 0.0f, carPosition[1]);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, -1, -1))
			{
				spawnPosition = new Vector3(carPosition[0]-2*obsGenerator.range, 0.0f, carPosition[1]-2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, 0, -1))
			{
				spawnPosition = new Vector3(carPosition[0], 0.0f, carPosition[1]-2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
			if (!isOcupated(carPosition, 1, -1))
			{
				spawnPosition = new Vector3(carPosition[0]+2*obsGenerator.range, 0.0f, carPosition[1]-2*obsGenerator.range);
				obsGenerator.SpawnObstacles(spawnPosition);
				sectorPosMap.Add(spawnPosition);
			}
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
		// Инициализация
		obsGenerator = this.GetComponent<ObstacleGenerator>();
		scoreList = new int[scaleOfGeneration];
		
		// Начальный спаун препятствий
		sectorPosMap = new List<Vector3>();
		//obsGenerator.Start();
		sectorPosMap.Add(Vector3.zero);
		
		// Спаун машин и инициализация вспомогательных переменных
		car = new List<GameObject>();
		brain = new Brain[scaleOfGeneration];
		prevPos = new Vector3[scaleOfGeneration];
		prevRot = new Vector3[scaleOfGeneration];
		radVectM = new float[scaleOfGeneration];
		maxRadVectM = new float[scaleOfGeneration];
		travelDists = new float[scaleOfGeneration];
		fullRotats = new float[scaleOfGeneration];
		for (int i = 0; i < scaleOfGeneration; i++)
		{
			travelDists[i] = 0;
			fullRotats[i] = 0;
			radVectM[i] = 0;
			maxRadVectM[i] = 0;
			prevPos[i] = Vector3.zero;
			prevRot[i] = Vector3.zero;
			car.Add((GameObject)Instantiate(sampleCar, Vector3.up, Quaternion.identity));
			brain[i] = car[i].transform.GetChild(3).GetComponent<Brain>();
		}
    }


    void FixedUpdate()
    {
        if (Time.time - startOfLastGeneration > maxDurationOfGen || isAllDead(brain))
			StartNextGeneration();
		
		if (Time.time - lastEscapeCheck > escapeCheckPeriod)
		{
			lastEscapeCheck = Time.time;
			CheckSimulationEscape();
		}
		
		BigBrotherManager();
		CalculateScore();
		
		Time.timeScale = speedOfTime;
    }
	
	///////////////////////////////////////////////////////////////////////////////////////////////////
	float Mutate(float gene)
	{
		gene = Random.Range(-mutationRate, mutationRate);
		return gene;
	}
	
	bool isSurvived(int i, int count, float min, float max)
	{
		float rand = Random.Range(0.0f, 100.0f);
		if ((i*(min-max)/count + max) > rand)
			return true;
		return false;
	}
	
	int DonorNumber(int count, float prob)
	{
		float rand = Random.Range(0.0f, 1.0f);
		prob = 1 - (prob/100);
		for (int i = 0; i < count; i++)
		{
			if (rand > prob)
				prob += (1-prob)/prob;
			else
				return i;
		}
		return 0;
	}
	
	bool isOcupated(int[] pos, int divX, int divZ)
	{
		for (int j = 0; j < sectorPosMap.Count; j++)
		{
			if ((int)(pos[0]+(divX*2*obsGenerator.range)) == (int)sectorPosMap[j].x && (int)(pos[1]+(divZ*2*obsGenerator.range)) == (int)sectorPosMap[j].z)
			{
				return true;
			}					
		}
		return false;
	}
	
	bool isAllDead(Brain[] brain)
	{
		int cnt = 0;
		for (int i = 0; i < brain.Length; i++)
			if (brain[i].isDead)
				cnt += 1;
		
		if (cnt == brain.Length)
			return true;
		return false;
	}
	
	void SortCarsByScore()
	{
		car = car.OrderByDescending(GameObject => GameObject.transform.GetChild(3).GetComponent<Brain>().SCORE).ToList();
		for (int i = 0; i < scaleOfGeneration; i++)
		{
			brain[i] = car[i].transform.GetChild(3).GetComponent<Brain>();
		}
	}
	
	void CalculateScore()
	{
		if (Time.time - lastCheck > scoreCheckPeriod)
		{
			Transform tHull;
			for (int i = 0; i < scaleOfGeneration; i++)
			{
				tHull = car[i].transform.GetChild(3);
				travelDists[i] += (tHull.position - prevPos[i]).magnitude;
				prevPos[i] = tHull.position;
				fullRotats[i] += Mathf.Abs(tHull.eulerAngles.y - prevRot[i].y);
				prevRot[i] = tHull.eulerAngles;
				radVectM[i] = tHull.position.magnitude;
				if (radVectM[i] > maxRadVectM[i])
					maxRadVectM[i] = radVectM[i];
				
				int score = (int)(maxRadVectM[i]*distanceImportance) + (int)(fullRotats[i]*rotationImportance);
				if (Time.time - startOfLastGeneration > dumbBrainCheckTime ? score > minScore : true)
					tHull.gameObject.GetComponent<Brain>().SCORE = score;
				else
					tHull.gameObject.GetComponent<Brain>().isDead = true;
			}
			lastCheck = Time.time;
		}
	}
	
	void CheckUselessBrains()
	{
		
	}
	
	void ResetCars()
	{
		for (int i = 0; i < scaleOfGeneration; i++)
		{
			car[i].GetComponent<CarConstructor>().ResetTransform();
			car[i].transform.GetChild(3).GetComponent<Muscles>().rightNdForce = 0.0f;
			car[i].transform.GetChild(3).GetComponent<Muscles>().leftNdForce = 0.0f;
			prevPos[i] = Vector3.zero;
			prevRot[i] = Vector3.zero;
			brain[i].ResetStats();
			travelDists[i] = 0;
			fullRotats[i] = 0;
			radVectM[i] = 0;
			maxRadVectM[i] = 0;
		}
	}
	
	void BigBrotherManager()
	{
		if (!freeCam)
		{
			Vector3 util = Vector3.zero;
			Vector3 pos = new Vector3(car[0].transform.GetChild(3).gameObject.transform.position.x, 0.0f, car[0].transform.GetChild(3).gameObject.transform.position.z);
			for (int i = 1; i < car.Count; i++)
				if (car[i].transform.GetChild(3).gameObject.transform.position.magnitude > pos.magnitude)
				{
					pos = car[i].transform.GetChild(3).gameObject.transform.position;
				}
			bigBrother.transform.position = Vector3.SmoothDamp(bigBrother.transform.position,
															new Vector3(pos.x, bigBrother.transform.position.y, pos.z),
															ref util,
															bigBrotherSpeed);
		}
	}
}