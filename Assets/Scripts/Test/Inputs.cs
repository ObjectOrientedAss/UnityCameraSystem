using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour
{
    public float RotationSpeed;
    public float MovementSpeed;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(transform.up, -RotationSpeed * Time.deltaTime);
        }
        else if(Input.GetKey(KeyCode.E))
        {
            transform.Rotate(transform.up, RotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * MovementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * MovementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * MovementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * MovementSpeed * Time.deltaTime;
        }
    }
}
