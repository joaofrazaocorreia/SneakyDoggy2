using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPosition : MonoBehaviour
{

    private GameObject tracker;
    private Material grass;
    // Start is called before the first frame update
    void Start()
    {
        grass = GetComponent<Renderer>().material;
        tracker = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 trackerpos=tracker.GetComponent<Transform>().position;
        grass.SetVector("_TrackPosition", trackerpos);
    }
}
