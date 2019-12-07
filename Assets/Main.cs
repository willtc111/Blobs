using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    // The player's blob object
    [SerializeField]
    public GameObject player;

    // The objects that will be used to render triangles
    [SerializeField]
    public GameObject TriangleDrawerPrefab;

    // The objects that will be used to render green blobs
    [SerializeField]
    public GameObject PointPrefab;

    // The objects that will be used to render red blobs
    [SerializeField]
    public GameObject RedPointPrefab;

    // The dot to eat
    [SerializeField]
    public GameObject dot;

    // The current point (blob) list
    private List<Point> points;
    // The current triangulation
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

        // Handle user inputs
        if( Input.GetKeyDown(KeyCode.R)) {
            paused = true;
            Invoke("lose", 2);
        }
        if( Input.GetKeyDown("t") ) {
            showingTriangulation = !showingTriangulation;
        }
        if( Input.GetKeyDown("v") ) {
            showingVoronoi = !showingVoronoi;
        }
        // if( Input.GetKeyDown("c") ) {
        //     ClearAll();
        // }

        // Do the frame update stuff
        UpdatePoints();
        UpdateTriangles();
        movePlayer();

        // Draw the "educational" mode stuff
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

    // End the game
    private void lose() {
        //Debug.Log("Ending Game");
        Menu.score = score;
        ClearAll();
        Menu.LoseGame();
    }

    // Handle moving the player
    private void movePlayer() {
        // Get the player point
        Point pp = points[4];

        // Move it
        Vector2 push = new Vector2(0,0);

        // Add mouse influence
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
        
        // Carry over momentum
        push = push + momentum;

        // If not cheating, handle influence of blobs
        if (!Input.GetMouseButton(1)) {
            // For every neighbor add their push
            List<Point> neighbors = pp.GetNeighbors();
            foreach(Point p in neighbors) {
                if( p.isRed() ) {
                    //Debug.Log("YOU LOSE, GOOD DAY SIR!");
                    paused = true;
                    Invoke("lose", 2);
                    return;
                }
                Vector2 diff = pp.ToVector2() - p.ToVector2();
                push = push + (diff.normalized * 0.3f * blobInfluenceMult(diff.magnitude));
            }
            // Limit the movement speed
            if(push.magnitude > 5.0f) {
                push = push.normalized * 5.0f;
            }
        }

        // Take the new force vector and apply it to the current player position
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

    // Calculate the mouse influence
    private float mouseInfluenceMult(float distance) {
        //1/(1+e^(0.6*(x-6))) - (1/(1+e^(15(x-0.25))))
        float mult = (float)(
            (1/(1+System.Math.Exp(0.6*(distance-6)))) - (1/(1+System.Math.Exp(15*(distance-0.25))))
        );
        return mult;
    }
    // Calculate the blob influence
    private float blobInfluenceMult(float distance) {
        // 1.2/(1+e^(0.7*(x-3)))
        float mult = (float)(
            (1.2/(1+System.Math.Exp(0.7*(distance-3))))
        );
        return mult;
    }

    // Regenerate the point list based on the current game objects
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

    // Re-trianguate
    private void UpdateTriangles() {
        triangles = Triangulator.Triangulate(points);
    }

    // Draw the triangles
    private void ShowTriangulation() {
        // Destroy the old triangles first
        HideTriangulation();

        foreach(Triangle tri in triangles) {
            GameObject newTriangleDrawer = Instantiate(TriangleDrawerPrefab);
            LineRenderer lineRenderer = newTriangleDrawer.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 3;
            lineRenderer.SetPositions(tri.getVector3s());
        }
    }

    // Destroy the triangulation
    private void HideTriangulation() {
        GameObject[] trianglesToRemove = GameObject.FindGameObjectsWithTag("TriangleDrawer");
        foreach( GameObject tri in trianglesToRemove ) {
            Destroy(tri);
        }
    }

    // Draw the voronoi edges
    private void ShowVoronoi() {
        foreach( Point p in points ) {
            p.DrawVoronoi(bounds);
        }
    }

    // Hide the voronoi edges
    private void HideVoronoi() {
        foreach( Point p in points ) {
            p.RemoveVoronoi();
        }
    }
    
    // Add a new blob to the world
    private void AddPoint() {
        Vector3 corner = bounds * -1;
        float x = UnityEngine.Random.Range(-corner.x, corner.x);
        float y = UnityEngine.Random.Range(-corner.y, corner.y);

        // Make sure to place it in the opposite quadrant as the player
        if(Math.Sign(player.transform.position.x) == Math.Sign(x)) {
            x = x * -1;
        }
        if(Math.Sign(player.transform.position.y) == Math.Sign(y)) {
            y = y * -1;
        }

        // Decide whether to add a red or green blob
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

    // Move the dot
    private void TeleportDot() {
        Vector3 corner = bounds * -1;
        float x = UnityEngine.Random.Range(-corner.x, corner.x);
        float y = UnityEngine.Random.Range(-corner.y, corner.y);

        Debug.Log("Teleporting dot to (" + x + "," + y + ").");
        dot.transform.position = new Vector2(x, y);
    }

    // Remove all blobs except the player and corners
    private void ClearAll() {
        HideTriangulation();
        // Get all the green blobs and destroy them
        GameObject[] pointsToRemove = GameObject.FindGameObjectsWithTag("PointMarker");
        foreach( GameObject point in pointsToRemove ) {
            Destroy(point);
        }
        // Get all the red blobs and destroy them
        pointsToRemove = GameObject.FindGameObjectsWithTag("RedPointMarker");
        foreach( GameObject point in pointsToRemove ) {
            Destroy(point);
        }
        points = new List<Point>();
    }

}
