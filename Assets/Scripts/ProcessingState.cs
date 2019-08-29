using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessingState : MonoBehaviour
{
    public float compareTime;
    public float resultingTime;

    private PhotonGameState gameState;
    private GameObject[] processingCards;
    private PhotonPlayer player;//masterclient only
    private void Start()
    {
        gameState = GetComponent<PhotonGameState>();
        player = GameObject.Find("Player").GetComponent<PhotonPlayer>();
        StartCoroutine("Processing");

    }
    IEnumerator Processing()
    {
        while (true)
        {
            yield return new WaitUntil(() => gameState.GetGameState() == GameState.processing);//processing상태까지 기달
            //processing상태 == 모두가 카드를 내고 진행화면이 나오길 기다리고 있는 상태

            //yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Card").Length - PhotonNetwork.PlayerList.Length + 1
            //== GameStaticVariable.maxHand);//모든 카드가 다 소환될때까지 기달

            
            processingCards = GameObject.FindGameObjectsWithTag("Card");
            for (int i = 0; i < processingCards.Length; i++)
            {
                processingCards[i].GetComponent<CardGameObject>().GetCard().SetIsReversed(false);
            }//뒷면 까기

            yield return new WaitForSeconds(compareTime);//승부전에 대기시간

            if (PhotonNetwork.IsMasterClient)//masterclient는 게임진행하고 결과통보
            {
                if (player.Brawl(out SubmittedInfo winner) == true)//가위 바위 보!
                {
                    player.AnounceResult(winner.GetActorNum(), false);//이놈이 이기고 나머지 짐
                }
                else
                {
                    player.AnounceResult(0, true);//무승부
                }
            }

            yield return new WaitForSeconds(resultingTime);

            player.ResultDistributeCard();
            player.ClearData();
            Clearing();
            yield return new WaitUntil(() => player.IsHandFull());//다시 드로우한거 받을때까지 기달
            if (player.IsChampion())//우승하면
                gameState.AskForVictory(PhotonNetwork.LocalPlayer.ActorNumber);
            else//아니면
                gameState.AskForNextState();//순서 - before wait processing wait processing-----
            yield return new WaitWhile(() => gameState.GetGameState() == GameState.processing);
        }
    }
    private void Clearing()//화면 정리
    {
        GameObject[] otherObjects = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for(int i = 0;i<otherObjects.Length;i++)
        {
            otherObjects[i].transform.Find("CardPrefab").gameObject.SetActive(false);//카드 숨기기
        }
        Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(new Vector2(0,0)));
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Card"));
        if (hit)
        {
            Destroy(hit.transform.parent.gameObject);
        }
    }
}
