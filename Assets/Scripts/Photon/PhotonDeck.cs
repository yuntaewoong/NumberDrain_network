using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonDeck : MonoBehaviour
{
    private Card[] deck;
    private int deckSize = 30;
    private int currentIndex = 0;
    private Text deckText;
    private PhotonView pv;

    public Card DrawCard()
    {
        if (currentIndex == 30)
        {
            Shuffle();
            currentIndex = 0;
        }
        return deck[currentIndex++];
    }
    private void Shuffle()//덱을 무작위로 섞음
    {
        Card tempCard;
        int randomNum;
        for (int i = 0; i < deckSize - 1; i++)
        {
            randomNum = Random.Range(i, deckSize);
            tempCard = deck[i];
            deck[i] = deck[randomNum];
            deck[randomNum] = tempCard;
        }
    }
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        deckText = GetComponentInChildren<Text>();
        if (!PhotonNetwork.IsMasterClient)//방장이 덱관리 담당
            return;
        deck = new Card[deckSize];
        for(int i =0;i<deckSize;i++)//덱 생성
        {
            if(i < deckSize/3)//주먹
            {
                deck[i] = new Card(CardType.Rock, (i + 1) % 10 + 1, false);
            }
            else if(i < deckSize / 3 * 2)//가위
            {
                deck[i] = new Card(CardType.scissors, (i + 1) % 10 + 1, false);
            }
            else
            {
                deck[i] = new Card(CardType.paper, (i + 1) % 10 + 1, false);
            }
        }
        Shuffle();//덱 섞기
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("RPC_UpdateDeck", RpcTarget.Others, currentIndex);
        TextUpdate();
    }
    private void TextUpdate()
    {
        deckText.text = (deckSize - currentIndex) + " Left";
    }
    [PunRPC]
    public void RPC_UpdateDeck(int currentIndex)//masterclient의 덱 인덱스를 다른 클라이언트들에게 전달
    {
        this.currentIndex = currentIndex;
    }
}
