using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    StreamWriter setW;
    StreamReader setR;
    bool isFirstLoad = true;

    InputField ObsType, CrossType, MutType, GenScale;


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;

        ObsType = GameObject.Find("Canvas/Panel/ObsType").GetComponent<InputField>();
        CrossType = GameObject.Find("Canvas/Panel/CrossType").GetComponent<InputField>();
        MutType = GameObject.Find("Canvas/Panel/MutType").GetComponent<InputField>();
        GenScale = GameObject.Find("Canvas/Panel/GenScale").GetComponent<InputField>();

        isFirstLoad = !File.Exists("./Settings.txt");

        if (isFirstLoad)
        {
            ObsType.text = "0";
            CrossType.text = "0";
            MutType.text = "1";
            GenScale.text = "50";
        }
        else
        {
            setR = new StreamReader("./Settings.txt");
            ObsType.text = setR.ReadLine();
            CrossType.text = setR.ReadLine();
            MutType.text = setR.ReadLine();
            GenScale.text = setR.ReadLine();
            setR.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSomething()
    {
        setW = new StreamWriter("./Settings.txt");
        setW.WriteLine(ObsType.text);
        setW.WriteLine(CrossType.text);
        setW.WriteLine(MutType.text);
        setW.WriteLine(GenScale.text);
        setW.Close();

        SceneManager.LoadScene("Simulation");
    }

}
