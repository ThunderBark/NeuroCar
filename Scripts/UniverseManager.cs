using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static System.Exception;
using UnityEngine;
using UnityEngine.UI;

public class UniverseManager : MonoBehaviour
{
	
	[Header("Общая информация")]
	public int generationNumber;
	public int bestPrevGenScore;
	public int avgPrevGenScore;
	public int gain;

	// Блок переменных для записи статистики
	InformationManager infoManager;
	string statPath;
	
	[Header("Основные параметры")]
	public float maxDurationOfGen;
	public int scaleOfGeneration;
	public float speedOfTime;
	public float escapeCheckPeriod;

	public bool targetInCentre;
	public int maxTargetRange;
	public int minTargetRange;
	
	public Object sampleCar;
	
	[Header("Настройки естественного отбора")]
	public int winGenesSpreadRatio;
	public int maxSurvivorCnt;
	public float maxSurviveProb;
	public float minSurviveProb;
	public float mutationChance;
	public float mutationRate;
	public bool generateObsNextGen;
	public bool	enableGenerateObs;

	int MutationType;
	int CrossingoverType;	// Выбор типа кроссинговера: 0 - Деление отобранных особей
													  // 1 - Два родителя
													  // 2 - Множество родителей
	
	[Header("Настройки расчета приспособленности")]
	public float distanceImportance;
	public float rotationImportance;
	public float dumbBrainCheckTime;
	public int minScore;	
	public bool enRandomCarOrientation;
	public bool changeTargetPos;
	
	[Header("Камера")]
	public GameObject bigBrother;
	public float bigBrotherSpeed;

	public float targetScale;
	public bool freeCam;
	private Vector3 dragOrigin;
	
	[Header("Отладка")]
	public float startOfLastGeneration;
	public int[] scoreList;
	public float scoreCheckPeriod;
	public int t,t1;
	public string str;
	public string tstr;
	public int deathGen;
	
	
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
	private int[] maxScore;
	
	private Vector3[] prevPos;
	private Vector3[] prevRot;
	
	private float lastCheck;
	
	/****************************************************************/
	//////////////////////////////////////////////////////////////////
	/****************************************************************/
	
	void NaturalSelection()
	{
		int parentCount = 0;
		
		car = SortCarsByScore(car);
		
		gain = avgPrevGenScore;
		avgPrevGenScore = 0;
		for (int i = 0; i < scaleOfGeneration; i++)
		{	
			scoreList[i] = brain[i].SCORE;
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
		
		// Запись статистики
		infoManager.StatWrite(statPath, generationNumber, bestPrevGenScore,
							avgPrevGenScore, gain);

		car = SortCarsByScore(car);
		
		// Кроссоверинг и мутация
		// Выбор типа кроссинговера: 0 - Деление отобранных особей
								  // 1 - Два родителя
								  // 2 - Множество родителей
		int numOfConnections;	
		int parent = 0;
		
		// Тестовая версия выбора двух родителей
		int parent1 = Random.Range(0, parentCount);
		int parent2 = Random.Range(0, parentCount);
		while (parent2 == parent1)
			parent2 = Random.Range(0, parentCount);

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
						// Кроссинговер --------------------
						if (CrossingoverType == 0)
						{
							brain[i].weights[k][n][m] = brain[parent].weights[k][n][m];
							brain[i].biases[k][n][m] = brain[parent].biases[k][n][m];
						}
						else if (CrossingoverType == 1)
						{
							int pick = Random.Range(0,2);
							brain[i].weights[k][n][m] = brain[pick>0 ? parent2:parent1].weights[k][n][m];
							brain[i].biases[k][n][m] = brain[pick>0 ? parent2:parent1].biases[k][n][m];
						}
						else if (CrossingoverType == 2)
						{
							parent = DonorNumber(parentCount, winGenesSpreadRatio);
							brain[i].weights[k][n][m] = brain[parent].weights[k][n][m];
							brain[i].biases[k][n][m] = brain[parent].biases[k][n][m];
						}
						// ---------------------------------
						// Мутация -------------------------
						if (Random.Range(0.0f, 100.0f) < mutationChance)
						{
							brain[i].weights[k][n][m] = Mutate(brain[i].weights[k][n][m]);
							brain[i].biases[k][n][m] = Mutate(brain[i].biases[k][n][m]);
						}
						//----------------------------------
					}
				}
				numOfConnections = brain[i].numOfLayers[k];
			}
			if (CrossingoverType == 0)
				if (parent < scaleOfGeneration)
					parent++;
				else
					parent = 0;
		}
	}
	
	void StartNextGeneration()
	{
		// Сортировка бесполезных мозгов
		CheckUselessBrains();
		
		// Процедура естественного отбора и размножения
		NaturalSelection();

		// Процедура выключения симуляции по времени или по поколению
		SelfDestruct();
		
		// Респавн препятствий
		if (generateObsNextGen)
		{
			sectorPosMap.Clear();
			//obsGenerator.Start();
			obsGenerator.SpawnObstacles(Vector3.zero);
			sectorPosMap.Add(Vector3.zero);
			generateObsNextGen = false;
			enableGenerateObs = true;
		}

		// Сброс всех машин
		ResetCars();	
		
		// Сброс положения конечной точки
		//ResetTarget(true);	
		
		startOfLastGeneration = Time.time;
		generationNumber++;
		// Вывод номера поколения
		GameObject.Find("Canvas/Generation/Text").GetComponent<Text>().text = "Поколение: " + generationNumber;
	}
	
	void CheckSimulationEscape()
	{
		if (enableGenerateObs)
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
	}
	
	 public void BigBang(bool isBrainLoad = false)
	{
		// Сброс положений машинок
		ResetCars();

		// Сброс положения конечной точки
		ResetTarget(targetInCentre);

		speedOfTime = 0;
		Time.timeScale = 0;

		// Если данный метод вызывается для загрузки мозга - загрузить его
		// иначе заново инициализировать мозги
		if (isBrainLoad)
			ImportBrain();
		else		
		{
			for (int i = 0; i < scaleOfGeneration; i++)
				brain[i].BrainInit();
		}

		// Очистка карты
		//generateObsNextGen = false;
		if (generateObsNextGen)
		{
			sectorPosMap.Clear();
			//obsGenerator.Start();
			obsGenerator.SpawnObstacles(Vector3.zero);
			sectorPosMap.Add(Vector3.zero);
			generateObsNextGen = false;
			enableGenerateObs = true;
		}
		// Сброс статистики
		bestPrevGenScore = 0;
		gain = 0;
		avgPrevGenScore = 0;
		bestPrevGenScore = 0;
		generationNumber = 0;
		statPath = infoManager.StatInit();
		// Восстановление времени
		startOfLastGeneration = Time.time;
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
		//ImportSettings();

		// Инициализация
		obsGenerator = this.GetComponent<ObstacleGenerator>();
		scoreList = new int[scaleOfGeneration];
		infoManager = bigBrother.GetComponent<InformationManager>();

		// Инициализация записи статистики
		statPath = infoManager.StatInit();
		
		// Начальный спаун препятствий
		sectorPosMap = new List<Vector3>();
		if (generateObsNextGen)
		{
			obsGenerator.Start();
			obsGenerator.SpawnObstacles(Vector3.zero);
			sectorPosMap.Add(Vector3.zero);
			generateObsNextGen = false;
			enableGenerateObs = true;
		}
		obsGenerator.Start();


		// Сброс положения конечной точки
		ResetTarget(targetInCentre);
		
		// Спаун машин и инициализация вспомогательных переменных
		car = new List<GameObject>();
		brain = new Brain[scaleOfGeneration];
		prevPos = new Vector3[scaleOfGeneration];
		prevRot = new Vector3[scaleOfGeneration];
		radVectM = new float[scaleOfGeneration];
		maxRadVectM = new float[scaleOfGeneration];
		maxScore = new int[scaleOfGeneration];
		travelDists = new float[scaleOfGeneration];
		fullRotats = new float[scaleOfGeneration];
		for (int i = 0; i < scaleOfGeneration; i++)
		{
			travelDists[i] = 0;
			fullRotats[i] = 0;
			radVectM[i] = 0;
			maxRadVectM[i] = 0;
			maxScore[i] = 0;
			prevPos[i] = Vector3.zero;
			prevRot[i] = Vector3.zero;
			car.Add((GameObject)Instantiate(sampleCar, Vector3.up, Quaternion.identity));
			brain[i] = car[i].transform.GetChild(3).GetComponent<Brain>();
			brain[i].BrainInit();
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
		int MutationType = 0;				// Тип мутации:			0 - Полная замена гена на значение в интервале
		if (MutationType == 0)									 // 1 - Изменение гена на значение
			gene = Random.Range(-mutationRate, mutationRate);
		//else if (MutationType == 1)
		return gene;
	}
	
	bool isSurvived(int i, int count, float min, float max)
	{
		float rand = Random.Range(0.0f, 100.0f);
		if (rand < (i*(min-max)/count + max))
			return true;
		return false;
	}
	
	int DonorNumber(int count, float prob)
	{
		int type = 0;
		if (type == 0)
		{
			if (prob == 100)
				return 0;
			float rand = Random.Range(0.0f, 1.0f);
			prob = 1 - (prob/100);
			for (int i = 0; i < count; i++)
			{
				if (rand > prob)
					prob += (1-prob)/prob;
				else
					return i;
			}
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
	
	List<GameObject> SortCarsByScore(List<GameObject> cars)
	{
		cars = cars.OrderByDescending(GameObject => GameObject.transform.GetChild(3).GetComponent<Brain>().SCORE).ToList();
		for (int i = 0; i < scaleOfGeneration; i++)
		{
			brain[i] = cars[i].transform.GetChild(3).GetComponent<Brain>();
		}
		return cars;
	}
	
	void CalculateScore()
	{
		if (Time.time - lastCheck > scoreCheckPeriod)
		{
			Transform tHull;
			Transform tTarget = GameObject.Find("Target").GetComponent<Transform>();
			bool toTarget = tTarget.position == Vector3.up ? false : true;
			for (int i = 0; i < scaleOfGeneration; i++)
			{
				tHull = car[i].transform.GetChild(3);
				travelDists[i] += (tHull.position - prevPos[i]).magnitude;
				prevPos[i] = tHull.position;
				fullRotats[i] += Mathf.Abs(tHull.eulerAngles.y - prevRot[i].y);
				prevRot[i] = tHull.eulerAngles;
				radVectM[i] = tHull.position.magnitude;
				//if (radVectM[i] > maxRadVectM[i])
					maxRadVectM[i] = radVectM[i];
				
				int score = 0;
				if (!toTarget)
					score = (int)(maxRadVectM[i]*distanceImportance) + (int)(fullRotats[i]*rotationImportance);
				else 
				{
					Vector2 hullPos = new Vector2(tHull.position.x, tHull.position.z);
					Vector2 targetPos = new Vector2(tTarget.position.x, tTarget.position.z);
					Vector2 vTargetToRadius = (hullPos - targetPos).normalized * targetPos.magnitude;
					score = (int)((vTargetToRadius - hullPos).magnitude);
					maxScore[i] = score > maxScore[i] ?  score : maxScore[i];
					//score = maxScore[i];
				}
				if ((Time.time - startOfLastGeneration > dumbBrainCheckTime) ? score > minScore : true)
					brain[i].SCORE = score;
				else
					brain[i].isDead = true;
			}
			lastCheck = Time.time;
		}
	}
	
	void CheckUselessBrains(){}
	
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
			maxScore[i] = 0;
		}
	}

	void ResetTarget(bool inCentre)
	{
		if (changeTargetPos)
		{
			GameObject target = GameObject.Find("Target");
			Transform tTarget = target.GetComponent<Transform>();
			if (inCentre)
				tTarget.position = Vector3.up;
			else
			{
				float randX = Random.insideUnitCircle.x*(maxTargetRange-minTargetRange);
				float randY = Random.insideUnitCircle.y*(maxTargetRange-minTargetRange);
				tTarget.position = new Vector3(
					minTargetRange*Mathf.Sign(randX) + randX,
					5, 
					minTargetRange*Mathf.Sign(randY) + randY
				);
			}
			changeTargetPos = false;
		}
	}
	
	void BigBrotherManager()
	{
		if (!freeCam)
		{
			Vector3 util = Vector3.zero;
			Vector3 pos = new Vector3(car[0].transform.GetChild(3).gameObject.transform.position.x, 0.0f, car[0].transform.GetChild(3).gameObject.transform.position.z);
			int maxScore = 0;
			for (int i = 1; i < car.Count; i++)
				//if (car[i].transform.GetChild(3).gameObject.transform.position.magnitude > pos.magnitude)
				if (brain[i].SCORE > maxScore)
				{
					maxScore = brain[i].SCORE;
					pos = car[i].transform.GetChild(3).gameObject.transform.position;
				}
			bigBrother.transform.position = Vector3.SmoothDamp(bigBrother.transform.position,
															new Vector3(pos.x, bigBrother.transform.position.y, pos.z),
															ref util,
															bigBrotherSpeed);
		}
	}

	public void SaveBestBrain()
	{
		string path = statPath.Remove(23) + "BestBrain.txt";
		brain[0].Upload_Brain(path);
	}

	public void ImportBrain()
	{
		StreamReader rd = new StreamReader("./BestBrain.txt");
		str = rd.ReadLine();
		tstr = str.Substring(str.LastIndexOf(':')+2);
		try
		{
			t = System.Convert.ToInt32(tstr);
			tstr = str.Substring(str.IndexOf(':')+2, (str.IndexOf(',') - str.IndexOf(':')-2));
			t1 = System.Convert.ToInt32(tstr);
		}
		catch{}

		float[][][] w = new float[t][][];
		float[][][] b = new float[t][][];
		for (int i = 0; i < t; i++)
		{
			str = rd.ReadLine();
			tstr = str.Substring(str.LastIndexOf(':')+2);
			int t2 = System.Convert.ToInt32(tstr);
			w[i] = new float[t2][];
			b[i] = new float[t2][];
			for (int j = 0; j < t2; j++)
			{
				w[i][j] = new float[t1];
				b[i][j] = new float[t1];
			}
			t1 = t2;
		}

		for (int i = 0; i < w.Length; i++)
		{
			str = rd.ReadLine();
			for (int j = 0; j < w[i].Length; j++)
			{
				str = rd.ReadLine();
				str = rd.ReadLine();
				string[] lol = str.Split(new char[] {' '});
				for (int k = 0; k < w[i][j].Length; k++)
				{
					w[i][j][k] = (float)System.Convert.ToDouble(lol[k+1].Split(';')[0].Trim());
					b[i][j][k] = (float)System.Convert.ToDouble(lol[k+1].Split(';')[1].Trim());
				}
			}
		}

		for (int i = 0; i < (brain.Count() > 3 ? 3:brain.Count()); i++)
		{
			brain[i].weights = w;
			brain[i].biases = b;
		}
	}

	void ImportSettings()
	{
		StreamReader rd = new StreamReader("./Settings.txt");
	}

	void SelfDestruct()
	{
		bool isCounting = GameObject.Find("Canvas/SelfDestruct/EnDestruct").GetComponent<Toggle>().isOn;
		if (isCounting)
		{
			int deathMin = int.Parse(GameObject.Find("Canvas/SelfDestruct/MinField").GetComponent<InputField>().text);
			/*int*/ deathGen = int.Parse(GameObject.Find("Canvas/SelfDestruct/GenField").GetComponent<InputField>().text);
			if (((Time.realtimeSinceStartup/60 > deathMin) && deathMin != 0) || 
				((generationNumber >= deathGen) && deathGen!=0))
            	Application.Quit();
		}
	}
}