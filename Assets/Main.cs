using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    [SerializeField]
    public GameObject player;

    [SerializeField]
    public GameObject TriangleDrawerPrefab;

    [SerializeField]
    public GameObject PointPrefab;

    [SerializeField]
    public GameObject RedPointPrefab;

    [SerializeField]
    public GameObject dot;

    private List<Point> points;
    private List<Triangle> triangles;

    private bool showingTriangulation = false;
    private bool showingVoronoi = false;
    private int roundsUntilRed = 10;
    private Vector2 momentum = new Vector2(0,0);
    private Vector2 bounds;
    private int score = 0;
    private bool paused = false;

    private void Start() {
        score = 0;
        points = new List<Point>();
        triangles = new List<Triangle>();
        
        // Add the corner blobs.
        Vector2 screenBL = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0));
        GameObject bl = Instantiate(PointPrefab, new Vector3(screenBL.x, screenBL.y, 0), Quaternion.identity);
        bl.tag="CornerPointMarker";
        GameObject br = Instantiate(PointPrefab, new Vector3(screenBL.x*-1, screenBL.y, 0), Quaternion.identity);
        br.tag="CornerPointMarker";
        GameObject tl = Instantiate(PointPrefab, new Vector3(screenBL.x, screenBL.y*-1, 0), Quaternion.identity);
        tl.tag="CornerPointMarker";
        GameObject tr = Instantiate(PointPrefab, new Vector3(screenBL.x*-1, screenBL.y*-1, 0), Quaternion.identity);
        tr.tag="CornerPointMarker";

        bounds = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0))*-1;
    }

    void Update() {
        if( paused ) {
            return;
        }

        if( Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("YOU GAVE UP!");
            paused = true;
            Invoke("lose", 2);
        }

        if( Input.GetKeyDown("t") ) {
            showingTriangulation = !showingTriangulation;
        }
        if( Input.GetKeyDown("v") ) {
            showingVoronoi = !showingVoronoi;
        }
        if( Input.GetKeyDown("c") ) {
            ClearAll();
        }

        UpdatePoints();
        UpdateTriangles();

        movePlayer();

        if( showingTriangulation ) {
            ShowTriangulation();
        } else {
            HideTriangulation();
        }

        if( showingVoronoi ) {
            ShowVoronoi();
        } else {
            HideVoronoi();
        }

    }

    private void lose() {
        Debug.Log("Ending Game");
        Menu.score = score;
        ClearAll();
        Menu.LoseGame();
    }

    private void movePlayer() {
        // Get the player point
        Point pp = points[4];

        // Move it
        Vector2 push = new Vector2(0,0);

        if (Input.GetMouseButton(0)) {
            Vector2 mp = Input.mousePosition;
            if( mp.x > 1 && mp.y > 1
             && mp.x < Screen.width - 1
             && mp.y < Screen.height - 1
            ) {
                push = pp.ToVector2() - ((Vector2) Camera.main.ScreenToWorldPoint(mp));
                push = push.normalized * -0.3f * mouseInfluenceMult(push.magnitude);
            }
        }

        push = push + momentum;
        if (!Input.GetMouseButton(1)) {
            List<Point> neighbors = pp.GetNeighbors();
            foreach(Point p in neighbors) {
                if( p.isRed() ) {
                    Debug.Log("YOU LOSE, GOOD DAY SIR!");
                    paused = true;
                    Invoke("lose", 2);
                    return;
                }
                Vector2 diff = pp.ToVector2() - p.ToVector2();
                push = push + (diff.normalized * 0.3f * blobInfluenceMult(diff.magnitude));
            }
            if(push.magnitude > 5.0f) {
                push = push.normalized * 5.0f;
            }
        }

        Vector2 newPos = pp.ToVector2() + push;
        float buf = 0.01f;
        if(newPos.x-buf <= bounds.x * -1) {
            newPos.x = -1 * bounds.x + buf;
            push.x = 0;
        } else if(newPos.x+buf >= bounds.x) {
            newPos.x = bounds.x-buf;
            push.x = 0;
        }
        if(newPos.y-buf <= bounds.y * -1) {
            newPos.y = -1 * bounds.y + buf;
            push.y = 0;
        } else if(newPos.y+buf >= bounds.y) {
            newPos.y = bounds.y - buf;
            push.y = 0;
        }
        pp.move(newPos);
        momentum = 0.8f * push;

        // Check if we ate the dot
        if(pp.CellContains(dot.transform.position)) {
            score = score + 1;
            AddPoint();
            TeleportDot();

        }
    }

    private float mouseInfluenceMult(float distance) {
        //1/(1+e^(0.6*(x-6))) - (1/(1+e^(15(x-0.25))))
        float mult = (float)(
            (1/(1+System.Math.Exp(0.6*(distance-6)))) - (1/(1+System.Math.Exp(15*(distance-0.25))))
        );
        return mult;
    }

    private float blobInfluenceMult(float distance) {
        // 1.2/(1+e^(0.7*(x-3)))
        float mult = (float)(
            (1.2/(1+System.Math.Exp(0.7*(distance-3))))
        );
        return mult;
    }

    private void UpdatePoints() {
        // Clear the point list
        points = new List<Point>();

        // Add the corner blobs
        GameObject[] corners = GameObject.FindGameObjectsWithTag("CornerPointMarker");
        foreach(GameObject blob in corners) {
            Point blobPoint = new Point(blob);
            points.Add(blobPoint);
        }
        
        // Add the player point
        Point playerPoint = new Point(player);
        points.Add(playerPoint);

        // Add the rest of the blobs
        GameObject[] blobs = GameObject.FindGameObjectsWithTag("PointMarker");
        foreach(GameObject blob in blobs) {
            Point blobPoint = new Point(blob);
            points.Add(blobPoint);
        }
        GameObject[] redBlobs = GameObject.FindGameObjectsWithTag("RedPointMarker");
        foreach(GameObject redBlob in redBlobs) {
            Point redBlobPoint = new Point(redBlob);
            points.Add(redBlobPoint);
        }
    }

    private void UpdateTriangles() {
        triangles = Triangulator.Triangulate(points);
    }

    private void ShowTriangulation() {
        HideTriangulation();
        foreach(Triangle tri in triangles) {
            GameObject newTriangleDrawer = Instantiate(TriangleDrawerPrefab);
            LineRenderer lineRenderer = newTriangleDrawer.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 3;
            lineRenderer.SetPositions(tri.getVector3s());
        }
    }

    private void HideTriangulation() {
        GameObject[] trianglesToRemove = GameObject.FindGameObjectsWithTag("TriangleDrawer");
        foreach( GameObject tri in trianglesToRemove ) {
            Destroy(tri);
        }
    }

    private void ShowVoronoi() {
        foreach( Point p in points ) {
            p.DrawVoronoi(bounds);
        }
    }

    private void HideVoronoi() {
        foreach( Point p in points ) {
            p.RemoveVoronoi();
        }
    }
    
    private void AddPoint() {
        Vector3 corner = bounds * -1;
        float x = UnityEngine.Random.Range(-corner.x, corner.x);
        float y = UnityEngine.Random.Range(-corner.y, corner.y);

        if(Math.Sign(player.transform.position.x) == Math.Sign(x)) {
            x = x * -1;
        }
        if(Math.Sign(player.transform.position.y) == Math.Sign(y)) {
            y = y * -1;
        }

        GameObject blob = null;
        if( roundsUntilRed > 0 ) {
            Debug.Log("Adding point at (" + x + "," + y + ").");
            blob = Instantiate(PointPrefab, new Vector3(x,y,0), Quaternion.identity);
            roundsUntilRed--;
        } else {
            Debug.Log("Adding RED point at (" + x + "," + y + ").");
            blob = Instantiate(RedPointPrefab, new Vector3(x,y,0), Quaternion.identity);
            roundsUntilRed = 7;
        }
        Point p = new Point(blob);
        points.Add(p);
    }

    private void TeleportDot() {
        Vector3 corner = bounds * -1;
        float x = UnityEngine.Random.Range(-corner.x, corner.x);
        float y = UnityEngine.Random.Range(-corner.y, corner.y);

        Debug.Log("Teleporting dot to (" + x + "," + y + ").");
        dot.transform.position = new Vector2(x, y);
    }

    private void MoveDot() {
        Vector3 corner = bounds * -1;
        float x = UnityEngine.Random.Range(-corner.x, corner.x);
        float y = UnityEngine.Random.Range(-corner.y, corner.y);

        Debug.Log("Moving dot to (" + x + "," + y + ").");
        dot.transform.position = new Vector2(x, y);
    }


    private void ClearAll() {
        HideTriangulation();
        GameObject[] pointsToRemove = GameObject.FindGameObjectsWithTag("PointMarker");
        foreach( GameObject point in pointsToRemove ) {
            Destroy(point);
        }
        pointsToRemove = GameObject.FindGameObjectsWithTag("RedPointMarker");
        foreach( GameObject point in pointsToRemove ) {
            Destroy(point);
        }
        points = new List<Point>();
        
    }

}
