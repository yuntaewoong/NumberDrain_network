using Photon.Realtime;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PhotonPlayer : MonoBehaviour
{
    public GameObject cardObjectPrefab;
    public SpriteRenderer handView;
    public SoundManager sm;

    private PhotonGameState gameState;
    private PhotonDeck deck;//masterclient만
    private SubmittedInfo[] submittedInfos;//masterClient만
    private int currentSubmittedNum = 0;//masterClient만
    private Card[] cards;
    private List<SubmittedInfo> otherPlayerCards;
    private GameObject onProcessingCardObject;
    private int currentHand = 0;
    private PhotonView pv;
    private bool disTributedCompleted = false;
    private int cardEmptyIndex = -1;//cards의 빈자리 인덱스 저장용,-1은 초기값
    public List<SubmittedInfo> GetOtherPlayerInfo()
    {
        return otherPlayerCards;
    }
    public int GetActorNumber()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber;
    }
    public void ClearData()
    {
        otherPlayerCards.Clear();
        currentSubmittedNum = 0;
    }
    public bool IsHandFull()
    {
        if (currentHand == GameStaticVariable.maxHand)
            return true;
        else
            return false;
    }
    public bool IsChampion()
    {
        for(int i = 0;i<cards.Length;i++)
        {
            if (cards[i].GetValue() >= 100)
                return true;
        }
        return false;
    }
    public void SubmitCard(CardGameObject cardGameObject)
    {
        sm.PlayHitAudio();
        Card card = cardGameObject.GetCard();
        pv.RPC("RPC_SubmitCard", RpcTarget.MasterClient,card,GetActorNumber());//제출
        pv.RPC("RPC_AnounceCard", RpcTarget.Others, card, GetActorNumber());//다른애들에게 뒷면 표시되게 알림
        onProcessingCardObject = Instantiate(cardObjectPrefab, new Vector2(0, 0), Quaternion.identity);
        onProcessingCardObject.GetComponent<CardGameObject>().SetCard(new Card(card.GetCardType(),card.GetValue(),true));//중앙에 카드생성
        Destroy(cardGameObject.gameObject);
        for (int i = 0;i<cards.Length;i++)
        {
            if (card == cards[i])
            {
                cardEmptyIndex = i;
                cards[i] = null;
                currentHand--;
            }  
        }
    }
    public bool Brawl(out SubmittedInfo winner)//승패가 정해지면 true,아니면 false
    {
        if (currentSubmittedNum == 0)
            Debug.LogError("Tried to divide by 0");
        for(int i = 0;i<currentSubmittedNum;i++)
        {
            int winCount = 0;
            for(int j = 0;j<currentSubmittedNum;j++)
            {
                if (i == j)
                    continue;//같은카드는 검사하지 않는다
                FightResult result = submittedInfos[i].GetCard().Fight(submittedInfos[j].GetCard());
                if (result == FightResult.Win)
                {
                    winCount++;
                    if (winCount == currentSubmittedNum-1)//모든놈 다이김
                    {
                        winner = submittedInfos[i];
                        return true;
                        //i가 이김
                    }
                    continue;//모두를 이겼는지 확인해야하므로 다음상대와 검사
                }
                else
                    break;//이미 이길수가 없으므로 다음카드부터 검사
            }
        }
        winner = null;
        return false;//아무도 이기지 못함
    }
    public void AnounceResult(int winActorNum,bool drawed)//이긴놈 없으면 winActorNum은 의미 x
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (drawed)
        {
            pv.RPC("RPC_AnounceResult", RpcTarget.All, FightResult.Draw,0);//모두에게 무승부라고 통보
            return;
        }
        for(int i = 0;i<PhotonNetwork.PlayerList.Length;i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == winActorNum)
                pv.RPC("RPC_AnounceResult", PhotonNetwork.PlayerList[i], FightResult.Win,winActorNum);
            else
                pv.RPC("RPC_AnounceResult", PhotonNetwork.PlayerList[i], FightResult.Lose,winActorNum);
        }
    }
    public void ResultDistributeCard()//masterclient가 결과에 따라 다시 판을 정리함
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (onProcessingCardObject != null)//내 카드가 이겨서 파괴되지 않았다면
            pv.RPC("RPC_DistributeCard", RpcTarget.MasterClient, onProcessingCardObject.GetComponent<CardGameObject>().GetCard());//나한테 그걸 전달
        else //아니면
            pv.RPC("RPC_DistributeCard", RpcTarget.MasterClient, deck.DrawCard());//새거뽑음
        GameObject[] otherObjects = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for (int i = 0;i<otherObjects.Length;i++)
        {
            int targetId = Int32.Parse(otherObjects[i].transform.Find("Canvas").GetComponentInChildren<Text>().text[3].ToString());
            if (otherObjects[i].transform.Find("CardPrefab").gameObject.activeInHierarchy)
                pv.RPC("RPC_DistributeCard", PhotonNetwork.PlayerList[0].Get(targetId), otherObjects[i].transform.Find("CardPrefab").GetComponent<CardGameObject>().GetCard());
            else
                pv.RPC("RPC_DistributeCard", PhotonNetwork.PlayerList[0].Get(targetId), deck.DrawCard());
        }
        
    }
    private void Start()
    {
        otherPlayerCards = new List<SubmittedInfo>();
        gameState = GameObject.Find("GameState").GetComponent<PhotonGameState>();
        pv = GetComponent<PhotonView>();
        StartCoroutine("HandState");
        if (!PhotonNetwork.IsMasterClient)//masterclient만이 deck을 관리함
            return;
        deck = GameObject.Find("Deck").GetComponent<PhotonDeck>();
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if ((gameState.GetGameState() == GameState.Wait)&& !disTributedCompleted)
        {
            Player[] players = PhotonNetwork.PlayerList;
            for (int j = 0; j < players.Length; j++)
            {
                for (int i = 0; i < GameStaticVariable.maxHand; i++)
                {
                    pv.RPC("RPC_DistributeCard", players[j], deck.DrawCard());
                }
            }
            disTributedCompleted = true;
        }
    }
    IEnumerator HandState()//handview담당
    {
        yield return new WaitUntil(() => GameStaticVariable.maxHand != 0);//staticvariable초기화 기달
        while (true)
        {
            yield return new WaitUntil(() => currentHand == GameStaticVariable.maxHand);//꽉차게 드로우할때까지 기달
            for (int i = 0; i < currentHand; i++)//currentHand는 0이 되어선 안됨
            {
                Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(new Vector2(handView.bounds.center.x - handView.bounds.extents.x + handView.bounds.extents.x * 2 / (currentHand + 1) * (i + 1), handView.bounds.center.y)));
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity,LayerMask.GetMask("Card"));
                if(hit)
                {
                    Destroy(hit.transform.parent.gameObject);
                }
                GameObject cardObject = Instantiate(cardObjectPrefab, 
                    new Vector2(handView.bounds.center.x - handView.bounds.extents.x + handView.bounds.extents.x * 2 / (currentHand + 1) * (i+1), handView.bounds.center.y), 
                    Quaternion.identity);//카드 오브젝트 보여줌
                cardObject.GetComponent<CardGameObject>().SetCard(cards[i]);//카드오브젝트에 카드정보전달
            }
            yield return new WaitUntil(() => currentHand != GameStaticVariable.maxHand);//현재핸드가 줄어들면 다시 루프시작
        }
    }
    [PunRPC]
    public void RPC_DistributeCard(Card card)//패 나눠주기
    {
        if (currentHand == GameStaticVariable.maxHand)
            Debug.Log("hand is full");
        if (cards == null)
        {
            cards = new Card[GameStaticVariable.maxHand];//player 소유의 카드초기화 (maxHand변수의 동기화가 distributecard함수 호출 전에 발생해서 여기서 초기화해줌)
        }
        if (cardEmptyIndex == -1)//초기조건
            cards[currentHand++] = card;
        else//빈자리 메우기
        {
            cards[cardEmptyIndex] = card;
            cardEmptyIndex = -1;
            currentHand++;
        }
    }
    [PunRPC]
    public void RPC_SubmitCard(Card card,int playerID)//master client에게 승부를 가를 카드를 1개씩 제출(자신을 구별할수 있게 id도 제출)
    {
        if (submittedInfos == null)
            submittedInfos = new SubmittedInfo[GameStaticVariable.maxPlayer];//만약 초기화 안되었을시 유저들이 제출한 카드를 저장할 배열초기화
        submittedInfos[currentSubmittedNum++] = new SubmittedInfo(card, playerID);
    }
    [PunRPC]
    public void RPC_AnounceResult(FightResult result,int winActorNum)//master client가 모든 클라이언트들에게 승부결과를 통보
    {
        switch (result)
        {
            case FightResult.Draw:
                DrawProcess();
                break;
            case FightResult.Win:
                WinProcess();
                break;
            case FightResult.Lose:
                LoseProcess(winActorNum);
                break;
        }
    }
    private void DrawProcess()//무승부 : 모든카드 폐기
    {
        GameObject[] otherPlayerObject = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for(int i = 0;i< otherPlayerObject.Length;i++)
        {
            otherPlayerObject[i].transform.Find("CardPrefab").gameObject.SetActive(false);
        }
        Destroy(onProcessingCardObject);
    }
    private void WinProcess()//이김 : 내카드 업데이트 상대방카드들 폐기
    {
        int resultValue = 0;
        foreach(SubmittedInfo otherCard in otherPlayerCards)
        {
            resultValue += otherCard.GetCard().GetValue();
            
        }
        resultValue += onProcessingCardObject.GetComponent<CardGameObject>().GetCard().GetValue();
        onProcessingCardObject.GetComponent<CardGameObject>().GetCard().SetValue(resultValue);//내카드 업데이트

        GameObject[] objects = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for(int i = 0;i<objects.Length;i++)
        {
            objects[i].transform.Find("CardPrefab").gameObject.SetActive(false);
        }
    }
    private void LoseProcess(int winActorNum)//짐 : 상대방카드중 하나 업데이트 그 카드제외 폐기
    {
        Card targetCard = null;
        int resultValue = 0;
        resultValue += onProcessingCardObject.GetComponent<CardGameObject>().GetCard().GetValue();
        foreach (SubmittedInfo otherCard in otherPlayerCards)
        {
            if (otherCard.GetActorNum() == winActorNum)
                targetCard = otherCard.GetCard();
            resultValue += otherCard.GetCard().GetValue();
        }
        targetCard.SetValue(resultValue);

        GameObject[] otherPlayerObject = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for (int i = 0; i < otherPlayerObject.Length; i++)
        {
            if (otherPlayerObject[i].transform.Find("Canvas").GetComponentInChildren<Text>().text[3].ToString() == winActorNum.ToString())
                continue;
            otherPlayerObject[i].transform.Find("CardPrefab").gameObject.SetActive(false);
        }
        Destroy(onProcessingCardObject);
    }
    [PunRPC]
    public void RPC_AnounceCard(Card card,int playerID)//다른 클라이언트들에게 내가 카드를 낸 사실을 알림(ui용)
    {
        sm.PlayHitAudio();
        card.SetIsReversed(true);//뒷면으로
        otherPlayerCards.Add(new SubmittedInfo(card, playerID));
    }
}
