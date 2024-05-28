using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using Fusion.Addons.Physics;
using UnityEngine.SceneManagement;

public class CarController : NetworkBehaviour
{
    private NetworkRigidbody3D _rb;

    [SerializeField] private Wheel[] _wheels;


    [SerializeField] private int _motorForce;
    [SerializeField] private int _breakForce;
    [SerializeField] private float brakeInput;
    [SerializeField] [Networked] private int wheelAngle {get; set;}
    [SerializeField] private int originalWheelAngle;
    [SerializeField] private int brakelWheelAngle;
    [SerializeField] private float _slipAllowance;

    [SerializeField] private float _speed;
    [SerializeField] Vector2 moveInputVector = Vector2.zero;

    [SerializeField] private AnimationCurve _steeringCurve;

    private bool isSpacePressed;

    [SerializeField] private TextMeshProUGUI driftPointsText; // UI Text to display drift points
    [SerializeField] private TextMeshProUGUI totalPointsText; // UI Text to display total points
    [SerializeField] private TextMeshProUGUI driftPointMoneyText;
    [SerializeField] private GameObject resultsMenu;

    private float currentDriftPoints = 0;
    private float totalPoints = 0;
    private float driftStartTime = 0f;
    private bool isDrifting = false;



    private void Start() 
    {


    }
    
    private void Update() 
    {


        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSpacePressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isSpacePressed = false;
        }


    }

    public override void Spawned()
    {
        _rb = GetComponent<NetworkRigidbody3D>();

        for (int i = 0; i < _wheels.Length; i++)
        {
            _wheels[i].originalStiffness = _wheels[i].wheelCollider.forwardFriction.stiffness;
            _wheels[i].originalExtremumSlip = _wheels[i].wheelCollider.forwardFriction.extremumSlip;
        }
        
        driftPointsText.text = "";
        totalPointsText.text = $"Score: {totalPoints:F1}";
    }

    public override void FixedUpdateNetwork()
    {

        if (GetInput(out NetworkInputData networkInputData))
        {
            moveInputVector.x = Input.GetAxis("Horizontal");
            if (!isSpacePressed)
                moveInputVector.y = Input.GetAxis("Vertical");

            //Acceleration
            _speed = _rb.Rigidbody.velocity.magnitude;
            foreach (Wheel wheel in _wheels)
            {
                wheel.wheelCollider.motorTorque = _motorForce * networkInputData.movementInput.y;
                wheel.UpdateMeshPosition();
            }

            if (networkInputData.movementInput.y == 0 && _speed > 0 && !networkInputData.isSpacePressed)
            {
                networkInputData.movementInput.y = -0.01f;
            }

            //Steering
            float steeringAngle = networkInputData.movementInput.x * _steeringCurve.Evaluate(_speed);
            float slipAngle = Vector3.Angle(transform.forward, _rb.Rigidbody.velocity - transform.forward);

            if (slipAngle < 120)
                steeringAngle += Vector3.SignedAngle(transform.forward, _rb.Rigidbody.velocity, Vector3.up) / 2;

            steeringAngle = Mathf.Clamp(steeringAngle, -wheelAngle, wheelAngle);    

            foreach (Wheel wheel in _wheels)
            {
                if (wheel.isForwardWheels)
                    wheel.wheelCollider.steerAngle = steeringAngle;
            }


            //Braking
            foreach (Wheel wheel in _wheels)
            wheel.wheelCollider.brakeTorque = brakeInput * _breakForce * (wheel.isForwardWheels ? 0.7f : 0.3f);

            float movingDirectional = Vector3.Dot(transform.forward, _rb.Rigidbody.velocity);

            brakeInput = (movingDirectional < -0.5f && networkInputData.movementInput.y > 0) || (movingDirectional > 0.5f && networkInputData.movementInput.y < 0) ? Mathf.Abs(networkInputData.movementInput.y) : 0;

            if (networkInputData.isSpacePressed)
            {
                foreach (Wheel wheel in _wheels)
                {
                    // Get the current forwardFriction
                    WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;

                    // Modify the stiffness and extremumSlip
                    forwardFriction.stiffness = wheel.brakeStiffness;
                    forwardFriction.extremumSlip = wheel.breakExtremumSlip; // Example value, change as needed
                    wheelAngle = brakelWheelAngle;

                    // Reassign the modified friction curve back to the wheel collider
                    wheel.wheelCollider.forwardFriction = forwardFriction;
                }
                moveInputVector.y = 0f;
                networkInputData.movementInput.y = -0.02f;

            }
            else
            {
                foreach (Wheel wheel in _wheels)
                {
                    // Get the current forwardFriction
                    WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;

                    // Revert the stiffness and extremumSlip to the original values
                    forwardFriction.stiffness = wheel.originalStiffness;
                    forwardFriction.extremumSlip = wheel.originalExtremumSlip;
                    wheelAngle = originalWheelAngle;
                    // Reassign the modified friction curve back to the wheel collider
                    wheel.wheelCollider.forwardFriction = forwardFriction;
                }
            }

            CheckParticles();
        }
    }

    public void ConvertDriftPointsToMoney()
    {
        resultsMenu.SetActive(true);
        int money = Mathf.FloorToInt(totalPoints / 10);
        GameManager.instance._money += money;
        driftPointMoneyText.text = $"Drift points ({totalPoints}) = Money ({money})";
    }

    public void ExitToMainMenu()
    {
        if (Object.HasInputAuthority)
        {
            Runner.Despawn(Object);
            SceneManager.LoadScene("ExampleScene");
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        networkInputData.movementInput = moveInputVector;
        networkInputData.isSpacePressed = isSpacePressed;

        return networkInputData;
    }

    private void CheckParticles()
    {
        bool wasDrifting = isDrifting;
        isDrifting = false;

        foreach (Wheel wheel in _wheels)
        {
            WheelHit wheelHit;
            wheel.wheelCollider.GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip) > _slipAllowance)
            {
                if (!wheel.wheelSmoke.isPlaying)
                    wheel.wheelSmoke.Play();

                isDrifting = true;
            }
            else
            {
                wheel.wheelSmoke.Stop();
            }
        }


        if (isDrifting && _speed > 5)
        {
            if (!wasDrifting)
            {
                driftStartTime = Time.time;
            }

            float driftDuration = Time.time - driftStartTime;
            float pointsMultiplier = 1f;

            if (_speed > 5 && _speed <= 20)
            {
                pointsMultiplier = 1f;
            }
            else if (_speed > 20 && _speed <= 40)
            {
                pointsMultiplier = 1.5f;
            }
            else if (_speed > 40 && _speed <= 60)
            {
                pointsMultiplier = 2f;
            }
            else if (_speed > 60 && _speed <= 80)
            {
                pointsMultiplier = 2.5f;
            }

            if (driftDuration < 1f)
            {
                currentDriftPoints += 100 * pointsMultiplier * Time.deltaTime;
            }
            else if (driftDuration < 2f)
            {
                currentDriftPoints += 250 * pointsMultiplier * Time.deltaTime;
            }
            else
            {
                currentDriftPoints += 500 * pointsMultiplier * Time.deltaTime;
            }

            driftPointsText.text = $"{Mathf.FloorToInt(currentDriftPoints)}";
        }
        else if (wasDrifting && !isDrifting)
        {
            // Add the drift points to the total points when the drift ends
            totalPoints += Mathf.FloorToInt(currentDriftPoints);
            totalPointsText.text = $"Score: {totalPoints}";
            currentDriftPoints = 0;
            driftPointsText.text = "";
        }
    }
}

[System.Serializable]

public struct Wheel
{
    public Transform wheelMesh;
    public WheelCollider wheelCollider;
    public ParticleSystem wheelSmoke;
    public bool isForwardWheels;

    public float originalStiffness;
    public float originalExtremumSlip;

    public float brakeStiffness;
    public float breakExtremumSlip;

    public void UpdateMeshPosition()
    {
        Vector3 position;
        Quaternion rotation;
        
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelMesh.position = position;
        wheelMesh.rotation = rotation;
    }
}
