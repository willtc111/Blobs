using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : IEquatable<Line>{

    public Point[] points = new Point[2];

    public Line(Point a, Point b) {
        // Ensure that the points are always sorted for hashing purposes.
        if( a.x() < b.x() ) {
            points[0] = a;
            points[1] = b;
        } else if( a.x() > b.x() ) {
            points[0] = b;
            points[1] = a;
        } else {
            if( a.y() < b.y() ) {
                points[0] = a;
                points[1] = b;
            } else if( a.y() > b.y() ) {
                points[0] = b;
                points[1] = a;
            } else {
                Debug.Log("Line being made with identical points, watch out!");
                points[0] = a;
                points[1] = b;
            }
        }
    }

    public Point a() {
        return points[0];
    }

    public Point b() {
        return points[1];
    }

    public override String ToString() {
        return String.Format("({0},{1}),({2},{3})", points[0].x(), points[0].y(), points[1].x(), points[1].y());
    }

    // Amazing hash function
    public override int GetHashCode() {
        int hash = 17;
        hash = hash * 31 + a().GetHashCode();
        hash = hash * 31 + b().GetHashCode();
        return hash;
    }

    public override bool Equals(object other) {
        return other is Line && Equals((Line) other);
    }

    public bool Equals(Line other) {
        return a().Equals(other.a()) && b().Equals(other.b());
    }
       

}
