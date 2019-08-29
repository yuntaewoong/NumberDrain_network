using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;



public class CardGameObject : MonoBehaviour
{
    public Sprite rock;
    public Sprite scissors;
    public Sprite paper;
    public SpriteRenderer cardTypeRenderer;
    public SpriteRenderer cardBackRenderer;
    public Text cardValueText;

    private Card card;
    public Card GetCard()
    {
        return card;
    }
    public void SetCard(Card card)
    {
        this.card = card;
        card.SetIsChanged(true);
    }
    private void Update()
    {
        if (!card.IsChanged())//멤버변수 변화가 없다면 바로리턴
            return;
        switch(card.GetCardType())//cardType변화 적용
        {
            case CardType.Rock:
                cardTypeRenderer.sprite = rock;
                break;
            case CardType.scissors:
                cardTypeRenderer.sprite = scissors;
                break;
            case CardType.paper:
                cardTypeRenderer.sprite = paper;
                break;
        }
        cardBackRenderer.enabled = card.IsReversed();//앞면 뒷면 변화 적용

        cardValueText.text = card.GetValue().ToString();//cardValue 적용
        card.SetIsChanged(false);//변화 다 적용했으니 다시 false값가짐
    }
}

