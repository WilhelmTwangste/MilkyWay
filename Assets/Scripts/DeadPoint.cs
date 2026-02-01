using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeadPoint : MonoBehaviour
{
    [SerializeField] private Transform Player;
    // Update is called once per frame
    private void Update()
    {
        if (Player.position.y <= transform.position.y)
        {
            SceneManager.LoadScene(9);
        }
    }
}