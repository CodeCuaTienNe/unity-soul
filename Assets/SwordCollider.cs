using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollisionDetector : MonoBehaviour
{
    // This function is called when the sword trigger collider enters another collider
    private void OnTriggerEnter(Collider other)
    {
        // Log what was hit
        Debug.Log("Sword hit: " + other.gameObject.name);

        // You can add more code here if you want to do something when hit occurs
    }
}