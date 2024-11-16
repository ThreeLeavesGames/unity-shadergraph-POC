using System;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");

    }

    private void OnEnable()
    {
        Debug.Log("Enable");
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}
