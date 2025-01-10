using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 
/// </summary> 
public class Rotator : MonoBehaviour
{
    [HideInInspector] public Axis axis;
    [HideInInspector] public Vector3 customAxis;
    [HideInInspector] public SpeedType speedType;
    [HideInInspector] public float initial;
    [HideInInspector] public float acceleration;
    [HideInInspector] public float coefficient = 1;
    [HideInInspector] public Rigidbody reference;
    [HideInInspector] public float limit;
    [HideInInspector] public bool running;

    private float AngleSpeed
    {
        get
        {
            var result = initial;
            if (speedType == SpeedType.Sync)
            {
                var velocity = reference.velocity;
                var vDir = velocity.x > 0 ? -1 : 1;

                var posDir = reference.transform.eulerAngles.y > 180 ? 1 : -1;
                result += coefficient * velocity.magnitude * vDir * posDir / (float)Math.PI / 2 * 360;
            }
            var acc = speedType == SpeedType.Fixed ? 0 : acceleration;
            result += acceleration * Time / 1000;
            var limitAbs = Mathf.Abs(limit);
            var resultAbs = Mathf.Abs(result);
            return limit != 0 && resultAbs > limitAbs ? limit : result;
        }
    }

    private float Time => _stopwatch.ElapsedMilliseconds;

    private readonly Stopwatch _stopwatch = new Stopwatch();

    private Vector3 AxisVector
    {
        get
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.right;
                case Axis.Y:
                    return Vector3.up;
                case Axis.Z:
                    return Vector3.forward;
                case Axis.Custom:
                    return customAxis;
                case Axis.Red:
                    return transform.right;
                case Axis.Blue:
                    return transform.forward;
                case Axis.Green:
                    return transform.up;
                default:
                    return Vector3.zero;
            }
        }
    }

    private void Start()
    {
        if (running)
        {
            _stopwatch.Start();
        }
    }

    private void Update()
    {
        transform.Rotate(AxisVector, AngleSpeed * UnityEngine.Time.deltaTime, Space.World);

    }

    public enum Axis
    {
        X,
        Y,
        Z,
        Red,
        Blue,
        Green,
        Custom
    }

    public enum SpeedType
    {
        Fixed,
        Linear,
        Sync,
    }

    public void Run()
    {
        _stopwatch.Start();
    }
}
