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
    public class OscillateSettings
    {
        [Tooltip("Enable oscillation")]
        public bool Enabled;

        [Tooltip("Oscillation axis (direction and amplitude)")]
        public Vector3 Axis = Vector3.up;

        [Tooltip("Oscillation amplitude in meters")]
        public float Amplitude = 1f;

        [Tooltip("Oscillation frequency in cycles per second")]
        public float Frequency = 1f;
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
    [SerializeField] private OscillateSettings _oscillate;
    [SerializeField] private RotationSettings _rotation;

    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private float _pingPongT;
    private int _pingPongDirection = 1;
    private float _pauseTimer;

    private float _pathLength;

    private float _oscillatePhase;

    void Start()
    {
        _startPosition = transform.localPosition;
        _endPosition = _startPosition + _pingPong.Offset;
        _pathLength = Vector3.Distance(_startPosition, _endPosition);
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        Vector3 position = _startPosition;

        if (_pingPong.Enabled)
            position = UpdatePingPong(deltaTime);

        if (_oscillate.Enabled)
            position += GetOscillateOffset(deltaTime);

        transform.localPosition = position;

        if (_rotation.Enabled)
            transform.Rotate(_rotation.Speed * deltaTime, Space.Self);
    }

    private Vector3 UpdatePingPong(float deltaTime)
    {
        if (_pauseTimer > 0)
        {
            _pauseTimer -= deltaTime;
            return Vector3.Lerp(_startPosition, _endPosition, _pingPongT);
        }

        if (_pathLength < 0.001f)
            return _startPosition;

        float step = _pingPong.Speed / _pathLength * deltaTime;
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

        return Vector3.Lerp(_startPosition, _endPosition, _pingPongT);
    }

    private Vector3 GetOscillateOffset(float deltaTime)
    {
        _oscillatePhase += _oscillate.Frequency * deltaTime * Mathf.PI * 2f;
        return _oscillate.Axis.normalized * (Mathf.Sin(_oscillatePhase) * _oscillate.Amplitude);
    }
}
