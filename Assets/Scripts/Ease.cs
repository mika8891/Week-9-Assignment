using UnityEngine;

public static class Easing 
{
    public static float EaseOutQuad(float t)
{
    t = Mathf.Clamp01(t);
    return 1.0f - (1.0f - t) * (1.0f - t);
}

}
