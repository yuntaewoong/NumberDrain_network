using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public int submitZoneYPixel;

    private PhotonGameState gameState;
    private PhotonPlayer player;
    private bool isCardSelected = false;
    private Vector2 cardReturnPosition;
    private CardGameObject cardObject;
    private Transform cardTransform;
    private bool isSubmitFinish = false;

    public bool IsSubmitFinish()
    {
        return isSubmitFinish;
    }

    private void Start()
    {
        gameState = GameObject.Find("GameState").GetComponent<PhotonGameState>();
        player = GetComponent<PhotonPlayer>();
    }

    private void Update()
    {
        if (gameState.GetGameState() == GameState.processing)
            isSubmitFinish = false;
        if (gameState.GetGameState() != GameState.Wait)//wait상태일때만 입력받음
            return;
        if (isSubmitFinish)
            return;
        Touch touch = new Touch();
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Card"));//입력좌표에 카드있나 감지
            if (hit && !isCardSelected)//있으면
            {
                isCardSelected = true;
                cardObject = hit.collider.gameObject.GetComponentInParent<CardGameObject>();
                cardTransform = hit.collider.gameObject.transform.parent;
                cardReturnPosition = cardTransform.position;
            }
            if (isCardSelected)//카드가 잡혀있고 마우스가 계속 눌려있다면
            {
                cardTransform.position = (Vector2)Camera.main.ScreenToWorldPoint(touch.position);//마우스 따라 이동
            }
            if (touch.phase == TouchPhase.Ended && isCardSelected)
            {
                if (touch.position.y > submitZoneYPixel)
                {
                    player.SubmitCard(cardObject);//제출
                    isCardSelected = false;
                    isSubmitFinish = true;
                }
                else
                {
                    isCardSelected = false;
                    cardTransform.position = cardReturnPosition;//원래자리로 백
                }
            }
        }
    }
}
