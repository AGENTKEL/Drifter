using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public string playerNickName = "";

    public int playerCarIndex;
    public int playerCarColorIndex;
    public int _money;
    private const string MoneyKey = "PlayerMoney";

    private void Awake() 
    {
        if (instance != this)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        InitializeMoney();

    }
    

    //Money Methods

    private void InitializeMoney()
    {
        if (!PlayerPrefs.HasKey(MoneyKey))
        {
            SetMoney(1000);
        }
        else
        {
            _money = PlayerPrefs.GetInt(MoneyKey);
        }
    }

    public int GetMoney()
    {
        return _money;
    }

    // Set the amount of money
    public void SetMoney(int amount)
    {
        _money = amount;
        PlayerPrefs.SetInt(MoneyKey, _money);
        PlayerPrefs.Save();
    }

    // Add money
    public void AddMoney(int amount)
    {
        _money += amount;
        PlayerPrefs.SetInt(MoneyKey, _money);
        PlayerPrefs.Save();
    }

    // Subtract money
    public bool SubtractMoney(int amount)
    {
        if (_money >= amount)
        {
            _money -= amount;
            PlayerPrefs.SetInt(MoneyKey, _money);
            PlayerPrefs.Save();
            return true;
        }
        else
        {
            Debug.Log("Not enough money!");
            return false;
        }
    }
}
