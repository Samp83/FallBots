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

        [Tooltip("Intermediate waypoints (offsets from start position) to curve the path")]
        public Vector3[] Waypoints;
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

    private Vector3[] _pathPoints;
    private float _pathLength;

    private float _oscillatePhase;

    void Start()
    {
        _startPosition = transform.localPosition;
        _endPosition = _startPosition + _pingPong.Offset;
        BuildPath();
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
            return EvaluatePath(_pathPoints, _pingPongT);
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

        return EvaluatePath(_pathPoints, _pingPongT);
    }

    private Vector3 GetOscillateOffset(float deltaTime)
    {
        _oscillatePhase += _oscillate.Frequency * deltaTime * Mathf.PI * 2f;
        return _oscillate.Axis.normalized * (Mathf.Sin(_oscillatePhase) * _oscillate.Amplitude);
    }

    void OnDrawGizmosSelected()
    {
        if (_pingPong.Enabled)
        {
            Vector3[] points = GetEditorPathPoints();

            // Draw curved path
            Gizmos.color = Color.cyan;
            const int segments = 50;
            Vector3 prev = EvaluatePath(points, 0f);
            for (int i = 1; i <= segments; i++)
            {
                Vector3 curr = EvaluatePath(points, (float)i / segments);
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }

            // Draw start and end cubes
            Gizmos.DrawWireCube(points[0], Vector3.one * 0.3f);
            Gizmos.DrawWireCube(points[points.Length - 1], Vector3.one * 0.3f);

            // Draw waypoint spheres
            Gizmos.color = Color.yellow;
            for (int i = 1; i < points.Length - 1; i++)
                Gizmos.DrawWireSphere(points[i], 0.2f);
        }

        if (_oscillate.Enabled)
        {
            Vector3 origin = transform.position;
            Vector3 dir = _oscillate.Axis.normalized * _oscillate.Amplitude;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(origin - dir, origin + dir);
            Gizmos.DrawWireSphere(origin - dir, 0.15f);
            Gizmos.DrawWireSphere(origin + dir, 0.15f);
        }
    }

    private void BuildPath()
    {
        _pathPoints = BuildPathPoints(_startPosition);
        _pathLength = ComputePathLength(_pathPoints);
    }

    private Vector3[] GetEditorPathPoints()
    {
        if (Application.isPlaying)
            return _pathPoints;

        return BuildPathPoints(transform.position);
    }

    private Vector3[] BuildPathPoints(Vector3 origin)
    {
        int waypointCount = _pingPong.Waypoints != null ? _pingPong.Waypoints.Length : 0;
        Vector3[] points = new Vector3[2 + waypointCount];
        points[0] = origin;
        for (int i = 0; i < waypointCount; i++)
            points[i + 1] = origin + _pingPong.Waypoints[i];
        points[points.Length - 1] = origin + _pingPong.Offset;
        return points;
    }

    private static float ComputePathLength(Vector3[] points)
    {
        const int samplesPerSegment = 10;
        int totalSamples = Mathf.Max(points.Length * samplesPerSegment, 20);
        float length = 0f;
        Vector3 prev = EvaluatePath(points, 0f);
        for (int i = 1; i <= totalSamples; i++)
        {
            Vector3 curr = EvaluatePath(points, (float)i / totalSamples);
            length += Vector3.Distance(prev, curr);
            prev = curr;
        }
        return length;
    }

    /// Calcule la position sur le chemin au temps t (de 0 a 1).
    /// Le chemin est decoupe en segments (Start→WP1, WP1→WP2, ..., WPn→End).
    /// Chaque segment utilise une spline Catmull-Rom avec 4 points :
    /// les 2 extremites du segment + leurs 2 voisins qui definissent la direction
    /// d'entree et de sortie pour obtenir une courbe lisse.
    /// La courbe est lisse entre chaque segment car la direction de sortie
    /// de l'un est la meme que la direction d'entree du suivant.
    private static Vector3 EvaluatePath(Vector3[] points, float t)
    {
        // Pas de waypoints : simple ligne droite
        if (points.Length == 2)
            return Vector3.Lerp(points[0], points[1], t);

        // Convertit le t global (0-1) en index de segment + t local dans ce segment
        int segmentCount = points.Length - 1;
        float scaledT = t * segmentCount;
        int segment = Mathf.Min((int)scaledT, segmentCount - 1);
        float localT = scaledT - segment;

        // Recupere les 4 points pour Catmull-Rom :
        // p0 : voisin avant le segment (donne la direction d'entree, jamais touche par la courbe)
        // p1 : debut du segment (la courbe passe exactement par ce point)
        // p2 : fin du segment   (la courbe passe exactement par ce point)
        // p3 : voisin apres le segment (donne la direction de sortie, jamais touche par la courbe)
        // Aux bords du chemin, on repete le premier/dernier point (clamp).
        Vector3 p0 = points[Mathf.Max(segment - 1, 0)];
        Vector3 p1 = points[segment];
        Vector3 p2 = points[Mathf.Min(segment + 1, points.Length - 1)];
        Vector3 p3 = points[Mathf.Min(segment + 2, points.Length - 1)];

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    /// Interpolation cubique Catmull-Rom entre p1 et p2.
    /// La courbe passe par p1 (t=0) et p2 (t=1).
    /// p0 et p3 ne sont jamais touches par la courbe, ils servent uniquement
    /// a definir la direction de la courbe a ses extremites.
    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
