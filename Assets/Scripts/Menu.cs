using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    public void Scenes(int numberScene)
    {
        SceneManager.LoadScene(numberScene);
    }

    // Update is called once per frame
    void Update()
    {

    }
}