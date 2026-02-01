using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidePoint : MonoBehaviour
{
    [SerializeField] private Transform OpPoint;
    [SerializeField] private Transform Player;

    [SerializeField] private bool leftSide;

    // Update is called once per frame
    private void Update()
    {
        if (leftSide)
        {
            if (transform.position.x > Player.position.x)
            {
                MoveToOppositePoint();
            }
        }
        else
        {
            if (transform.position.x < Player.position.x)
            {
                MoveToOppositePoint();
            }
        }
    }
    private void MoveToOppositePoint() => Player.position = new Vector2(OpPoint.position.x, Player.position.y);
}