using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using TMPro;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {get; set;}
    public GameObject cameraCinemaChine;
    public Canvas playerCanvas;
    public Canvas playerWorldSpaceCanvas;
    public TextMeshProUGUI playerNickName;
    public TextMeshProUGUI driftPointMoneyText;
    [Networked]
    public NetworkString<_16> _nickName { get; set; }
    [Networked]
    public int _carIndex { get; set; }
    [Networked]
    public int _colorIndex { get; set; }

    [SerializeField] private GameObject camaroModel;
    [SerializeField] private GameObject mustangModel;
    [SerializeField] private MeshRenderer camaroRenderer;
    [SerializeField] private MeshRenderer mustangRenderer;
    ChangeDetector _changeDetector;




    [Header("Car Materials")]
    [SerializeField] private List<Material> blueMaterials;
    [SerializeField] private List<Material> greenMaterials;
    [SerializeField] private List<Material> redMaterials;
    [SerializeField] private List<Material> yellowMaterials;
    private Dictionary<int, List<Material>> _colorMaterials;
    

    // Start is called before the first frame update
    void Start()
    {

    }
    

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);


        if (Object.HasInputAuthority)
        {
            Local = this;
            
            // Initialize color materials dictionary
            _colorMaterials = new Dictionary<int, List<Material>>()
            {
                { 0, blueMaterials },
                { 1, greenMaterials },
                { 2, redMaterials },
                { 3, yellowMaterials }
            };

            Utils.SetRenderLayerInChildren(playerWorldSpaceCanvas.transform, LayerMask.NameToLayer("LocalPlayerModel"));

            Camera.main.gameObject.SetActive(false);

            TriggerRPC();
            Debug.Log("Spawned local player");
        }
        else
        {
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;
            cameraCinemaChine.SetActive(false);
            playerCanvas.gameObject.SetActive(false);

            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            Debug.Log("Spawned remote player");
        }

        transform.name = $"P_{Object.Id}";
    }


    public override void Render()
    {
        

        foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(_nickName):
                    OnNickNameChanged();
                    break;
                case nameof(_carIndex):
                    OnCarIndexChanged();
                    break;
                case nameof(_colorIndex):
                    OnColorIndexChanged();
                    break;
            }
        }
    }

    public void TriggerRPC()
    {
        RPC_SetNickName(GameManager.instance.playerNickName);
        RPC_SetCarIndex(GameManager.instance.playerCarIndex);
        RPC_SetColor(GameManager.instance.playerCarColorIndex);
    }

    

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        Runner.Despawn(Object);
    }

    public void OnCarIndexChanged()
    {

        if (camaroModel == null || mustangModel == null) return;
        // Ensure both models are correctly activated or deactivated
        if (_carIndex == 0)
        {
            mustangModel.SetActive(false);
            camaroModel.SetActive(true);
        }
        else if (_carIndex == 1)
        {
            camaroModel.SetActive(false);
            mustangModel.SetActive(true);
        }

        // Apply the color to the active car model
        ApplyColor();

    }

    public void OnColorIndexChanged()
    {

        ApplyColor();
    }
    

    public void ApplyColor()
    {
        if (_colorMaterials == null || _colorMaterials.Count == 0) return;
        if (_colorMaterials.TryGetValue(_colorIndex, out var materials))
        {
            if (_carIndex == 0 && camaroRenderer != null)
            {
                camaroRenderer.material = materials[0];
            }
            else if (_carIndex == 1 && mustangRenderer != null)
            {
                mustangRenderer.material = materials[1];
            }
        }
    }

    public void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {_nickName} for player {gameObject.name}");
        playerNickName.text = _nickName.ToString();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this._nickName = nickName;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCarIndex(int carIndex, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetCarIndex {carIndex}");
        this._carIndex = carIndex;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetColor(int colorIndex, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetColor {colorIndex}");
        this._colorIndex = colorIndex;
    }

            
}
