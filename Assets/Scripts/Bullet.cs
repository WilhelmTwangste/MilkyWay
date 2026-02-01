using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    public float distance;
    public int damage;
    public LayerMask whatIsSolid;

    public Transform enemy;

    private Vector2 moveDirection;
    private int count;

    void Start()
    {
        moveDirection = transform.right; // Предполагаем, что пуля движется вправо
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Работаем с Vector2 для позиции
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = currentPos + moveDirection; // Складываем Vector2 с Vector2
        currentPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);
        transform.position = new Vector3(currentPos.x, currentPos.y, transform.position.z); // Применяем новую позицию, сохраняя Z

        if (enemy != null && Vector2.Distance(currentPos, enemy.position) < 0.1f)
        {
            enemy.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    public void SetTarget(Transform target)
    {
        enemy = target;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (enemy != null && other.transform == enemy)
        {
            enemy.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (((1 << other.gameObject.layer) & whatIsSolid) != 0)
        {
            Destroy(gameObject);
        }
    }
}