using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public int health;
    public float timetime;
    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    private void Update()
    {
        timetime -= Time.deltaTime;
        if (health <= 0 || timetime <= 0)
        {
            Destroy(gameObject);
            SpawnManager.spawnNow--;
            //Destroy(GameObject.Find("Point One"));
            //Destroy(GameObject.Find("Point Two"));
            //Destroy(GameObject.Find("Point Tree"));
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(3);
        }
    }
}