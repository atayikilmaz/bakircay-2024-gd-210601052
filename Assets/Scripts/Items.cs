using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CubeDragHandler : MonoBehaviour
{
    Rigidbody rb;
    Vector3 ScreenPoint;
    Vector3 Offset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
    }

    void OnMouseDown()
    {
        rb.useGravity = false;
        ScreenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ScreenPoint.z));
    }

    private void OnMouseDrag() {
        Vector3 cursorPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, ScreenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + Offset;
        rb.position = cursorPosition;
        rb. MovePosition(new Vector3(rb.position.x, 0.5f, rb.position.z));
    }

    private void OnMouseUp() {
        rb.useGravity = true;
    }
}
