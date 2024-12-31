using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OcclusionChecker : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float occlusionTransparency = 0.4f;
    [SerializeField] private int occlusionSpeed = 2;

    private List<GameObject> transparentObstacles;

    // Initializes a list of every object that can be occluded
    private void Start()
    {
        transparentObstacles = new List<GameObject>();
    }

    // When the occlusion checker finds an obstacle between the player and the camera
    private void OnCollisionEnter(Collision collision)
    {
        // Checks if that obstacle isn't already transparent and is one of the objects that can be occluded
        if(!transparentObstacles.Contains(collision.gameObject) && collision.gameObject.tag == "Occlusive")
        {
            // Registers the obstacle as occluded, then turns it transparent
            transparentObstacles.Add(collision.gameObject);

            foreach(MeshRenderer mr in collision.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(ChangeAlpha(mr.material, occlusionTransparency, collision.gameObject));
            }
        }
    }

    // When the occlusion checker no longer finds an obstacle between the player and the camera
    private void OnCollisionExit(Collision collision)
    {
        // Checks if that obstacle had been turned transparent
        if(transparentObstacles.Contains(collision.gameObject))
        {
            // Unregisters the obstacle as occluded, then returns the transparency to normal
            transparentObstacles.Remove(collision.gameObject);

            foreach(MeshRenderer mr in collision.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(ChangeAlpha(mr.material, 1f, collision.gameObject));
            }
        }
    }

    // Changes the transparency over multiple frames to make it a smooth visual to the human eye
    private IEnumerator ChangeAlpha(Material material, float alpha, GameObject collisionGameObject)
    {
        while(material.color.a != alpha)
        {
            float newAlpha = material.color.a;

            if(material.color.a > alpha)
            {
                newAlpha -= 0.01f * occlusionSpeed;

                if(!transparentObstacles.Contains(collisionGameObject))
                    break;
            }

            if(material.color.a < alpha)
            {
                newAlpha += 0.01f * occlusionSpeed;

                if(transparentObstacles.Contains(collisionGameObject))
                    break;
            }


            if(Mathf.Abs(alpha - newAlpha) < 0.01f * occlusionSpeed)
                newAlpha = alpha;


            material.color = new Color(material.color.r, material.color.g, material.color.b, newAlpha);

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}