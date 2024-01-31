using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public GameObject hmd;
    private float startPosY;
    private OVRCameraRig cameraRig;
    private bool heightInitialized = false;
    private Rigidbody frogRb;
    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;
    private GameObject parent;

    [SerializeField] private Line energyLine;
    private float totalEnergy = 0f;
    private float minDistanceDown = 0f;
    public bool isAllowedToJump = true;
    private string lastStage = "";
    void Start()
    {
        cameraRig = hmd.GetComponent<OVRCameraRig>();
        // Set initial head position on start
        cameraRig.UpdatedAnchors += OnUpdatedAnchors;
        parent = this.transform.parent.gameObject;
        frogRb = parent.GetComponent<Rigidbody>();
    }

    void OnUpdatedAnchors(OVRCameraRig rig)
    {
        // Set head position
        startPosY = cameraRig.centerEyeAnchor.localPosition.y;

        // Remove listener
        if(startPosY != 0){
            heightInitialized = true;
            cameraRig.UpdatedAnchors -= OnUpdatedAnchors;
        }
    }
    
    void Update()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Please implement your LOCOMOTION TECHNIQUE in this script :D.
        if(heightInitialized && isAllowedToJump)
        {
            float deltaYDown = cameraRig.centerEyeAnchor.localPosition.y - startPosY;
            if(deltaYDown < -0.1f)
            {
                totalEnergy = Math.Max(totalEnergy, -deltaYDown);
                // Set threshold for totalEnergy
                totalEnergy = Math.Min(totalEnergy, 0.5f);
                energyLine.End = new Vector3(totalEnergy*2*5, 0, 0);

                minDistanceDown = Math.Min(minDistanceDown, deltaYDown);
            }

            // If player moves up more than 0.1m, use force to jump and release the energy
            if(Math.Abs(minDistanceDown - deltaYDown) > 0.1f)
            {
                
                Vector3 jumpDirection = new Vector3(cameraRig.centerEyeAnchor.forward.x, 1, cameraRig.centerEyeAnchor.forward.z);
                frogRb.AddForce(5 * totalEnergy * jumpDirection, ForceMode.Impulse);

                // Reset for next jump
                totalEnergy = 0f;
                minDistanceDown = 0f;
                energyLine.End = new Vector3(0, 0, 0);
            }
            
            // Keyboard input for testing. Use force to jump the player at 45 degree angle in the direction he is facing.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 jumpDirection = new Vector3(cameraRig.centerEyeAnchor.forward.x, 1, cameraRig.centerEyeAnchor.forward.z);
                frogRb.AddForce(10 * 0.3f * jumpDirection, ForceMode.Impulse);
            }

            parent.transform.rotation = Quaternion.Euler(0, cameraRig.centerEyeAnchor.rotation.eulerAngles.y, 0);
            // If magnitude of velocity is less than 0.1, move hmd to frog position
            if (frogRb.velocity.magnitude < 0.1)
            {
                Vector3 frogPos = parent.transform.position;
                Vector3 hmdPos = hmd.transform.position;
    
                // The camera should always be behind the frog. Calculated based on the direction the frog is facing
                Vector3 moveToPos = new Vector3(frogPos.x - (cameraRig.centerEyeAnchor.forward.x * 3f), hmdPos.y, frogPos.z - (cameraRig.centerEyeAnchor.forward.z * 3f));

                // Use lerp to move hmd to frog position
                hmd.transform.position = Vector3.Lerp(hmdPos, moveToPos, 0.05f);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////
        // These are for the game mechanism.
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (parkourCounter.parkourStart)
            {
                this.transform.position = parkourCounter.currentRespawnPos;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            if (lastStage == stage)
            {
                // Make sure the task is not triggered twice.
                return;
            }
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            // rotation: facing the user's entering direction
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new Vector3(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            
            isAllowedToJump = false;
            selectionTaskMeasure.StartOneTask();
            lastStage = stage;
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            this.GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
        // These are for the game mechanism.
    }
}