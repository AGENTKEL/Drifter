using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public NetworkBool isLeftPressed;
    public NetworkBool isRightPressed;
    public NetworkBool isForwardPressed;
    public NetworkBool isBackPressed;

    public NetworkBool isSpacePressed;
}
