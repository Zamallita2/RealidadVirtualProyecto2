using UnityEngine;
using System.Collections;

public class UnitMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float distanciaMinima = 0.5f;

    [Header("Rotation")]
    public Vector3 modelRotationOffset;

    private Animator animator;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public IEnumerator MoveToPositionAndAttack(
    Vector3 targetPos,
    System.Action onArrive
    )
    {
        animator.SetBool("IsWalking", true);

        // Bloquear Y
        targetPos.y = transform.position.y;

        Vector3 lookDir =
            targetPos - transform.position;

        lookDir.y = 0f;

        if(lookDir != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir);

            transform.rotation =
                targetRotation *
                Quaternion.Euler(modelRotationOffset);
        }

        while(Vector3.Distance(transform.position, targetPos) > distanciaMinima)
        {
            Vector3 newPos =
                Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

            // Mantener Y fija
            newPos.y = transform.position.y;

            transform.position = newPos;

            yield return null;
        }

        animator.SetBool("IsWalking", false);

        onArrive?.Invoke();
    }public IEnumerator LookTargetPosition(
    Vector3 targetPos,
    System.Action onArrive
    )
    {
        Vector3 lookDir =
            targetPos - transform.position;

        lookDir.y = 0f;

        if(lookDir != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir);

            transform.rotation =
                targetRotation *
                Quaternion.Euler(modelRotationOffset);
        }
        

        yield return null;
        onArrive?.Invoke();
    }
    
    public IEnumerator LookOriginalPosition(
        System.Action onFinish
    )
    {
        float timer = 0f;
        float rotateDuration = 0.25f;

        Quaternion startRotation =
            transform.rotation;

        while(timer < rotateDuration)
        {
            timer += Time.deltaTime;

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    originalRotation,
                    timer / rotateDuration
                );

            yield return null;
        }

        transform.rotation = originalRotation;

        yield return new WaitForSeconds(0.5f);

        onFinish?.Invoke();
    }
    public IEnumerator ReturnToOriginalPosition(
        System.Action onFinish
    )
    {
        animator.SetBool("IsWalking", true);

        Vector3 lookDir =
            originalPosition - transform.position;
        originalPosition.y = transform.position.y;
        lookDir.y = 0f;

        if(lookDir != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir);

            transform.rotation =
                targetRotation *
                Quaternion.Euler(modelRotationOffset);
        }

        while(Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            Vector3 newPos =
                Vector3.MoveTowards(
                    transform.position,
                    originalPosition,
                    moveSpeed * Time.deltaTime
                );

            newPos.y = transform.position.y;

            transform.position = newPos;
            

            yield return null;
        }

        animator.SetBool("IsWalking", false);

        yield return new WaitForSeconds(0.2f);

        transform.position = originalPosition;

        float timer = 0f;
        float rotateDuration = 0.25f;

        Quaternion startRotation =
            transform.rotation;

        while(timer < rotateDuration)
        {
            timer += Time.deltaTime;

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    originalRotation,
                    timer / rotateDuration
                );

            yield return null;
        }

        transform.rotation = originalRotation;

        yield return new WaitForSeconds(0.5f);

        onFinish?.Invoke();
    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }
}