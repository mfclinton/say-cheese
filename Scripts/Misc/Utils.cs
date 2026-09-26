using System.Linq;
using UnityEngine;
using System;
using Random = System.Random;

public static class Utils
{
    public static int[] GenerateUniqueNumbers(int count, int max)
    {
        Random random = new Random();
        return Enumerable.Range(0, max).OrderBy(x => random.Next()).Take(count).ToArray();
    }
    
    public static Vector3 ProjectToCircle(CircleCollider2D circle, Vector3 point, bool clampToBottom = false, float bottomOffset = 0f)
    {
        float radius = circle.radius * circle.transform.localScale.x; // Assuming uniform scaling
        Vector3 center = circle.transform.position + (Vector3)circle.offset;
        if(clampToBottom)
            point.y = Mathf.Min(point.y, center.y + bottomOffset);
        
        Vector3 direction = point - center;
        
        float distance = direction.magnitude;
        
        if (distance < radius)
            return point;
        else
            return center + direction.normalized * radius;
    }
    
    public static string FormatTime(float time)
    {
        int mins = Mathf.FloorToInt(time / 60f);
        int secs = Mathf.FloorToInt(time % 60f);
        string text = "";
        if (mins > 0)
            text += mins + "m ";
        text += secs + "s";
        return text;
    }
}