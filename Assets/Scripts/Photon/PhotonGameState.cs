using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState { BeforeGame = 0,Wait,processing,end}//게임의 상태
public class PhotonGameState : MonoBehaviour
{
    private GameState gameState = GameState.BeforeGame;
    private PhotonView pv;
    private bool isAllClientReadyToNextState = false;//masterclient only
    private int stateCheckingInt = 0;
    private int championId = 0;

    public int GetChampionId()
    {
        return championId;
    }
    public GameState GetGameState()
    {
        return gameState;
    }
    [PunRPC]
    public void RPC_AskForNextState()//master client를 타겟으로 호출("방장아 나 다음상태로 나갈수있음")
    {
        stateCheckingInt++;
        if (stateCheckingInt == PhotonNetwork.CurrentRoom.PlayerCount)//방에 참가자들의 게임상태가 모두바뀌면
        {
            isAllClientReadyToNextState = true;
        }
    }
    [PunRPC]
    public void RPC_AnnounceGameState(GameState gameState)//일반 client를 타겟으로 호출("ㅇㅋㅇㅋ 다음상태로 진도빼겠음")
    {
        this.gameState = gameState;
    }
    [PunRPC]
    public void RPC_SettingStaticVariables(int maxHand, int turnTime, int maxPlayer)//masterclient가 나머지들에게 static변수값들 알려줌
    {
        GameStaticVariable.maxHand = maxHand;
        GameStaticVariable.turnTime = turnTime;
        GameStaticVariable.maxPlayer = maxPlayer;
    }
    [PunRPC]
    public void RPC_AskForVictory(int actorNum)//한 클라이언트가 모든 클라이언트에게 승리를 알림
    {
        gameState = GameState.end;
        championId = actorNum;
    }
    public void AskForVictory(int actorNum)
    {
        pv.RPC("RPC_AskForVictory", RpcTarget.All, actorNum);
    }
    public void AskForNextState()
    {
        pv.RPC("RPC_AskForNextState", RpcTarget.MasterClient);
    }
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        gameState = GameState.BeforeGame;
        if (!PhotonNetwork.IsMasterClient)
            return;
        StartCoroutine("StateManage");
    }
    IEnumerator StateManage()//모든 참가자들의 게임state변경담당
    {
        while (true)
        {
            if (gameState == GameState.Wait)
                pv.RPC("RPC_SettingStaticVariables", RpcTarget.Others, GameStaticVariable.maxHand, GameStaticVariable.turnTime, GameStaticVariable.maxPlayer);
            yield return new WaitUntil(() => isAllClientReadyToNextState);
            yield return new WaitForSeconds(0.5f);//0.5초의 지연시간
            if (gameState == GameState.processing)
                pv.RPC("RPC_AnnounceGameState", RpcTarget.All, GameState.Wait);
            else 
                pv.RPC("RPC_AnnounceGameState", RpcTarget.All, (GameState)(((int)gameState + 1) % 3));
            isAllClientReadyToNextState = false;
            stateCheckingInt = 0;
        }
    }



}
