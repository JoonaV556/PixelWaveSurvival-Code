using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

/// <summary>
/// Drives enemy movement behavior. Currently chases the assigned target
/// </summary>
public class EnemyBehavior : MonoBehaviour
{
    public Vector2 MoveTargetDirection = Vector2.zero;

    public Seeker seeker;

    public CharacterInput Input;

    public Transform target; // Player etc.

    public MonoModifier PathModifier;

    public float closeToTargetTreshold = 0.2f;

    Vector2 moveTargetPosition;

    Vector2 SeekTargetPosition;

    public List<Vector2> pathPointsToFollow;
    public int pointToFollowIndex = 0;

    ABPath currentPath;


    private void Start()
    {
        // Request a path to the target
        RequestPath();
    }

    private void Update()
    {
        // Find the target position - player or smthng else - new paths are calculated to this position
        SeekTargetPosition = target.position;

        // Draw lines if we have a path
        if (currentPath != null)
        {
            DrawDebugLines(currentPath);
        }

        // Track movement along the path. If we have reached the current path point, move to the next path point
        TrackPath();

        // Make enemy move - Send move direction to enemy character input
        Input.MoveInput = MoveTargetDirection.normalized;
    }

    private void TrackPath()
    {
        // Track if we have reached the current target
        if (moveTargetPosition == null)
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, moveTargetPosition);

        if (distanceToTarget < closeToTargetTreshold)
        {
            bool morePointsToFollow = pointToFollowIndex < pathPointsToFollow.Count - 1;

            if (morePointsToFollow)
            {
                pointToFollowIndex++;
                moveTargetPosition = pathPointsToFollow[pointToFollowIndex];
            }
            else
            {
                // Request a new path
                RequestPath();
                Debug.Log("Reached path end.");
            }
        }
    }

    // Triggered when the path is received from seeker
    private void OnPathComplete(Path p)
    {
        // The path is now calculated!

        if (p.error)
        {
            Debug.LogError("Path failed: " + p.errorLog);
            return;
        }

        // Apply modifier on the path if modifier is set
        if (PathModifier != null)
        {
            PathModifier.Apply(p);
        }

        // Draw debug lines
        currentPath = p as ABPath;
        DrawDebugLines(p);

        // Make enemy input move towards next point in path
        pathPointsToFollow = p.vectorPath.ConvertAll(v => new Vector2(v.x, v.y)); // Get the path points as Vector2
        moveTargetPosition = pathPointsToFollow[1]; // Assign position to mova towards
        MoveTargetDirection = Direction(transform.position, moveTargetPosition).normalized; // Calculate direction to the target - the direction is sent to enemys character input component for driving movement
        pointToFollowIndex = 1; // Track the current point in path

        // Request a new path
        RequestPath();
    }

    // TODO - Make static util
    /// <summary>
    /// Returns direction vector pointing from one point to another
    /// </summary>
    /// <param name="start">start point</param>
    /// <param name="end">target point</param>
    /// <returns>Direction as Vector2. Not normalized.</returns>
    public Vector2 Direction(Vector2 start, Vector2 end)
    {
        return end - start;
    }

    private void DrawDebugLines(Path p)
    {
        // Draw the path in the scene view for 10 seconds
        for (int i = 0; i < currentPath.vectorPath.Count - 1; i++)
        {
            Debug.DrawLine(currentPath.vectorPath[i], currentPath.vectorPath[i + 1], Color.red, 0.5f);
        }
    }

    private void RequestPath()
    {
        seeker.StartPath(transform.position, SeekTargetPosition, OnPathComplete);
    }
}
