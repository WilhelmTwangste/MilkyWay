using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordManager : MonoBehaviour
{
    [SerializeField] public Text Record1;
    //[SerializeField] public Text Record2;
    //[SerializeField] public Text Record3;
    //[SerializeField] public Text Record4;
    //[SerializeField] public Text Record5;

    int rec1, rec2, rec3, rec4, rec5;
    //int massiv[] = new int[5];

    // Start is called before the first frame update
    void Start()
    {
        Record1.text = PlayerPrefs.GetInt("score").ToString();
        //rec1 = PlayerPrefs.GetInt("score");
        //rec2 = PlayerPrefs.GetInt("score");
        //rec3 = PlayerPrefs.GetInt("score");
        //rec4 = PlayerPrefs.GetInt("score");
        //rec5 = PlayerPrefs.GetInt("score");
    }

    // Update is called once per frame
    void Update()
    {

    }
}