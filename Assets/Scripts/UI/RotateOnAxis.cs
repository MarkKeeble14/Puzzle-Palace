using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnAxis : MonoBehaviour
{
    [SerializeField] private Transform toRotate;
    [SerializeField] private Axis[] rotateAlongAxis;
    [SerializeField] private float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        Vector3 newEuler = toRotate.localEulerAngles;
        foreach (Axis axis in rotateAlongAxis)
        {
            switch (axis)
            {
                case Axis.X:
                    newEuler.x += rotationSpeed * Time.deltaTime;
                    break;
                case Axis.Y:
                    newEuler.y += rotationSpeed * Time.deltaTime;
                    break;
                case Axis.Z:
                    newEuler.z += rotationSpeed * Time.deltaTime;
                    break;
            }
        }
        toRotate.localEulerAngles = newEuler;
    }
}
