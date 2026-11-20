using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionItemUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sessionNameText;
    [SerializeField] private TextMeshProUGUI _sessionMapText;
    [SerializeField] private TextMeshProUGUI _numberOfPLayerText;

    public void SetTexts(string sessionName, string sessionMap, string numberOfPlayers)
    {
        _sessionNameText .text = sessionName;
        _sessionMapText .text = sessionMap;
        _numberOfPLayerText .text = numberOfPlayers;
    }

    public void OnJoinButtonClicked()
    {

    }
}
