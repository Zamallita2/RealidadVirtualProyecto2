using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    private Transform target;

    private Action onHit;

    public float speed = 8f;

    public void Initialize(
        Transform newTarget,
        Action callback)
    {
        target = newTarget;
        onHit = callback;
    }

    void Update()
    {
        if(target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 dir = new Vector3(target.position.x,target.position.y+0.3f,target.position.z);
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                dir,
                speed * Time.deltaTime
            );

        if(Vector3.Distance(
            transform.position,
            dir) < 0.1f)
        {
            onHit?.Invoke();

            Destroy(gameObject);
        }
    }
}