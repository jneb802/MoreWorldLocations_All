// Minimal stand-ins for the UnityEngine types the road logic uses.
// Behavior mirrors Unity's managed math so compiled mod sources run headless.

// ReSharper disable InconsistentNaming
namespace UnityEngine;

public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public float sqrMagnitude => x * x + y * y;
    public float magnitude => (float)System.Math.Sqrt(x * x + y * y);

    public void Normalize()
    {
        float m = magnitude;
        if (m > 1e-5f) { x /= m; y /= m; }
        else { x = 0; y = 0; }
    }

    public static float Distance(Vector2 a, Vector2 b)
    {
        float dx = a.x - b.x, dy = a.y - b.y;
        return (float)System.Math.Sqrt(dx * dx + dy * dy);
    }

    public Vector2 normalized
    {
        get
        {
            var v = this;
            v.Normalize();
            return v;
        }
    }

    public static float SqrMagnitude(Vector2 a) => a.x * a.x + a.y * a.y;

    public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x + b.x, a.y + b.y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x - b.x, a.y - b.y);
    public static Vector2 operator -(Vector2 a) => new(-a.x, -a.y);
    public static Vector2 operator *(Vector2 a, float d) => new(a.x * d, a.y * d);
    public static Vector2 operator *(float d, Vector2 a) => new(a.x * d, a.y * d);

    public override bool Equals(object? other) =>
        other is Vector2 v && v.x == x && v.y == y;

    public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);

    public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

    public override string ToString() => $"({x:F1}, {y:F1})";
}

public struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3 zero => new(0f, 0f, 0f);

    public static float Distance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
        return (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static float SqrMagnitude(Vector3 a) => a.x * a.x + a.y * a.y + a.z * a.z;

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3 operator *(Vector3 a, float d) => new(a.x * d, a.y * d, a.z * d);

    public override string ToString() => $"({x:F1}, {y:F1}, {z:F1})";
}

public struct Color
{
    public float r, g, b, a;

    public Color(float r, float g, float b, float a = 1f)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public static Color Lerp(Color x, Color y, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color(
            x.r + (y.r - x.r) * t,
            x.g + (y.g - x.g) * t,
            x.b + (y.b - x.b) * t,
            x.a + (y.a - x.a) * t);
    }
}

public static class Canvas
{
    public static void ForceUpdateCanvases() { }
}

public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public static class Mathf
{
    public const float PI = (float)System.Math.PI;

    public static float Sqrt(float f) => (float)System.Math.Sqrt(f);
    public static float Abs(float f) => System.Math.Abs(f);
    public static float Min(float a, float b) => System.Math.Min(a, b);
    public static float Max(float a, float b) => System.Math.Max(a, b);
    public static int Min(int a, int b) => System.Math.Min(a, b);
    public static int Max(int a, int b) => System.Math.Max(a, b);

    public static float Min(params float[] values)
    {
        float m = values[0];
        for (int i = 1; i < values.Length; i++) m = System.Math.Min(m, values[i]);
        return m;
    }

    public static float Max(params float[] values)
    {
        float m = values[0];
        for (int i = 1; i < values.Length; i++) m = System.Math.Max(m, values[i]);
        return m;
    }

    public static int CeilToInt(float f) => (int)System.Math.Ceiling(f);
    public static int FloorToInt(float f) => (int)System.Math.Floor(f);
    public static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
    public static float Cos(float f) => (float)System.Math.Cos(f);
    public static float Sin(float f) => (float)System.Math.Sin(f);
    public static float Pow(float f, float p) => (float)System.Math.Pow(f, p);
    public static float Atan2(float y, float x) => (float)System.Math.Atan2(y, x);
    public static float Round(float f) => (float)System.Math.Round(f);
    public static float Floor(float f) => (float)System.Math.Floor(f);
    public static int RoundToInt(float f) => (int)System.Math.Round(f);

    public static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

    public static float Clamp(float value, float min, float max) =>
        value < min ? min : value > max ? max : value;

    public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

    public static float SmoothStep(float from, float to, float t)
    {
        t = Clamp01(t);
        t = -2f * t * t * t + 3f * t * t;
        return to * t + from * (1f - t);
    }
}
