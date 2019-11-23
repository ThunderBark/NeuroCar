using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class InformationManager : MonoBehaviour
{
	public GameObject plane;
    UniverseManager manager;
    ObstacleGenerator generator;

    [Header("Элементы ПИ")]
    public Texture restart;
	
	private GUIStyle style;

    
    public float dragSpeed = 2;
    public Vector3 dragOrigin;
    public Vector3 mouse;
	
    void Start()
    {
        manager = plane.GetComponent<UniverseManager>();
        generator = plane.GetComponent<ObstacleGenerator>();
    }

    void OnGUI()
    {
        // Вывод номера поколения
        GUI.Box(new Rect(18, 20, 100, 20), "Поколение " + manager.generationNumber);

        if (GUI.Button(new Rect(130, 10, 40, 40), restart))
        {
            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        }
        if (GUI.Button(new Rect(180, 10, 40, 40), "CAM"))
            manager.freeCam = !manager.freeCam;

        // Блок настроек генераций препятствий
        GUI.Window(0, new Rect(16, 115, 208, 119), WindowManager, "Настройки препятствий");
        // Блок параметров поколения
        GUI.Window(1, new Rect(16, 240, 208, 84), WindowManager, "Параметры поколений");
        // Блок настроек естественного отбора
        GUI.Window(2, new Rect(16, 330, 208, 232), WindowManager, "Настройки отбора");
        
        // Слайдер времени
        GUI.Box(new Rect(16, 53, 208, 54), "Скорость времени " + manager.speedOfTime);
        manager.speedOfTime = (int)GUI.HorizontalSlider(new Rect(20, 80, 200, 40), manager.speedOfTime, 0.0f, 20.0f);

        Time.timeScale = manager.speedOfTime;

        CheckKeys();
        if (manager.freeCam)
            FreeCamMode();
    }

    void CheckKeys()
    {

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
            GUI.Label(new Rect(5, 143, 145, 20), "Шанс мутации:");
            manager.mutationChance = Convert.ToInt32(GUI.TextField(new Rect(150, 143, 50, 20),manager.mutationChance.ToString(), 3));
            // Важность мутации - ползун
            GUI.Label(new Rect(5, 163, 200, 20), "Сила мутации: " + (manager.mutationRate - manager.mutationRate%0.01).ToString());
            manager.mutationRate  = GUI.HorizontalSlider(new Rect(4, 183, 200, 20), manager.mutationRate, 0.0f, 1.0f);
            // Генерация препятствий в каждом поколении - переключатель
            GUI.Label(new Rect(5, 200, 145, 20), "Случ. преп. кажд. пок.: ");
            manager.generateObsNextGen = GUI.Toggle(new Rect(150, 200, 35, 35), manager.generateObsNextGen, "");
        }
        
    }
    
    void FreeCamMode()
    {
        Vector3 pos = Vector3.zero;
        Vector3 util = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position,
                                            new Vector3 (transform.position.x,
                                            transform.position.y + Input.mouseScrollDelta.y*(-4.0f) > 0 ? transform.position.y + Input.mouseScrollDelta.y*(-5.0f) : transform.position.y,
                                            transform.position.z),
                                            ref util, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime);

        RaycastHit hit;
        if (Input.GetMouseButtonDown(2))
        {
            mouse = Input.mousePosition;
            if (Physics.Raycast(transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out hit, Mathf.Infinity, 512))
                dragOrigin = hit.point;
            return;
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
}
