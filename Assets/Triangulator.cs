using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulator {
    public static List<Triangle> Triangulate(List<Point> points) {
        // Create two triangles to form the square of the entire environment
        Point bl = points[0];
        Point br = points[1];
        Point tl = points[2];
        Point tr = points[3];
        Triangle lowerTri = new Triangle(bl, br, tl);
        Triangle upperTri = new Triangle(tl, br, tr);

        // Kick off the triangulation with those boundary triangles
        List<Triangle> triangulation = new List<Triangle>(){lowerTri, upperTri};

        // Add each point to the triangulation
        foreach(Point p in points.GetRange(4, points.Count-4)) {
            HashSet<Line> lines = new HashSet<Line>();
            // Determine which triangle(s) p is circle-contained by
            for(int tIndex = triangulation.Count-1; tIndex >= 0; tIndex--){
                if( triangulation[tIndex].CircleContains(p) ) {
                    // Save the lines of the containing triangle
                    Line[] newLines = triangulation[tIndex].toLines();
                    foreach(Line l in newLines) {
                        if(!lines.Contains(l)) {
                            lines.Add(l);
                        } else {
                            // Delete duplicates
                            lines.Remove(l);
                        }
                    }
                    // And then forget about it...
                    triangulation.RemoveAt(tIndex);
                }
            }

            // Add the new triangles composed of each line and p
            foreach(Line l in lines) {
                triangulation.Add(new Triangle(p, l.a(), l.b()));
            }
        }

        
        //Debug.Log("DONE ADDING, NOW UPDATING NEIGHBORS");

        foreach(Point p in points) {
            p.ClearNeighbors();
        }
        
        // Update the points to know their neighbors
        foreach(Triangle t in triangulation) {
            t.points[0].AddNeighbor(t.points[1]);
            t.points[1].AddNeighbor(t.points[0]);

            t.points[1].AddNeighbor(t.points[2]);
            t.points[2].AddNeighbor(t.points[1]);

            t.points[2].AddNeighbor(t.points[0]);
            t.points[0].AddNeighbor(t.points[2]);
        }
        
        return triangulation;
    }
}
