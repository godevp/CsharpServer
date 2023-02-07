using System;
using System.Net;
using System.Numerics;

class Player
{
    public EndPoint playerEndPoint;
    public string name = "";
    public string currentXY = "";
    public string destXY = "";
    public Player(EndPoint eP, string name, string newXY)
    {
        playerEndPoint = eP;
        this.name = name;
        currentXY = newXY;
    }

}