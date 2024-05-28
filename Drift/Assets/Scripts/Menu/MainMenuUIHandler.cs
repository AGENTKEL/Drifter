using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject nickNameMenu;
    public TMP_InputField nickNameInputField;
    public GameObject mainMenuUI;
    public GameObject garageUI;
    public GameObject settingsUI;
    public GameObject upperMenuUI;

    public TextMeshProUGUI moneyText;

    [Header("Settings")]
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;


    [Header("Car Selection")]
    [SerializeField] private TextMeshProUGUI _selectCarText;
    [SerializeField] private string _key;
    [SerializeField] private Transform _itemParent;
    private int _currentIndex;
    private int _savedItemIndex;
    private const int CarCost = 1000;

    [Header("Car Materials")]
    [SerializeField] private List<Material> blueMaterials;
    [SerializeField] private List<Material> greenMaterials;
    [SerializeField] private List<Material> redMaterials;
    [SerializeField] private List<Material> yellowMaterials;
    private Dictionary<string, List<Material>> _colorMaterials;



    private int _currentColorIndex;
    private int _savedColorIndex;
    [SerializeField] private string _colorKey;


    void Start()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner != null)
            networkRunner.Shutdown();
            
        if (PlayerPrefs.HasKey("PlayerNickName"))
            nickNameInputField.text = PlayerPrefs.GetString("PlayerNickName");


        //Resolution Set
        resolutions = Screen.resolutions;    
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        //Garage Start
        for (int i = 0; i < _itemParent.childCount; i++)
            _itemParent.GetChild(i).gameObject.SetActive(false);

        _savedItemIndex = PlayerPrefs.HasKey(_key) ? PlayerPrefs.GetInt(_key) : 0;
        _currentIndex = _savedItemIndex;

        _itemParent.GetChild(_savedItemIndex).gameObject.SetActive(true);
        _selectCarText.text = _savedItemIndex == _currentIndex ? "Choosen" : "Choose";

        // Initialize color materials dictionary
        _colorMaterials = new Dictionary<string, List<Material>>()
        {
            { "Blue", blueMaterials },
            { "Green", greenMaterials },
            { "Red", redMaterials },
            { "Yellow", yellowMaterials }
        };

        // Load saved color
        _savedColorIndex = PlayerPrefs.HasKey(_colorKey) ? PlayerPrefs.GetInt(_colorKey) : 0;
        _currentColorIndex = _savedColorIndex;
        ApplyColor(_currentColorIndex);
    }

    private void Update() 
    {
        moneyText.text = GameManager.instance._money.ToString();
    }

#region Garage Methods

    public void GarageButton()
    {
        garageUI.SetActive(true);
        mainMenuUI.SetActive(false);
        UpdateSelectCarText();
    }
    

    public void SelectLeft()
    {
        _itemParent.GetChild(_currentIndex).gameObject.SetActive(false);

        if (_currentIndex - 1 >= 0)
            _currentIndex--;
        else
            _currentIndex = _itemParent.childCount - 1;

        _itemParent.GetChild(_currentIndex).gameObject.SetActive(true);
        _selectCarText.text = _savedItemIndex == _currentIndex ? "Choosen" : "Choose";
        UpdateSelectCarText();
        ApplyColor(_currentColorIndex);
    }

    public void SelectRight()
    {
        _itemParent.GetChild(_currentIndex).gameObject.SetActive(false);

        if (_currentIndex + 1 < _itemParent.childCount)
            _currentIndex++;
        else
            _currentIndex = 0;

        _itemParent.GetChild(_currentIndex).gameObject.SetActive(true);
        _selectCarText.text = _savedItemIndex == _currentIndex ? "Choosen" : "Choose";
        UpdateSelectCarText();
        ApplyColor(_currentColorIndex);
    }

public void SaveItem()
    {
        if (_currentIndex == 1 && !IsCarPurchased(_currentIndex))
        {
            if (GameManager.instance.GetMoney() >= CarCost)
            {
                GameManager.instance.SubtractMoney(CarCost);
                PlayerPrefs.SetInt("CarPurchased_" + _currentIndex, 1);
                _selectCarText.text = "Choosen";
                GameManager.instance.playerCarIndex = _currentIndex;
                Debug.Log("Car purchased and selected");
            }
            else
            {
                Debug.Log("Not enough money to buy this car");
            }
        }
        else
        {
            PlayerPrefs.SetInt(_key, _currentIndex);
            _savedItemIndex = _currentIndex;
            _selectCarText.text = "Choosen";
            GameManager.instance.playerCarIndex = _currentIndex;
        }
    }

    private bool IsCarPurchased(int index)
    {
        return PlayerPrefs.GetInt("CarPurchased_" + index, 0) == 1;
    }

    private void UpdateSelectCarText()
    {
        if (_currentIndex == _savedItemIndex)
        {
            _selectCarText.text = "Choosen";
        }
        else if (_currentIndex == 1 && !IsCarPurchased(_currentIndex))
        {
            _selectCarText.text = "Buy for " + CarCost + " Money";
        }
        else
        {
            _selectCarText.text = "Choose";
        }
    }

    public void ChangeCarColor(string color)
    {
        if (_colorMaterials.ContainsKey(color) && _currentIndex < _itemParent.childCount)
        {
            var car = _itemParent.GetChild(_currentIndex).gameObject;
            var renderer = car.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                renderer.material = _colorMaterials[color][_currentIndex];
                _currentColorIndex = GetColorIndex(color);
                PlayerPrefs.SetInt(_colorKey, _currentColorIndex);

                GameManager.instance.playerCarColorIndex = _currentColorIndex;
            }
        }
    }

    private void ApplyColor(int colorIndex)
    {
        string colorName = GetColorName(colorIndex);
        if (_colorMaterials.ContainsKey(colorName) && _currentIndex < _itemParent.childCount)
        {
            var car = _itemParent.GetChild(_currentIndex).gameObject;
            var renderer = car.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                renderer.material = _colorMaterials[colorName][_currentIndex];
            }
        }
    }

    private int GetColorIndex(string color)
    {
        switch (color)
        {
            case "Blue": return 0;
            case "Green": return 1;
            case "Red": return 2;
            case "Yellow": return 3;
            default: return 0;
        }
    }

    private string GetColorName(int index)
    {
        switch (index)
        {
            case 0: return "Blue";
            case 1: return "Green";
            case 2: return "Red";
            case 3: return "Yellow";
            default: return "Blue";
        }
    }

    #endregion

    public void OnStartGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickName", nickNameInputField.text);
        PlayerPrefs.Save();

        GameManager.instance.playerNickName = nickNameInputField.text;

        nickNameMenu.SetActive(false);
        mainMenuUI.SetActive(true);
        upperMenuUI.SetActive(true);
    }

    public void BackButton()
    {
        mainMenuUI.SetActive(true);
        garageUI.SetActive(false);
        settingsUI.SetActive(false);
    }


    public void OnJoinClicked()
    {
        SceneManager.LoadScene("complete_track_demo");
    }

    #region Settings

    public void SettingsButton()
    {
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    #endregion
    
}
