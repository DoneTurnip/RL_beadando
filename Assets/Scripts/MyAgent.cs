using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class MyAgent : Agent
{
    public float speed = 1;
    private enum ACTION
    {
        LEFT = 0,
        FORWARD = 1,
        RIGHT = 2,
        BACKWARD = 3
    }

    public Transform targetTransform;
    public List<Vector3> insideWallPositionsList;

    public void ListFill() 
    {
        insideWallPositionsList.Add(new Vector3(5f, 1f, -3f));
        insideWallPositionsList.Add(new Vector3(-5f, 1f, -3f));
        insideWallPositionsList.Add(new Vector3(5f, 1f, 3f));
        insideWallPositionsList.Add(new Vector3(-5f, 1f, 3f));
        insideWallPositionsList.Add(new Vector3(-6f, 1f, 0f));
        insideWallPositionsList.Add(new Vector3(6f, 1f, 0f));
        insideWallPositionsList.Add(new Vector3(7f, 1f, 7f));
        insideWallPositionsList.Add(new Vector3(-7f, 1f, 7f));
        insideWallPositionsList.Add(new Vector3(7f, 1f, -7f));
        insideWallPositionsList.Add(new Vector3(-7f, 1f, -7f));
    }

    public void StaffSpawn() 
    {
        int z = 0;
        int x = 0;
        do
        {
            z = Random.Range(-5, 13);
            x = Random.Range(-10, 7);
        } while ((z == -3 || z == 3) && (x == -5 || x == 5)
                || (z == 0 && (x == -6 || x == 6))
                || ((z == 7 || z == -7) && (x == 7 || x == -7)));

        targetTransform.localPosition = new Vector3(x, -2.3f, z);
    }
    
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, 1f, 0);
        
        ListFill();
        StaffSpawn();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);

        sensor.AddObservation(targetTransform.localPosition.x);
        sensor.AddObservation(targetTransform.localPosition.z);

        float distance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        sensor.AddObservation(distance);

        for (int i = 0; i < 6; i++)
        {
            sensor.AddObservation(insideWallPositionsList[i].x);
            sensor.AddObservation(insideWallPositionsList[i].z);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.DiscreteActions[0];

        switch (actionTaken)
        {
            case (int)ACTION.FORWARD:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case (int)ACTION.BACKWARD:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;

            case (int)ACTION.LEFT:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;

            case (int)ACTION.RIGHT:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
        }

        transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);

        AddReward(-0.01f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;

        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        action[0] = (int)ACTION.FORWARD;

        if (horizontal == -1)
        {
            action[0] = (int)ACTION.LEFT;
        }
        else if (horizontal == 1)
        {
            action[0] = (int)ACTION.RIGHT;
        }

        if (vertical == -1)
        {
            action[0] = (int)ACTION.BACKWARD;
        }
        else if (vertical == 1)
        {
            action[0] = (int)ACTION.FORWARD;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.collider.tag == "Wall")
        {
            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.collider.tag == "Staff")
        {
            AddReward(+1);
            EndEpisode();
        }
    }
}