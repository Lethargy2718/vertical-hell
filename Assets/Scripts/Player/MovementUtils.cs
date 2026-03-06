using UnityEngine;

public static class MovementUtils
{
    public static float ApplyHorizontal(float current, float input, float maxSpeed, float acceleration, float deceleration, float dt)
    {
        if (Mathf.Abs(input) <= 0.01f)
        {
            return Mathf.MoveTowards(current, 0, deceleration * dt);
        }

        return Mathf.MoveTowards(current, input * maxSpeed, acceleration * dt);
    }
}