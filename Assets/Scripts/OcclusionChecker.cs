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

    private void Start()
    {
        transparentObstacles = new List<GameObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!transparentObstacles.Contains(collision.gameObject) && collision.gameObject.tag == "Occlusive")
        {
            Debug.Log("has collided with " + collision.gameObject.name);

            transparentObstacles.Add(collision.gameObject);

            foreach(MeshRenderer mr in collision.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(ChangeAlpha(mr.material, occlusionTransparency, collision.gameObject));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(transparentObstacles.Contains(collision.gameObject))
        {
            Debug.Log("no longer colliding with " + collision.gameObject.name);

            transparentObstacles.Remove(collision.gameObject);

            foreach(MeshRenderer mr in collision.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(ChangeAlpha(mr.material, 1f, collision.gameObject));
            }
        }
    }

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