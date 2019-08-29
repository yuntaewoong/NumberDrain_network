using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndState : MonoBehaviour
{
    private PhotonGameState gameState;
    private Text resultText;
    private Image endUIBackgroundColor;
    private GameObject endUI;
    private void Start()
    {
        gameState = GetComponent<PhotonGameState>();
        endUI = GameObject.Find("Canvas").transform.Find("EndUI").gameObject;
        resultText = GameObject.Find("Canvas").transform.Find("EndUI").Find("Text").GetComponent<Text>();
        endUIBackgroundColor = GameObject.Find("Canvas").transform.Find("EndUI").Find("Image").GetComponent<Image>();
        StartCoroutine("Ending");
    }
    IEnumerator Ending()
    {
        yield return new WaitUntil(() => gameState.GetGameState() == GameState.end);
        endUI.SetActive(true);
        if(PhotonNetwork.LocalPlayer.ActorNumber == gameState.GetChampionId())//우승자면
        {
            resultText.text = "Win!";
            endUIBackgroundColor.color = Color.cyan;
        }
        else
        {
            resultText.text = "Lose!\nwinner : " + gameState.GetChampionId();//패배자면
            endUIBackgroundColor.color = Color.red;
        }
    }
    public void OnClikedMainButton()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        SceneManager.LoadScene(0);//메인 메뉴로
        
    }
}
