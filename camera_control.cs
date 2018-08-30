using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class camera_control : MonoBehaviour
{
    public float mouse_x, mouse_y, mouse_scroll;
    // Use this for initialization

    void Start()
    {
        mouse_x = 0;
        mouse_y = 0;
        mouse_scroll = 0;
    }
    // Update is called once per frame
    void Update()
    {
        //---------------------滾輪-----------------
        mouse_scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouse_scroll != 0)
            transform.Translate(0, 0, mouse_scroll * Time.deltaTime * 50f, Space.Self);
        if (Input.GetMouseButton(2))
        {
            mouse_x = Input.GetAxis("Mouse X");
            mouse_y = Input.GetAxis("Mouse Y");
            transform.Translate(-mouse_x * Time.deltaTime * 10, -mouse_y * Time.deltaTime * 10, 0, Space.Self);
        }
        if (Input.GetMouseButton(1))
        {
            mouse_x = Input.GetAxis("Mouse X");
            mouse_y = Input.GetAxis("Mouse Y");
            transform.RotateAround(transform.position, transform.up, mouse_x * Time.deltaTime * 100);
            transform.RotateAround(transform.position, transform.right, mouse_y * Time.deltaTime * 100);
        }
    }
}
