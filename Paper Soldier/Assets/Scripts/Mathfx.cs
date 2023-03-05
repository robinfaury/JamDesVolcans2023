using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class from : http://wiki.unity3d.com/index.php?title=Mathfx
/// </summary>
public sealed class Mathfx
{
    //Ease in out
    public static float Hermite(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
    }

    public static Vector2 Hermite(Vector2 start, Vector2 end, float value)
    {
        return new Vector2(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value));
    }

    public static Vector3 Hermite(Vector3 start, Vector3 end, float value)
    {
        return new Vector3(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value), Hermite(start.z, end.z, value));
    }

    //Ease out
    public static float Sinerp(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
    }

    public static Vector2 Sinerp(Vector2 start, Vector2 end, float value)
    {
        return new Vector2(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)));
    }

    public static Vector3 Sinerp(Vector3 start, Vector3 end, float value)
    {
        return new Vector3(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.z, end.z, Mathf.Sin(value * Mathf.PI * 0.5f)));
    }
    //Ease in
    public static float Coserp(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
    }

    public static Vector2 Coserp(Vector2 start, Vector2 end, float value)
    {
        return new Vector2(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value));
    }

    public static Vector3 Coserp(Vector3 start, Vector3 end, float value)
    {
        return new Vector3(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value), Coserp(start.z, end.z, value));
    }

    //Boing
    public static float Berp(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static Vector2 Berp(Vector2 start, Vector2 end, float value)
    {
        return new Vector2(Berp(start.x, end.x, value), Berp(start.y, end.y, value));
    }

    public static Vector3 Berp(Vector3 start, Vector3 end, float value)
    {
        return new Vector3(Berp(start.x, end.x, value), Berp(start.y, end.y, value), Berp(start.z, end.z, value));
    }

    //Like lerp with ease in ease out
    public static float SmoothStep(float x, float min, float max)
    {
        x = Mathf.Clamp(x, min, max);
        float v1 = (x - min) / (max - min);
        float v2 = (x - min) / (max - min);
        return -2 * v1 * v1 * v1 + 3 * v2 * v2;
    }

    public static Vector2 SmoothStep(Vector2 vec, float min, float max)
    {
        return new Vector2(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max));
    }

    public static Vector3 SmoothStep(Vector3 vec, float min, float max)
    {
        return new Vector3(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max), SmoothStep(vec.z, min, max));
    }

    public static float Lerp(float start, float end, float value)
    {
        return ((1.0f - value) * start) + (value * end);
    }

    public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
        float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
        return lineStart + (closestPoint * lineDirection);
    }

    public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 fullDirection = lineEnd - lineStart;
        Vector3 lineDirection = Vector3.Normalize(fullDirection);
        float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
        return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
    }

    //Bounce
    public static float Bounce(float x)
    {
        return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
    }

    public static Vector2 Bounce(Vector2 vec)
    {
        return new Vector2(Bounce(vec.x), Bounce(vec.y));
    }

    public static Vector3 Bounce(Vector3 vec)
    {
        return new Vector3(Bounce(vec.x), Bounce(vec.y), Bounce(vec.z));
    }

    // test for value that is near specified float (due to floating point inprecision)
    // all thanks to Opless for this!
    public static bool Approx(float val, float about, float range)
    {
        return ((Mathf.Abs(val - about) < range));
    }

    // test if a Vector3 is close to another Vector3 (due to floating point inprecision)
    // compares the square of the distance to the square of the range as this 
    // avoids calculating a square root which is much slower than squaring the range
    public static bool Approx(Vector3 val, Vector3 about, float range)
    {
        return ((val - about).sqrMagnitude < range * range);
    }

    /*
      * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
      * This is useful when interpolating eulerAngles and the object
      * crosses the 0/360 boundary.  The standard Lerp function causes the object
      * to rotate in the wrong direction and looks stupid. Clerp fixes that.
      */
    public static float Clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
        float retval = 0.0f;
        float diff = 0.0f;

        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        // Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
        return retval;
    }


    private static float SqrDistance(ref Vector3 p1, ref Vector3 p2)
    {
        return (p2.x - p1.x) * (p2.x - p1.x)
            + (p2.y - p1.y) * (p2.y - p1.y)
            + (p2.z - p1.z) * (p2.z - p1.z);
    }

    public static Vector3 GetNearestPoint(Vector3 pPoint, Vector3[] pLine, out float sqrDist)
    {
        sqrDist = float.MaxValue;
        Vector3 lNearest = pLine[0];
        for (int i = 0; i < pLine.Length - 1; i++)
        {
            Vector3 lPoint = ProjectPointOnLineSegment(pLine[i], pLine[i + 1], pPoint);
            float lSqr = SqrDistance(ref pPoint, ref lPoint);
            if (lSqr < sqrDist)
            {
                sqrDist = lSqr;
                lNearest = lPoint;
            }
        }
        return lNearest;
    }

    //This function returns a point which is a projection from a point to a line.
    //The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
    public static Vector3 ProjectPointOnLine(Vector3 A, Vector3 B, Vector3 P)
    {
        //get vector from point on line to point in space
        Vector3 AP = P - A;
        float t = Vector3.Dot(AP, B);
        return A + B * t;
    }

    //This function returns a point which is a projection from a point to a line segment.
    //If the projected point lies outside of the line segment, the projected point will 
    //be clamped to the appropriate line edge.
    //If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
    public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
    {
        Vector3 vector = linePoint2 - linePoint1;
        Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);
        int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);
        switch (side)
        {
            case 0:
                return projectedPoint;
            case 1:
                return linePoint1;
            case 2:
                return linePoint2;
            default:
                return Vector3.zero;
        }
    }

    //This function finds out on which side of a line segment the point is located.
    //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
    //the line segment, project it on the line using ProjectPointOnLine() first.
    //Returns 0 if point is on the line segment.
    //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
    //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
    public static int PointOnWhichSideOfLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AB = B - A;
        Vector3 AP = P - A;
        float dot = Vector3.Dot(AP, AB);
        if (dot > 0)
        {
            //point is on the line segment
            if ( AP.sqrMagnitude <= AB.sqrMagnitude)
                return 0;
            else //point is not on the line segment and it is on the side of linePoint2
                return 2;
        }
        //Point is not on side of linePoint2, compared to linePoint1.
        //Point is not on the line segment and it is on the side of linePoint1.
        else
        {
            return 1;
        }
    }
}