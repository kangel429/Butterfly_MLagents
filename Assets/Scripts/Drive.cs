﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour
{
    Animator anim;
    public Rigidbody rb;
    //public Transform target;
    public float speed = 25.0f;

    private void Start()
    {
        //reset anim states   
        anim = this.GetComponent<Animator>();
        anim.SetBool("IsDoubleFlapping", false);
        anim.SetBool("TurnLeft", false);
        anim.SetBool("TurnRight", false);
        anim.SetBool("GoForward", false);
        anim.SetBool("IsSlowFlapping", false);
        anim.SetBool("IsTouched", false);
    }   



}