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
        findNewPath = true;
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

        pathTargetIndex = 0;

        while (pathTargetIndex < path.Length)
        {
            float distance = Vector3.Distance(transform.position, pathTargetPosition);

            if (distance < minDistanceToPathTarget)
            {
                pathTargetPosition = path[pathTargetIndex];
                //steering.MoveTo(pathTargetPosition);

                pathTargetIndex++;
            }

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Finished following the path!");
    }

    private void OnDrawGizmos()
    {
        if (path == null)
        {
            return;
        }

        for (int i = pathTargetIndex; i < path.Length; i++)
        {
            Gizmos.color = pathGizmoColor;
            Gizmos.DrawSphere(path[i], 0.05f);

            if (i == pathTargetIndex)
            {
                Gizmos.DrawLine(transform.position, path[i]);
            }
            else
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(pathTargetPosition, minDistanceToPathTarget);
    }
}
