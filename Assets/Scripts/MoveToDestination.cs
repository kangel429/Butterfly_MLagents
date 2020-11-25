using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class MoveToDestination : MonoBehaviour
{

    public Animator butterflyAnim;
    //Vector3 targetPosition = new Vector3(0, 0, 0);
    public float speed = 1.0f;
    public Vector3 random1;
    public Vector3 initPosition;
    public GameObject ceterPosition;
    void Start()
    {
        StartCoroutine("MoveTo");
        initPosition = ceterPosition.transform.position;
        random1 = new Vector3(Random.Range(-10 + initPosition.x, 10 + initPosition.x),
            Random.Range(0, 5), Random.Range(-10 + initPosition.z, 10 + initPosition.z));
    }


    void FixedUpdate()
    {

        //float distanceToOrigin = ( transform.position - destinationObject.position).sqrMagnitude;

        //if (Input.GetKey(KeyCode.F))
        //{
        transform.position = Vector3.MoveTowards(transform.position, random1, speed * Time.deltaTime);
        transform.LookAt(random1);
        butterflyAnim.SetBool("IsDoubleFlapping", true);
        butterflyAnim.SetBool("TurnLeft", false);
        butterflyAnim.SetBool("TurnRight", false);
        butterflyAnim.SetBool("GoForward", false);
        butterflyAnim.SetBool("IsSlowFlapping", false);
        butterflyAnim.SetBool("IsTouched", false);
        butterflyAnim.SetBool("IsReturning", true);
        //}

        //if (distanceToOrigin < 0.01f)
        //{
        //    Vector3 eulerAngles = transform.eulerAngles;
        //    transform.eulerAngles = new Vector3(0, eulerAngles.y, 0);

        //    butterflyAnim.SetBool("IsDoubleFlapping", false);
        //    butterflyAnim.SetBool("TurnLeft", false);
        //    butterflyAnim.SetBool("TurnRight", false);
        //    butterflyAnim.SetBool("GoForward", false);
        //    butterflyAnim.SetBool("IsSlowFlapping", false);
        //    butterflyAnim.SetBool("IsTouched", true);
        //    butterflyAnim.SetBool("IsReturning", true);
        //}

    }


    IEnumerator MoveTo( )
    {
        random1 = new Vector3(Random.Range(-10+ initPosition.x, 10+ initPosition.x),
            Random.Range(0, 5), Random.Range(-10+ initPosition.z, 10+ initPosition.z));

        yield return new WaitForSeconds(1f);
    }


}
