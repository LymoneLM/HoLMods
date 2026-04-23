using System;
using UnityEngine;
using UnityEngine.UI;

namespace PanelTweak;

public class RemoveSaveAlert : MonoBehaviour
{
    public Button cancelBT;
    public Button sureBT;

    public Text tip;
    public Text cancelText;
    public Text sureText;

    public Action OnRemove;
    
    private void Awake()
    {
        cancelBT.onClick.AddListener(CancelBT);
        sureBT.onClick.AddListener(SureBT);
        
        tip.font = FontManager.BoldFont;
        cancelText.font = FontManager.MediumFont;
        sureText.font = FontManager.MediumFont;
    }

    private void OnEnable()
    {
        tip.text = AllText.Text_UIA[32][Mainload.SetData[4]];
        cancelText.text = AllText.Text_UIA[33][Mainload.SetData[4]];
        sureText.text = AllText.Text_UIA[34][Mainload.SetData[4]];
    }

    private void SureBT()
    {
        OnRemove?.Invoke();
        gameObject.SetActive(false);
    }

    private void CancelBT()
    {
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        OnRemove = null;
    }
}