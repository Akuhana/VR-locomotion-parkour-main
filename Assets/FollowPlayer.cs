using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    private Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {

        // Get gameobject with tag "CameraRig"
        playerTransform = GameObject.FindGameObjectWithTag("CameraRig").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f + Camera.main.transform.right * 0.2f;
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0, 180, 0);
    }
}
