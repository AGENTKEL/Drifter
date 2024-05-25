using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MovementHandler : NetworkBehaviour
{
    CarController carController;

    private void Awake() 
    {
        carController = GetComponent<CarController>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData))
        {
            
        }
    }
}
