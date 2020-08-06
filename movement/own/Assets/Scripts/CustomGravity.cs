using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public static class CustomGravity
{
    public static Vector3 GetGravity(Vector3 position)
    {
        return position.normalized * Physics.gravity.y;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 up = position.normalized;
        return up * Physics.gravity.y;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 up = position.normalized;
        upAxis = Physics.gravity.y < 0f ? up : -up;
        return upAxis * Physics.gravity.y;
    }
}
