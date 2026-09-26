using UnityEngine;

public class RandomCircleMovement : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float changeInterval = 2f;

    private Vector3 targetPosition;
    private Vector3 originPosition;
    private float timer;

    void Awake()
    {
        originPosition = transform.position;
        targetPosition = originPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > changeInterval)
        {
            timer = 0f;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            targetPosition = originPosition + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}
