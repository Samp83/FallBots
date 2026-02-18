using UnityEngine;


public class PlatformMover : MonoBehaviour
{
    [System.Serializable]
    public class PingPongSettings
    {
        [Tooltip("Enable ping-pong movement")]
        public bool Enabled;

        [Tooltip("Offset from start position")]
        public Vector3 Offset = Vector3.up * 5f;

        [Tooltip("Movement speed in m/s")]
        public float Speed = 2f;

        [Tooltip("Pause duration at each end in seconds")]
        public float PauseDuration = 0f;
    }

    [System.Serializable]
    public class RotationSettings
    {
        [Tooltip("Enable rotation")]
        public bool Enabled;

        [Tooltip("Rotation speed in degrees/s per axis")]
        public Vector3 Speed = new Vector3(0, 45f, 0);
    }

    [SerializeField] private PingPongSettings _pingPong;
    [SerializeField] private RotationSettings _rotation;

    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private float _pingPongT;
    private int _pingPongDirection = 1;
    private float _pauseTimer;

    void Start()
    {
        _startPosition = transform.position;
        _endPosition = _startPosition + _pingPong.Offset;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        if (_pingPong.Enabled)
            UpdatePingPong(deltaTime);

        if (_rotation.Enabled)
            transform.Rotate(_rotation.Speed * deltaTime, Space.Self);
    }

    private void UpdatePingPong(float deltaTime)
    {
        if (_pauseTimer > 0)
        {
            _pauseTimer -= deltaTime;
            return;
        }

        float distance = Vector3.Distance(_startPosition, _endPosition);
        if (distance < 0.001f)
            return;

        float step = _pingPong.Speed / distance * deltaTime;
        _pingPongT += step * _pingPongDirection;

        if (_pingPongT >= 1f)
        {
            _pingPongT = 1f;
            _pingPongDirection = -1;
            _pauseTimer = _pingPong.PauseDuration;
        }
        else if (_pingPongT <= 0f)
        {
            _pingPongT = 0f;
            _pingPongDirection = 1;
            _pauseTimer = _pingPong.PauseDuration;
        }

        transform.position = Vector3.Lerp(_startPosition, _endPosition, _pingPongT);
    }

    void OnDrawGizmosSelected()
    {
        if (!_pingPong.Enabled)
            return;

        Vector3 start = Application.isPlaying ? _startPosition : transform.position;
        Vector3 end = Application.isPlaying ? _endPosition : transform.position + _pingPong.Offset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(start, Vector3.one * 0.3f);
        Gizmos.DrawWireCube(end, Vector3.one * 0.3f);
    }
}
