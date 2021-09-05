using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    public static Vector3 Round(this Vector3 v, float step)
    {
        v.x = Snap(v.x, step);
        v.y = Snap(v.y, step);
        v.z = Snap(v.z, step);
        return v;
    }
    
    public static Vector3 Round(this Vector3 v, float step, GridPlane plane)
    {
        switch (plane)
        {
            case GridPlane.XZ:
                v.x = Snap(v.x, step);
                v.z = Snap(v.z, step);
                break;
            case GridPlane.YZ:
                v.y = Snap(v.y, step);
                v.z = Snap(v.z, step);
                break;
            case GridPlane.XY:
                v.x = Snap(v.x, step);
                v.y = Snap(v.y, step);
                break;
        }
        return v;
    }

    public static float Snap(float value, float step)
    {
        float sign = Mathf.Sign(value);
        float mod = Mathf.Abs(value);
        
        if (mod % step < step / 2f)
        {
            mod = Mathf.Floor(mod / step) * step;
        }
        else
        {
            mod = Mathf.Ceil(mod / step) * step;
        }

        return mod * sign;
    }
}

[Serializable]
public enum GridPlane
{
    XZ,
    YZ,
    XY,
}
