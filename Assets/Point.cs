using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point : IEquatable<Point>{

    private Vector2 pos;

    private GameObject blob;

    private HashSet<Point> neighbors = new HashSet<Point>();

    public Point(GameObject obj):this(obj.GetComponent<Transform>().position){
        blob = obj;
    }
    public Point(float x, float y) {
        pos = new Vector2(x, y);
    }
    public Point(Vector2 p) {
        // clone the vector
        pos = new Vector2(p[0], p[1]);
    }
    public Point(Vector3 p) {
        // clone the vector
        pos = new Vector2(p[0], p[1]);
    }

    public bool isRed() {
        return blob.tag == "RedPointMarker";
    }

    public float distance(Point other) {
        return Vector2.Distance(this, other);
    }

    public void move(Vector3 position) {
        pos = (Vector2) position;
        blob.transform.position = pos;
    }

    public float x() {
        return pos[0];
    }

    public float y() {
        return pos[1];
    }
    
    public List<Point> GetNeighbors() {
        return new List<Point>(neighbors);
    }

    public void ClearNeighbors() {
        neighbors = new HashSet<Point>();
    }

    public void AddNeighbor(Point p) {
        neighbors.Add(p);
    }

    // Draw the voronoi edges for this point
    public void DrawVoronoi(Vector2 bounds) {
        if( blob.tag.Equals("CornerPointMarker") | Math.Abs(pos[0]) > bounds[0] | Math.Abs(pos[1]) > bounds[1] ) {
            // Don't draw the voronoi borders for the four corner blobs or the player if they go out of bounds
            RemoveVoronoi();
            return;
        }

        // Sort neighbors by angle
        List<Point> neighborList = GetNeighbors();
        neighborList.Sort(new PointComparator(this));
        Point[] ns = neighborList.ToArray();    // Sorted neighbor points
        
        Vector3[] vs = new Vector3[neighbors.Count];    // Voronoi points
        for(int i = 0; i < neighbors.Count; i++) {
            // j = next neighbor index
            int j = (i + 1) % neighbors.Count;
            Triangle t = new Triangle(this, ns[i], ns[j]);
            // Triangle circumcenter == perpendicular bisectors intersection
            float[] circle = t.Circle();
            vs[i] = new Vector3(circle[0], circle[1], 0);
        }

        // Draw the cell border
        LineRenderer line = blob.GetComponent<LineRenderer>();
        line.positionCount = neighbors.Count;
        line.loop = true;
        line.SetPositions(vs);

    }

    // Does the voronoi cell contain the given point?
    public bool CellContains(Vector2 point) {
        // Just compare distance to neighbors
        foreach(Point neighbor in neighbors) {
            float selfDist = (ToVector2()-point).magnitude;
            float neighDist = (neighbor.ToVector2()-point).magnitude;
            // If distance from point to neighbor is less than distance from point to self
            if(neighDist < selfDist) {
                return false;
            }
        }
        return neighbors.Count > 0;
    }

    public void RemoveVoronoi() {
        blob.GetComponent<LineRenderer>().positionCount = 0;
    }

    public static implicit operator Vector3(Point p) {
        return p.ToVector3();
    }

    public static implicit operator Vector2(Point p) {
        return p.ToVector2();
    }
    
    public Vector3 ToVector3() {
        return new Vector3(pos[0],pos[1],0);
    }

    public Vector2 ToVector2() {
        return new Vector2(pos[0],pos[1]);
    }

    public override string ToString() {
        return pos.ToString();
    }

    public override bool Equals(object other) {
        return other is Point && Equals((Point) other);
    }

    public bool Equals(Point other) {
        return x() == other.x() && y() == other.y();
    }

    public override int GetHashCode() {
        return pos.GetHashCode();
    }
}
