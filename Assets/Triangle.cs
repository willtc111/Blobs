using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle {

    public Point[] points = new Point[3];

    public Triangle(Point a, Point b, Point c) {
        // Enforce counter clockwise order.
        if( LeftOf(c, a, b) ) {
            points[0] = a;
            points[1] = b;
            points[2] = c;
        } else {
            points[0] = a;
            points[1] = c;
            points[2] = b;
        }
    }

    // Does this triangle contain the given point?
    public bool PointInside(Point p) {
        return LeftOf(p, points[0], points[1])
            && LeftOf(p, points[1], points[2])
            && LeftOf(p, points[2], points[0]);
    }

    // Calculate the triangle's circumcircle
    public float[] Circle() {
        float xy12 = Mathf.Pow(points[0].x(),2) - Mathf.Pow(points[1].x(),2) + Mathf.Pow(points[0].y(),2) - Mathf.Pow(points[1].y(),2);
        float xy13 = Mathf.Pow(points[0].x(),2) - Mathf.Pow(points[2].x(),2) + Mathf.Pow(points[0].y(),2) - Mathf.Pow(points[2].y(),2);
        float y21 = 2*(points[1].y()-points[0].y());
        float x21 = 2*(points[1].x()-points[0].x());
        float y31 = 2*(points[2].y()-points[0].y());
        float x31 = 2*(points[2].x()-points[0].x());

        float y = (xy13 * x21 - xy12 * x31) / (y21 * x31 - x21 * y31);
        float x = (xy12 + y * y21) / (-x21);
        float r = Mathf.Sqrt(Mathf.Pow(points[0].x()-x,2) + Mathf.Pow(points[0].y()-y,2));

        return new float[]{x,y,r};
    }

    // Does this triangle's circumcircle contain the given point?
    public bool CircleContains(Point p) {
        float[] circle = Circle();
        
        float a = circle[0] - p.x();
        float b = circle[1] - p.y();
        float dist = Mathf.Sqrt(a*a + b*b);

        return dist <= circle[2];
    }
    
    // Is point p left of line a->b?
    public bool LeftOf(Point p, Point a, Point b) {
        Vector3 cp = Vector3.Cross((Vector3)b-(Vector3)a, (Vector3)p-(Vector3)a);
        return cp[2] > 0;
    }

    // Divide this triangle into three triangles
    public List<Triangle> Split(Point p) {
        if( !PointInside(p) ) {
            Debug.LogError("Trying to split triangle " + this + " using point " + p + ", but " + p + " is outside of it.");
        }
        List<Triangle> tris = new List<Triangle>();
        tris.Add(new Triangle(p, points[0], points[1]));
        tris.Add(new Triangle(p, points[1], points[2]));
        tris.Add(new Triangle(p, points[2], points[0]));
        return tris;
    }

    public override string ToString() {
        return points[0].ToString() + ", " + points[1].ToString() + ", " + points[2].ToString();
    }

    // Get triangle's points in 3D space
    public Vector3[] getVector3s() {
        Vector3[] vecs = new Vector3[points.Length];
        for(int i = 0; i < points.Length; i++) {
            vecs[i] = (Vector3)points[i];
        }
        return vecs;
    }

    // Get the lines that make up this triangle
    public Line[] toLines() {
        return new Line[3]{
            new Line(points[0], points[1]),
            new Line(points[1], points[2]),
            new Line(points[2], points[0])
        };
    }
}
