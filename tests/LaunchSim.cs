using System.Collections;
using UnityEngine;

/// <summary>
/// Simulates golf ball trajectory in the Unity scene.
/// Called by BallTracker when a shot is detected.
/// Unity 6 compatible.
/// </summary>
public class LaunchSim : MonoBehaviour
{
    [Header("Scene References")]
    public Transform ballTransform;
    public LaunchCalculations calc;

    [Header("Scene Scale")]
    public Vector3 teePosition = Vector3.zero;
    [Tooltip("Unity units per real-world metre (1.0 = 1:1 scale)")]
    public float metersToUnits = 1.0f;

    [Header("Physics")]
    [Tooltip("Use simplified physics (faster, less realistic)")]
    public bool useSimplifiedPhysics = false;

    [Header("Live Stats (read by UIStats)")]
    public bool hasReceivedShot = false;
    public float range = 0f;
    public float height = 0f;
    public float speed = 0f;
    public float verticalAngle = 0f;
    public float horizontalAngle = 0f;

    private Coroutine _simCoroutine;

    // ────────────────────────────────────────────────────────────────────────
    public void LaunchBall(float speedMs, float vAngle, float hAngle)
    {
        Debug.Log($"[LaunchSim] Launch — {speedMs:F1} m/s @ {vAngle:F1}°");

        if (calc == null)
        {
            Debug.LogError("[LaunchSim] LaunchCalculations not assigned!");
            return;
        }

        hasReceivedShot = true;
        speed = speedMs;
        verticalAngle = vAngle;
        horizontalAngle = hAngle;
        range = 0f;
        height = 0f;

        // Reset ball
        if (ballTransform != null)
            ballTransform.position = teePosition;

        // Stop any running simulation
        if (_simCoroutine != null)
            StopCoroutine(_simCoroutine);

        _simCoroutine = StartCoroutine(
            useSimplifiedPhysics
                ? SimulateSimple(speedMs, vAngle)
                : SimulateWithDrag(speedMs, vAngle, hAngle)
        );
    }

    // ────────────────────────────────────────────────────────────────────────
    // Simplified physics: no air resistance, pure parabola
    // Faster and more predictable for testing
    // ────────────────────────────────────────────────────────────────────────
    private IEnumerator SimulateSimple(float speedMs, float vAngle)
    {
        float vx = speedMs * Mathf.Cos(vAngle * Mathf.Deg2Rad);
        float vy = speedMs * Mathf.Sin(vAngle * Mathf.Deg2Rad);

        float currentR = 0f;
        float currentH = 0f;
        float maxH = 0f;

        while (true)
        {
            float dt = Time.deltaTime;

            vy -= 9.81f * dt;
            currentR += vx * dt;
            currentH += vy * dt;

            if (currentH > maxH)
            {
                maxH = currentH;
                height = maxH;
            }

            range = currentR;

            if (ballTransform != null)
            {
                ballTransform.position = teePosition + new Vector3(
                    currentR * metersToUnits,
                    Mathf.Max(0, currentH) * metersToUnits,
                    0f
                );
            }

            if (currentH <= 0f && currentR > 0.1f)
            {
                range = currentR;
                if (ballTransform != null)
                {
                    ballTransform.position = teePosition + new Vector3(
                        currentR * metersToUnits,
                        0f,
                        0f
                    );
                }

                Debug.Log($"[LaunchSim] Landed — range={range:F1}m  height={height:F1}m");
                break;
            }

            yield return null;
        }

        _simCoroutine = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Full physics with air resistance
    // More realistic but slower
    // ────────────────────────────────────────────────────────────────────────
    private IEnumerator SimulateWithDrag(float speedMs, float vAngle, float hAngle)
    {
        float hSpeed = calc.getHSpeed(speedMs, vAngle, hAngle);
        float vSpeed = calc.getInitVSpeed(speedMs, vAngle);

        float currentR = 0f;
        float currentH = 0f;
        float maxH = 0f;

        while (true)
        {
            float dt = Time.deltaTime;

            // Air resistance on horizontal component
            float drag = calc.getAirResAccel(Mathf.Abs(hSpeed));
            hSpeed = Mathf.Max(0f, hSpeed - drag * dt * 0.3f);  // reduced drag factor

            // Gravity
            vSpeed -= 9.81f * dt;

            // Integrate
            currentR += hSpeed * dt;
            currentH += vSpeed * dt;

            if (currentH > maxH)
            {
                maxH = currentH;
                height = maxH;
            }

            range = currentR;

            if (ballTransform != null)
            {
                ballTransform.position = teePosition + new Vector3(
                    currentR * metersToUnits,
                    Mathf.Max(0, currentH) * metersToUnits,
                    0f
                );
            }

            if (currentH <= 0f && currentR > 0.1f)
            {
                range = currentR;
                if (ballTransform != null)
                {
                    ballTransform.position = teePosition + new Vector3(
                        currentR * metersToUnits,
                        0f,
                        0f
                    );
                }

                Debug.Log($"[LaunchSim] Landed — range={range:F1}m  height={height:F1}m");
                break;
            }

            yield return null;
        }

        _simCoroutine = null;
    }
}
