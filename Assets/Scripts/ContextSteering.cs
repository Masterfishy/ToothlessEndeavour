using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ContextSteering : MonoBehaviour
{
    public LayerMask interestLayer;
    public LayerMask dangerLayer;

    public bool debug;

    public float speed;
    public float interestRange;
    public float dangerRange;
    public int numRays;

    [Header("Weights")]
    public float dangerWeight;

    private new Rigidbody2D rigidbody;

    private float[] interests;
    private Collider2D[] interestResults;

    private float[] dangers;
    private Collider2D[] dangerResults;

    private Vector2 chosenDirection;
    private Vector2[] rayDirections;

    private Vector3 interestTarget;

    private bool isMoving;
    private Coroutine steeringCoroutine;

    /// <summary>
    /// Steers the game object towards the given position.
    /// </summary>
    /// <param name="position">The position to move to</param>
    public void MoveTowards(Vector3 position)
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine(Steering());
        }

        print($"Move towards {position}");
        interestTarget = position;
    }

    public void Stop()
    {
        isMoving = false;
        chosenDirection = Vector2.zero;
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        interests = new float[numRays];
        interestResults = new Collider2D[1];

        dangers = new float[numRays];
        dangerResults = new Collider2D[numRays];

        chosenDirection = Vector2.zero;
        rayDirections = new Vector2[numRays];

        interestTarget = Vector3.zero;
    }

    private void OnEnable()
    {
        for (int i = 0; i < numRays; i++)
        {
            float angle = i * 2 * Mathf.PI / numRays;
            rayDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    private void FixedUpdate()
    {
        rigidbody.velocity = chosenDirection.normalized * speed;
    }

    private IEnumerator Steering()
    {
        print("Steering...");

        while (isActiveAndEnabled && isMoving)
        {
            SetInterest();
            SetDanger();
            SetDirection();

            if (debug)
            {
                DebugDraw();
            }
            
            yield return new WaitForFixedUpdate();
        }
    }

    private void SetInterest()
    {
        if (interestTarget == null)
        {
            return;
        }

        for (int i = 0; i < numRays; i++)
        {
            Vector2 direction = interestTarget - transform.position;
            float projection = Vector2.Dot(rayDirections[i], direction) / direction.magnitude;

            interests[i] = Mathf.Max(0.1f, projection);
        }
    }

    private void SetDanger()
    {
        for (int i = 0; i < numRays; i++)
        {
            dangers[i] = 0f;

            RaycastHit2D result = Physics2D.Raycast(transform.position, rayDirections[i], dangerRange, dangerLayer);
            if (result)
            {
                float weight = 1 - (result.distance / dangerRange);
                dangers[i] = weight * dangerWeight;
            }
        }
    }

    private void SetDirection()
    {
        chosenDirection = Vector2.zero;
        for (int i = 0; i < numRays; i++)
        {
            chosenDirection += rayDirections[i] * (interests[i] - dangers[i]);
        }

        chosenDirection.Normalize();
    }

    private void DebugDraw()
    {
        for (int i = 0; i < numRays; i++)
        {
            Vector2 debug = rayDirections[i].normalized;
            Debug.DrawRay(transform.position, debug, Color.yellow);

            Vector2 interest = rayDirections[i] * interests[i];
            Debug.DrawRay(transform.position, interest, Color.green);

            Vector2 danger = rayDirections[i] * dangers[i];
            Debug.DrawRay(transform.position, danger, Color.red);
        }

        Debug.DrawRay(transform.position, chosenDirection, Color.magenta);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interestRange);
    }
}
