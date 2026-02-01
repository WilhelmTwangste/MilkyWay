using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform Player;

    // Update is called once per frame
    private void Update()
    {
        if (transform.position.y < Player.position.y)
        {
            transform.position = new Vector3(transform.position.x, Player.position.y, transform.position.z);
        }
    }
}