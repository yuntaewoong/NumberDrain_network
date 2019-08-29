using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeforeState : MonoBehaviour//beforestate는 모두가 ready버튼을 누르기 전 상태임
{
    public GameObject otherPlayerView;
    public Button readyButton;
    
    private List<GameObject> otherPlayerGameObjects;
    private PhotonGameState gameState;
    private PhotonPlayer photonPlayer;

    public List<GameObject> GetOtherPlayer()
    {
        return otherPlayerGameObjects;
    }

    private void Start()
    {
        readyButton = GameObject.Find("ReadyButton").GetComponent<Button>();
        readyButton.onClick.AddListener(OnReadyButtonCliked);
        otherPlayerGameObjects = new List<GameObject>();
        photonPlayer = GameObject.Find("Player").GetComponent<PhotonPlayer>();
        gameState = GetComponent<PhotonGameState>();
        StartCoroutine("OtherPlayerView");
    }
    IEnumerator OtherPlayerView()
    {
        yield return new WaitForSeconds(0.1f);//다른 start보다 먼저 실행되는거 방지
        while (true)
        {
            Player[] players = PhotonNetwork.PlayerList;
            foreach (GameObject otherPlayer in otherPlayerGameObjects)
                Destroy(otherPlayer);
            otherPlayerGameObjects.Clear();


            int locateIndex = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].ActorNumber == photonPlayer.GetActorNumber())//나 자신은 표현하지 않음
                {
                    continue;
                }
                GameObject newOtherPlayer = Instantiate(otherPlayerView,
                    (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.scaledPixelWidth / players.Length * (locateIndex + 1), 540)) + new Vector2(0, 3),
                    Quaternion.identity);
                locateIndex++;
                Text[] texts = newOtherPlayer.GetComponentsInChildren<Text>();
                texts[0].text = "ID:" + players[i].ActorNumber;
                otherPlayerGameObjects.Add(newOtherPlayer);
            }
            int pastPlayerNum = players.Length;
            yield return new WaitUntil(() => pastPlayerNum + 1 == PhotonNetwork.PlayerList.Length);
            yield return new WaitWhile(() => gameState.GetGameState() != GameState.BeforeGame);
        }
    }
    private void OnReadyButtonCliked()
    {
        gameState.AskForNextState();//master client에게 레디했다고 알림
        readyButton.gameObject.SetActive(false);//button 안보이게 처리
    }
}
