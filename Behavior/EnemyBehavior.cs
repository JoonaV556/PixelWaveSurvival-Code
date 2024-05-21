using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

/// <summary>
/// Drives enemy movement behavior. Currently chases the assigned target
/// </summary>
public class EnemyBehavior : MonoBehaviour
{
    public Seeker seeker; // Seeker component. Used for pathfinding

    public CharacterInput Input; // Input component of the enemy character. MoveInput vector is updated for movement purposes

    public Transform target; // Player etc.

    public MonoModifier PathModifier; // Modifier applied to the paths before they are used, optional.

    public float closeToPointTreshold = 0.2f; // How close to a path point we need to be to consider it reached

    public float closeToTargetDistance = 0.9f; // How close to the target object to stop moving

    public int pointToFollowIndex = 0; // Index of current target point in current path

    ABPath currentPath; // Current path to follow, casted to APPath for debug drawing (no idea if required)

    public float targetPositionChangeTreshold = 0.45f; // If target moves more than this amount from last path calc position, new path is calculated

    float distanceToTargetObj = 0f; // Distance to target

    Vector2 targetLastPathRequestPosition; // Last position of target when path was requested

    public List<Vector2> pathPointsToFollow;

    Vector2 MoveTargetDirection = Vector2.zero; // Direction to move towards, supplied to Input component

    Vector2 pathTargetPosition = Vector2.zero; // Position to move towards. Should be one of the points in the current navigation path

    Vector2 SeekTargetPosition = Vector2.zero; // Actual position of the target obejct in world space. Updated every frame.

    private void Start()
    {
        // Find the target position - player or smthng else - new paths are calculated to this position
        SeekTargetPosition = target.position;
        // Request a path to the target
        RequestPath();
    }

    private void Update()
    {
        // Find the target position - player or smthng else - new paths are calculated to this position
        SeekTargetPosition = target.position;

        // Check if its time to request a new path (target has moved enough)
        if (targetLastPathRequestPosition != null)
        {
            bool targetMovedEnough = GetTargetMovedEnough();

            if (targetMovedEnough)
            {
                RequestPath();
            }
        }

        // Draw lines if we have a path
        if (currentPath != null)
        {
            DrawDebugLines(currentPath);
        }

        // Track movement along the path. If we have reached the current path point, move to the next path point
        TrackPath();
        // Update movement target position
        MoveTargetDirection = Direction(transform.position, pathTargetPosition).normalized;

        distanceToTargetObj = Vector2.Distance(transform.position, SeekTargetPosition);

        // Stop moving if we are close enough to target
        if (distanceToTargetObj < closeToTargetDistance)
        {
            // Stop moving
            Input.MoveInput = Vector2.zero;
        }
        else
        {
            // Continue moving
            Input.MoveInput = MoveTargetDirection.normalized;
        }

        // Update look input
        Input.LookInput = Direction(transform.position, SeekTargetPosition).normalized;
    }

    /// <summary>
    /// Check if target has moved enough from the last path request position. Used for updating path if target has moved.
    /// </summary>
    /// <returns>true if moveed enough</returns>
    private bool GetTargetMovedEnough()
    {
        return Vector2.Distance(targetLastPathRequestPosition, SeekTargetPosition) > targetPositionChangeTreshold;
    }

    private void TrackPath()
    {
        // Track if we have reached the current target
        if (pathTargetPosition == null)
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, pathTargetPosition);

        if (distanceToTarget < closeToPointTreshold)
        {
            bool morePointsToFollow = pointToFollowIndex < pathPointsToFollow.Count - 1;

            if (morePointsToFollow)
            {
                pointToFollowIndex++;
                pathTargetPosition = pathPointsToFollow[pointToFollowIndex];
            }
            else
            {
                // Path end has been reached
                Debug.Log("Reached path end.");
            }
        }
    }

    // Triggered when the path is received from seeker
    private void OnPathComplete(Path p)
    {
        // The path is now calculated!

        // Track the last position of the target when path was requested - used for checking if target has moved
        targetLastPathRequestPosition = target.position;

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
        pathTargetPosition = pathPointsToFollow[1]; // Assign position to mova towards
        MoveTargetDirection = Direction(transform.position, pathTargetPosition).normalized; // Calculate direction to the target - the direction is sent to enemys character input component for driving movement
        pointToFollowIndex = 1; // Track the current point in path
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
        // print("Requesting new path");
        targetLastPathRequestPosition = SeekTargetPosition;
        seeker.StartPath(transform.position, SeekTargetPosition, OnPathComplete);
    }
}
