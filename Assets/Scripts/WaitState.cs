using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WaitState : MonoBehaviour//wait state일때 수행되는 일들
{
    public Text leftTimeText;
    public SoundManager sm;

    private PhotonPlayer player;
    private PlayerInput playerInput;
    private PhotonGameState gameState;
    private List<GameObject> otherPlayerGameObjects;//상대 ui오브젝트들 참조
    private List<SubmittedInfo> otherPlayerSubmit;//상대의 카드 제출 현황참조

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PhotonPlayer>();
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        gameState = GetComponent<PhotonGameState>();
        StartCoroutine("Timer");
        StartCoroutine("OtherPlayerUpdate");
    }
    IEnumerator Timer()
    {
        yield return new WaitUntil(() => gameState.GetGameState() == GameState.Wait);//gamestate가 wait가 되면
        while (true)
        {
            sm.PlayWaitAudio();
            float leftTime = GameStaticVariable.turnTime;
            while (true)
            {
                leftTime -= Time.deltaTime;
                leftTimeText.text = "Left Time : " + Mathf.Round(leftTime * 100) / 100;
                if (leftTime < 0)//시간이 끝남
                {
                    GameObject[] cardObjects = GameObject.FindGameObjectsWithTag("Card");
                    int ran = Random.Range(0, GameStaticVariable.maxHand);//내 손중 몇번째 있는거 버릴지
                    int resultIndex = 0;
                    for(int i = 0;i<cardObjects.Length;i++)
                    {
                        if (cardObjects[i].GetComponent<CardGameObject>().GetCard().IsReversed())//뒤집어져 있는건 상대거
                            continue;
                        ran--;
                        if(ran == 0)
                        {
                            resultIndex = i;
                            break;
                        }
                    }
                    player.SubmitCard(cardObjects[resultIndex].GetComponent<CardGameObject>());//랜덤카드 제출
                    leftTimeText.text = "Left Time : " + "0";
                    break;
                }
                if (playerInput.IsSubmitFinish())
                {
                    break;
                }//시간 표시
                yield return null;
            }
            gameState.AskForNextState();
            leftTime = GameStaticVariable.turnTime;
            yield return new WaitUntil(() => gameState.GetGameState() == GameState.processing);//processing까지 기달
            yield return new WaitWhile(() => gameState.GetGameState() != GameState.Wait);//wait상태가 다시 될때까지 기달
        }
    }
    IEnumerator OtherPlayerUpdate()
    {
        yield return new WaitForSeconds(0.1f);//다른 start보다 먼저 실행되는거 방지
        yield return new WaitUntil(() => gameState.GetGameState() == GameState.Wait);
        otherPlayerSubmit = GameObject.Find("Player").GetComponent<PhotonPlayer>().GetOtherPlayerInfo();//player가 감지한 상대오브젝트정보 참조
        otherPlayerGameObjects = GetComponent<BeforeState>().GetOtherPlayer();//before state의 결과인 상대오브젝트들 참조
        while (true)
        {
            foreach (SubmittedInfo submit in otherPlayerSubmit)
            {
                foreach (GameObject otherPlayer in otherPlayerGameObjects)
                {
                    Text text = otherPlayer.transform.Find("Canvas").Find("Text").GetComponent<Text>();
                    if (submit.GetActorNum().ToString() == text.text[3].ToString())//id가 같다면 (id index == 3)
                    {
                        GameObject cardObject = otherPlayer.transform.Find("CardPrefab").gameObject;
                        cardObject.SetActive(true);
                        cardObject.GetComponent<CardGameObject>().SetCard(submit.GetCard());//카드 업데이트
                    }
                }
            }
            yield return new WaitWhile(() => gameState.GetGameState() != GameState.Wait);//wait상태가 아닐때는 update중단
            yield return null;
        }
    }
}
