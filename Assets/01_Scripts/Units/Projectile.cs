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

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

        if(Vector3.Distance(
            transform.position,
            target.position) < 0.1f)
        {
            onHit?.Invoke();

            Destroy(gameObject);
        }
    }
}