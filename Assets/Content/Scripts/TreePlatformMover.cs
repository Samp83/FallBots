using UnityEngine;

public class TreePlatformMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 _endPosition;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private AnimationCurve _moveCurve;

    private Vector3 _startPosition;
    private float _time;

    void Start()
    {
        // prend la position actuelle
        _startPosition = transform.localPosition;
    }

    void Update()
    {
        _time += Time.deltaTime * moveSpeed;
        Move();
    }

    private void Move()
    {
        float t = Mathf.PingPong(_time, 1f);
        transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, _moveCurve.Evaluate(t));
    }

    [ContextMenu("Set End Position Here")]
    private void SetEndPositionHere()
    {
        _endPosition = transform.localPosition;
    }
}