using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class matchMaker : MonoBehaviour
{

public List<GameObject> PlacedObject = new List<GameObject>();
public GameObject PointA;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (PlacedObject.Count == 0)
        {
           other.gameObject.transform.position = PointA.transform.position;
           other.gameObject.transform.rotation = PointA.transform.rotation;
           PlacedObject.Add(other.gameObject);
        }else if (other.gameObject.transform.name.Contains(PlacedObject[0].transform.name)== true)
        {
           Debug.Log("Matched");
           Destroy(other.gameObject);
           Destroy(PlacedObject[0].gameObject);
           PlacedObject.Clear();
        }
        else
        {
            other.GetComponent<Rigidbody>().velocity = new Vector3(0,1,1)*120 * Time.deltaTime;
        }
       
    }

    private void OnTriggerExit(Collider other)
    {
if (PlacedObject.Contains(other.gameObject))
{
    PlacedObject.Remove(other.gameObject);
}
    }
}
