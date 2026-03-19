using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointsParent;

    [Header("Déplacement")]
    public float rotationSpeed = 8f;

    [Header("Waypoint Settings")]
    [Tooltip("Distance à partir de laquelle la voiture considère avoir atteint un waypoint")]
    public float reachDistance = 0.5f;

    [Header("Décalage de départ")]
    [Tooltip("Index du waypoint de départ (pour espacer les voitures sur le circuit)")]
    public int startWaypointIndex = 0;

    private Transform[] _waypoints;
    private int _currentIndex;

    private void Start()
    {
        if (waypointsParent == null)
        {
            Debug.LogError($"[CarController] {name} : waypointsParent non assigné !", this);
            enabled = false;
            return;
        }

        _waypoints = new Transform[waypointsParent.childCount];
        for (int i = 0; i < waypointsParent.childCount; i++)
            _waypoints[i] = waypointsParent.GetChild(i);

        if (_waypoints.Length < 2)
        {
            Debug.LogError($"[CarController] {name} : il faut au moins 2 waypoints !", this);
            enabled = false;
            return;
        }

        _currentIndex = startWaypointIndex % _waypoints.Length;
        transform.position = _waypoints[_currentIndex].position;
        _currentIndex = (_currentIndex + 1) % _waypoints.Length;
    }

    private void Update()
    {
        MoveTowardsWaypoint();
    }

    private void MoveTowardsWaypoint()
    {
        Transform target = _waypoints[_currentIndex];
        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position += CarManager.Instance.speed * Time.deltaTime * transform.forward;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= reachDistance)
            _currentIndex = (_currentIndex + 1) % _waypoints.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log(other.name + " a été percuté par " + name);
    }

    private void OnDrawGizmosSelected()
    {
        if (waypointsParent == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypointsParent.childCount; i++)
        {
            Transform a = waypointsParent.GetChild(i);
            Transform b = waypointsParent.GetChild((i + 1) % waypointsParent.childCount);
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.DrawSphere(a.position, 0.2f);
        }
    }
}