using UnityEngine;
public static class TransformExtensions
{
    public static void SetX(this Transform transform, float x)
    {
        Vector3 pos = transform.position;
        pos.x = x;
        transform.position = pos;
    }

    public static void SetY(this Transform transform, float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    public static void SetZ(this Transform transform, float z)
    {
        Vector3 pos = transform.position;
        pos.z = z;
        transform.position = pos;
    }
}