using UnityEngine;

public struct SafeFloat
{
    private float offset;
    private float value;

    public SafeFloat(float value = 0)
    {
        offset = Random.Range(-10f, 10f);
        this.value = value + offset;
    }
    public float GetValue()
    {
        return value;
    }
}