using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public float minDistanceToPathTarget;
    public Color pathGizmoColor;
    public float minNewPathDistance;

    private Vector3[] path;
    private int pathTargetIndex;
    private bool findNewPath;
    private Vector3 pathTargetPosition;

    private ContextSteering steering;

    private Coroutine pathCoroutine;

    private void Awake()
    {
        steering = GetComponent<ContextSteering>();
    }

    private void OnEnable()
    {
        pathTargetPosition = transform.position;
    }

    private void Update()
    {
        if (findNewPath)
        {
            PathRequestManager.Instance.RequestPath(gameObject.GetInstanceID(), transform.position, target.position, OnPathFound);
            findNewPath = false;
        }

        bool pathExists = path != null && path.Length > 0;

        if (pathExists)
        {
            findNewPath = Vector3.Distance(target.position, path[path.Length - 1]) > minNewPathDistance;
        }

        if (!pathExists && !findNewPath)
        {
            findNewPath = Vector3.Distance(target.position, transform.position) > minNewPathDistance;
        }
    }

    public void OnPathFound(Vector3[] pathFound, bool pathSuccess)
    {
        if (pathSuccess)
        {
            path = pathFound;

            if (pathCoroutine != null)
            {
                StopCoroutine(pathCoroutine);
            }

            pathCoroutine = StartCoroutine(FollowPath());
        }
    }

    private IEnumerator FollowPath()
    {
        if (path.Length <= 0)
        {
            yield break;
        }

        //print("Follow path...");

        pathTargetIndex = 0;
        pathTargetPosition = path[pathTargetIndex];
        steering.MoveTowards(pathTargetPosition);

        float distance = Vector3.Distance(transform.position, pathTargetPosition);

        while (pathTargetIndex < path.Length - 1 || distance >= minDistanceToPathTarget)
        {
            if (distance < minDistanceToPathTarget)
            {
                pathTargetIndex++;

                pathTargetPosition = path[pathTargetIndex];
                steering.MoveTowards(pathTargetPosition);
            }

            distance = Vector3.Distance(transform.position, pathTargetPosition);

            yield return new WaitForEndOfFrame();
        }

        steering.Stop();
        //Debug.Log("Finished following the path!");
    }

    private void OnDrawGizmos()
    {
        if (path == null)
        {
            return;
        }

        for (int i = 0; i < path.Length; i++)
        {
            Gizmos.color = pathGizmoColor;
            Gizmos.DrawSphere(path[i], 0.05f);

            if (i == 0) continue;

            Gizmos.DrawLine(path[i - 1], path[i]);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(pathTargetPosition, minDistanceToPathTarget);
    }
}
