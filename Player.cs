using System;
using System.Net;
using System.Numerics;

class Player
{
    public EndPoint playerEndPoint;
    public string name = "";
    public Player(EndPoint eP, string name)
    {
        playerEndPoint = eP;
        this.name = name;
    }

}