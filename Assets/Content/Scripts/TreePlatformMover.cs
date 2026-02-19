using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 _endPosition;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 _startPosition;
    private float _time;

    void Start()
    {
        _startPosition = transform.localPosition; // prend la position actuelle
    }

    void Update()
    {
        _time += Time.deltaTime * moveSpeed;
        Move();
    }

    private void Move()
    {
        float t = Mathf.PingPong(_time, 1f);
        transform.transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, t);
    }
}