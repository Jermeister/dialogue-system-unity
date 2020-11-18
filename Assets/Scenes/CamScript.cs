using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
            cam = GameObject.FindWithTag("MainCamera");

        if (cam == null)
            return;
    }

    // Update is called once per frame
    void Update()
    {
        float camY = cam.transform.position.y;
        cam.transform.position = new Vector3(transform.position.x + 10, transform.position.y + 10, transform.position.z - 10);
    }
}
