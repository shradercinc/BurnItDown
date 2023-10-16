using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] float movementSpeed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x - movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x + movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z + movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x + movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x - movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z + movementSpeed * Time.deltaTime);
        }
    }
}
