using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        transform.parent.Find("Hull").GetComponent<Brain>().OnCollisionEnter(collision);
    }
}
