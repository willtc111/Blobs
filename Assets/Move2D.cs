using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move2D : MonoBehaviour {

    private Vector3 mp;
    public float moveSpeed = 0.1f; 
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        // if (Input.GetMouseButton(1)) {
        //     mp = Input.mousePosition;
        //     if( mp.x > 1 && mp.y > 1
        //      && mp.x < Screen.width - 1
        //      && mp.y < Screen.height - 1
        //     ) {
        //         mp = Camera.main.ScreenToWorldPoint(mp);
        //         transform.position = Vector2.Lerp(transform.position, mp, moveSpeed);
        //     }
        // }
    }
}
