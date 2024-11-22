using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicRBController : MonoBehaviour
{
    public Rigidbody rigidbody;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            //rigidbody.MovePosition(rigidbody.position + Vector3.up * Time.deltaTime);
            transform.position += Vector3.up * Time.deltaTime;
        }
    }
}
