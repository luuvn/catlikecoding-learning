﻿using UnityEngine;
using System.Collections;
using System.Transactions;

public class PositionInterpolator : MonoBehaviour
{
    [SerializeField] private Rigidbody body = default;

    [SerializeField] private Vector3 from = default, to = default;

    [SerializeField] private Transform relativeTo = default;

    public void Interpolate(float t)
    {
        Vector3 p;
        if (relativeTo)
        {
            p = Vector3.LerpUnclamped(relativeTo.TransformPoint(from), relativeTo.TransformPoint(to), t);
        }
        else
        {
            p = Vector3.LerpUnclamped(@from, to, t);
        }
        body.MovePosition(p);
    }
}
