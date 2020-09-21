using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;

public class InformationManager : MonoBehaviour
{
	public GameObject plane;
    public GameObject menu;
    public GameObject brainPathListShow;
    UniverseManager manager;
    ObstacleGenerator generator;

    [Header("Элементы ПИ")]
    public Texture restart;
    public Texture importBrain;
	private GUIStyle style;

    
    public float dragSpeed = 2;
    public Vector3 dragOrigin;
    public Vector3 mouse;
    StreamWriter stat;

    public bool isFollowing;

    public Transform trFollowing;

    float prevSpeedOfTime = 0;
	
    void Start()
    {
        manager = plane.GetComponent<UniverseManager>();
        generator = plane.GetComponent<ObstacleGenerator>();
        isFollowing = false;
    }

    void OnGUI()
    {
        // Слайдер времени
        GameObject.Find("Canvas/SpeedOfTime/Text").GetComponent<Text>().text = "Скорость времени: " + manager.speedOfTime;
        manager.speedOfTime = GameObject.Find("Canvas/SpeedOfTime/Slider").GetComponent<Slider>().value;
        Time.timeScale = manager.speedOfTime;
        
		manager.freeCam = GameObject.Find("Canvas/FreeCam").GetComponent<Toggle>().isOn;

        // Блок настроек генераций препятствий
        GUI.Window(0, new Rect(16, 115, 208, 119), WindowManager, "Настройки препятствий");
        // Блок параметров поколения
        GUI.Window(1, new Rect(16, 240, 208, 84), WindowManager, "Параметры поколений");
        // Блок настроек естественного отбора
        GUI.Window(2, new Rect(16, 330, 208, 232), WindowManager, "Настройки отбора");
        

        CheckKeys();
        if (manager.freeCam)
        {
            FreeCamMode();
        }
        else
            isFollowing = false;
    }

    void WindowManager(int windowId)
    {
        if (windowId == 0)
        {
            //////////////////////////////////////////////
            // Блок настроек генераций препятствий
            // Ребро элементарной площадки
            GUI.Label(new Rect(5, 20, 145, 20), "Ребро элем. площадки:");
            generator.range = Convert.ToInt32(GUI.TextField(new Rect(150, 20, 50, 20),generator.range.ToString(), 4));
            // Плотность
            GUI.Label(new Rect(5, 40, 200, 20), "Плотность (ед на метр): " + (generator.density - generator.density%0.001).ToString());
            generator.density = GUI.HorizontalSlider(new Rect(4, 60, 200, 20), generator.density, 0.001f, 1.0f);
            // Размеры препятствий
            GUI.Label(new Rect(5, 75, 200, 20), "Размеры препятствий в см");
            GUI.Label(new Rect(5, 93, 50, 20), "min:");
            generator.minDimension = (float)Convert.ToDouble(GUI.TextField(new Rect(32, 94, 45, 20), (generator.minDimension*100).ToString()))/100;
            GUI.Label(new Rect(102, 93, 50, 20), "max:");
            generator.maxDimension = (float)Convert.ToDouble(GUI.TextField(new Rect(132, 94, 45, 20), (generator.maxDimension*100).ToString()))/100;
        }
        else if (windowId == 1)
        {
            //////////////////////////////////////////
            // Блок параметров поколения
            // 
            GUI.Label(new Rect(5, 20, 145, 20), "Max длит. поколения:");
            manager.maxDurationOfGen = Convert.ToInt32(GUI.TextField(new Rect(150, 20, 50, 20),manager.maxDurationOfGen.ToString(), 4));
            GUI.Label(new Rect(5, 40, 145, 20), "Количество особей:");
            manager.scaleOfGeneration = Convert.ToInt32(GUI.TextField(new Rect(150, 40, 50, 20),manager.scaleOfGeneration.ToString(), 4));
            GUI.Label(new Rect(5, 60, 145, 20), "Период доген.(мс):");
            manager.escapeCheckPeriod = Convert.ToInt32(GUI.TextField(new Rect(150, 60, 50, 20),manager.escapeCheckPeriod.ToString(), 4));
        }
        else if (windowId == 2)
        {
            //////////////////////////////////////////
            // Блок настроек естественного отбора
            // Коэффициент важности лучшего - ползун
            GUI.Label(new Rect(5, 15, 200, 20), "Коэф. важности лучшего: " + manager.winGenesSpreadRatio.ToString());
            manager.winGenesSpreadRatio = (int)GUI.HorizontalSlider(new Rect(4, 35, 200, 20), manager.winGenesSpreadRatio, 1, 100);
            // Максимальное количество выживших - ползун
            GUI.Label(new Rect(5, 47, 200, 20), "Max кол-во выживших: " + manager.maxSurvivorCnt.ToString());
            manager.maxSurvivorCnt = (int)GUI.HorizontalSlider(new Rect(4, 67, 200, 20), manager.maxSurvivorCnt, 1, manager.scaleOfGeneration);
            // Максимальный шанс выжить - ползун
            GUI.Label(new Rect(5, 79, 200, 20), "Max шанс выжить: " + manager.maxSurviveProb.ToString());
            manager.maxSurviveProb = (int)GUI.HorizontalSlider(new Rect(4, 99, 200, 20), manager.maxSurviveProb, 0, 100);
            // Минимальный шанс выжить - ползун
            GUI.Label(new Rect(5, 111, 200, 20), "Min шанс выжить: " + manager.minSurviveProb.ToString());
            manager.minSurviveProb = GUI.HorizontalSlider(new Rect(4, 131, 200, 20), manager.minSurviveProb, 0, 100);
            // Шанс мутации - поле ввода
            GUI.Label(new Rect(5, 143, 145, 20), "Шанс мутации*0.01:");
            manager.mutationChance = float.Parse(GUI.TextField(new Rect(150, 143, 50, 20), (manager.mutationChance*100).ToString(), 3))/100;
            // Важность мутации - ползун
            GUI.Label(new Rect(5, 163, 200, 20), "Сила мутации: " + (manager.mutationRate - manager.mutationRate%0.01).ToString());
            manager.mutationRate  = GUI.HorizontalSlider(new Rect(4, 183, 200, 20), manager.mutationRate, 0.0f, 1.0f);
            // Генерация препятствий в каждом поколении - переключатель
            GUI.Label(new Rect(5, 200, 145, 20), "Случ. преп. кажд. пок.: ");
            manager.generateObsNextGen = GUI.Toggle(new Rect(150, 200, 35, 35), manager.generateObsNextGen, "");
        }
        
    }
    
    public void Follow()
    {
        isFollowing = true;
    }

    void FreeCamMode()
    {
        Vector3 pos = Vector3.zero;
        Vector3 util = Vector3.zero;

        // Изменение отдаления камеры
        transform.position = Vector3.SmoothDamp(
            transform.position,
            new Vector3 (transform.position.x,
            transform.position.y + Input.mouseScrollDelta.y*(-4.0f) > 0 ? transform.position.y + Input.mouseScrollDelta.y*(-5.0f) : transform.position.y,
            transform.position.z),
            ref util, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime
        );

        // Изменение масштаба цели в зависимости от расстояния до камеры
        Transform tTarget = GameObject.Find("Target").GetComponent<Transform>();
        float tScale = transform.position.y*manager.targetScale/35.0f;
        tTarget.localScale = new Vector3(tScale, tScale, 1.0f);
        
        // Проверка на начало Drag-передвижения камеры
        RaycastHit hit;
        if (Input.GetMouseButtonDown(2))
        {
            isFollowing = false;
            mouse = Input.mousePosition;
            if (Physics.Raycast(
                    transform.position, 
                    Camera.main.ScreenPointToRay(Input.mousePosition).direction, 
                    out hit, Mathf.Infinity, 512))
                dragOrigin = hit.point;
            return;
        }

        // Алготирм слежки за машинкой
        if (isFollowing)
            transform.position = Vector3.SmoothDamp(
                transform.position,
                new Vector3 (trFollowing.position.x, transform.position.y, trFollowing.position.z),
                ref util, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime
            );
            

        // Проверка на выбор машинки для слежения
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(transform.position, 
                                Camera.main.ScreenPointToRay(Input.mousePosition).direction,
                                out hit, Mathf.Infinity, 256))
            {
                if (hit.collider.transform.name == "Hull")
                    trFollowing = hit.collider.transform;
                else if (hit.collider.transform.name != "Plane" && hit.collider.transform.name != "Cube" && hit.collider.transform.name != "Cylinder")
                    trFollowing = hit.collider.transform.parent.Find("./Hull").transform;
                else
                    return;
                isFollowing = true;
                GameObject.Find("Canvas/WeightsVisualisation").GetComponent<WeightVisualisation>().ShowBrain();
                return;
            }
        }

        if (!Input.GetMouseButton(2)) 
        {
            pos = dragOrigin;
            return;
        }

        if (Physics.Raycast(transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out hit, Mathf.Infinity, 512))
        {
            pos = transform.position + dragOrigin - hit.point;
            transform.position = Vector3.SmoothDamp(transform.position,
                                                new Vector3 (pos.x, transform.position.y, pos.z),
                                                ref util, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }

    public string StatInit()
    {
        if (!Directory.Exists("./Logs/"))
            Directory.CreateDirectory("./Logs/");
		string statPath = "./Logs/" + getStrDate_Time()  + "_log.txt";
        stat = new StreamWriter(statPath, false);
        stat.WriteLine("Generation bestFit avgFit fitGain");
        stat.Close();
        return statPath;
    }

    public string getStrDate_Time()
    {
        string time = DateTime.Now.ToString("HHmmss");
        string date = DateTime.Now.ToString("yyyyMMdd");
        string str = date + "_" + time;
        return str;
    }

    public void StatWrite(string statPath, int genNum, int bestFit, int avgFit, int fitGain)
    {
		stat = new StreamWriter(statPath, true);
        stat.Write(genNum.ToString() + " ");
        stat.Write(bestFit.ToString() + " ");
        stat.Write(avgFit.ToString() + " ");
        stat.Write(fitGain.ToString() + " ");
        stat.Write('\n');
        stat.Close();
    }

    void CheckKeys()
    {
        if (Input.GetKey("escape"))
            menu.SetActive(true);

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale != 0)
            {
                prevSpeedOfTime = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = (prevSpeedOfTime == 0 ? 1 : prevSpeedOfTime);
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToBeginning()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void ShowBrainPathSelect()
    {
        //Time.timeScale = 0;
        brainPathListShow.SetActive(true);
        brainPathListShow.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            Screen.width/2, Screen.height/2
        );
    }
}