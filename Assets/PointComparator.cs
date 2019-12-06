using System;
using System.Collections.Generic;
using UnityEngine;

public class PointComparator : IComparer<Point> {

    private Vector2 p;
    private Vector2 pr = new Vector2(0,1);
    
    public PointComparator(Point pt) {
        p = (Vector2)pt;
    }
    
    public int Compare(Point a, Point b) {
        Vector2 pa = (Vector2)a - p;
        Vector2 pb = (Vector2)b - p;
        
        float aAngle = Mathf.Atan2(pr.x*pa.y-pr.y*pa.x, pr.x*pa.x+pr.y*pa.y);
        float bAngle = Mathf.Atan2(pr.x*pb.y-pr.y*pb.x, pr.x*pb.x+pr.y*pb.y);

        return aAngle.CompareTo(bAngle);
    }
}