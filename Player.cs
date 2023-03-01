using System;
using System.Net;
using System.Numerics;

class Player
{
    public EndPoint playerEndPoint;
    public string name = "";
    public string currentXYZ = "";
    public string destXYZ = "";
    public Player(EndPoint eP, string name, string newXYZ)
    {
        playerEndPoint = eP;
        this.name = name;
        currentXYZ = newXYZ;
    }

    public Vector3 LerpFromTo(Vector3 start, Vector3 where, float percentage)
    {
        var distance = where - start;
        return start + distance * percentage;
    }
    public static Vector3 MoveTowards11(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 direction = target - current;
        float magnitude = Magnitude(direction);

        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current + direction / magnitude * maxDistanceDelta;
    }
    public static float Magnitude(Vector3 vector)
    {
        return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Y * vector.Y);
    }
}