using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Rock = 0, scissors, paper }
public enum FightResult { Win = 0, Lose, Draw }
public class Card
{
    private CardType cardType;
    private int cardValue;
    private bool isChanged = false;
    private bool isReversed = false;


    public static object Deserialize(byte[] data)
    {
        var result = new Card();
        result.CardInit((CardType)data[0], data[1], (data[2] == 1) ? true:false);
        return result;
    }
    public static byte[] Serialize(object customType)
    {
        var c = (Card)customType;
        return new byte[] { (byte)c.cardType, (byte)c.cardValue, c.isReversed ? (byte)1 : (byte)0 };
    }
    public Card() { }//기본 생성자
    public Card(CardType cardType, int cardValue, bool isReversed)//생성자
    {
        this.cardType = cardType;
        this.cardValue = cardValue;
        this.isReversed = isReversed;
        isChanged = true;
    }
    public void CardInit(CardType cardType, int cardValue, bool isReversed)//기본생성자로 card생성시 초기화함수
    {
        this.cardType = cardType;
        this.cardValue = cardValue;
        this.isReversed = isReversed;
        isChanged = true;
    }
    public FightResult Fight(Card opponentCard)//1명의 상대와 가위바위보 결과값 리턴
    {
        if (((int)cardType + 1) % 3 == (int)opponentCard.GetCardType())
            return FightResult.Win;
        else if (cardType == opponentCard.GetCardType())
        {
            if (cardValue > opponentCard.GetValue())
                return FightResult.Win;
            else if (cardValue < opponentCard.GetValue())
                return FightResult.Lose;
            else
                return FightResult.Draw;
        }
        else
            return FightResult.Lose;
    }
    public CardType GetCardType()
    {
        return cardType;
    }
    public int GetValue()
    {
        return cardValue;
    }
    public bool IsChanged()
    {
        return isChanged;
    }
    public bool IsReversed()
    {
        return isReversed;
    }
    public void SetValue(int cardValue)
    {
        this.cardValue = cardValue;
        isChanged = true;
    }
    public void SetType(CardType cardType)
    {
        this.cardType = cardType;
        isChanged = true;
    }
    public void SetIsReversed(bool isReversed)
    {
        this.isReversed = isReversed;
        isChanged = true;
    }
    public void SetIsChanged(bool isChanged)
    {
        this.isChanged = isChanged;
    }
}

