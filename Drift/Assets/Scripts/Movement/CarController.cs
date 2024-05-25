using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CarController : NetworkBehaviour
{
    private Rigidbody _rb;

    [SerializeField] private Wheel[] _wheels;


    [SerializeField] private int _motorForce;
    [SerializeField] private int _breakForce;
    [SerializeField] private float brakeInput;
    [SerializeField] [Networked] private int wheelAngle {get; set;}

    [SerializeField] private float _speed;
    [SerializeField] Vector2 moveInputVector = Vector2.zero;

    [SerializeField] private AnimationCurve _steeringCurve;


    private void Start() 
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update() 
    {
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");


    }

    public override void FixedUpdateNetwork()
    {

        if (GetInput(out NetworkInputData networkInputData))
        {
            //Acceleration
            _speed = _rb.velocity.magnitude;
            foreach (Wheel wheel in _wheels)
            {
                wheel.wheelCollider.motorTorque = _motorForce * networkInputData.movementInput.y;
                wheel.UpdateMeshPosition();
            }


            //Steering
            float steeringAngle = networkInputData.movementInput.x * _steeringCurve.Evaluate(_speed);
            float slipAngle = Vector3.Angle(transform.forward, _rb.velocity - transform.forward);

            if (slipAngle < 120)
                steeringAngle += Vector3.SignedAngle(transform.forward, _rb.velocity, Vector3.up);

            steeringAngle = Mathf.Clamp(steeringAngle, -wheelAngle, wheelAngle);    

            foreach (Wheel wheel in _wheels)
            {
                if (wheel.isForwardWheels)
                    wheel.wheelCollider.steerAngle = steeringAngle;
            }


            //Braking
            foreach (Wheel wheel in _wheels)
            wheel.wheelCollider.brakeTorque = brakeInput * _breakForce * (wheel.isForwardWheels ? 0.7f : 0.3f);

            float movingDirectional = Vector3.Dot(transform.forward, _rb.velocity);

            brakeInput = (movingDirectional < -0.5f && networkInputData.movementInput.y > 0) || (movingDirectional > 0.5f && networkInputData.movementInput.y < 0) ? Mathf.Abs(networkInputData.movementInput.y) : 0;
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        networkInputData.movementInput = moveInputVector;

        return networkInputData;
    }
}

[System.Serializable]

public struct Wheel
{
    public Transform wheelMesh;
    public WheelCollider wheelCollider;
    public bool isForwardWheels;

    public void UpdateMeshPosition()
    {
        Vector3 position;
        Quaternion rotation;
        
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelMesh.position = position;
        wheelMesh.rotation = rotation;
    }
}
