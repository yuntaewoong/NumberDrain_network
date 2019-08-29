using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public GameObject[] roomButton;
    public GameObject roomMakingUI;
    public GameObject roomMakingButton;
    public Text maxPlayerInput;
    public Text maxHandInput;
    public Text turnTimeInput;

    private int maxPlayer = 0;
    private int maxHand = 0;
    private int turnTime = 0;
    private int selectedRoomNum;
    private List<RoomInfo> roomList;
    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has been connected to photon server");
        for (int i =0;i<roomButton.Length;i++)
        {
            roomButton[i].SetActive(true);
        }
        PhotonNetwork.JoinLobby();
    }
    public void OnRoomButtonCliked(int roomNum)
    {
        foreach(RoomInfo roomInfo in roomList)//존재하는 방리스트중 들어갈수 있는 방이 있는지 확인
        {
            if(Int32.Parse(roomInfo.Name.Substring(4)) == roomNum &&
                roomInfo.MaxPlayers > roomInfo.PlayerCount)//번호에 맞는 방이 존재하고 인원수가 다 안찼는지
            {
                PhotonNetwork.JoinRoom("Room" + roomNum);//입장
                return;
            }
        }
        selectedRoomNum = roomNum;
        roomMakingUI.SetActive(true);
        StartCoroutine("CheckingSetting");//새로 방파기
    }
    public void OnMakingRoomButtonCliked()
    {
        GameStaticVariable.maxPlayer = maxPlayer;
        GameStaticVariable.maxHand = maxHand;
        GameStaticVariable.turnTime = turnTime;
        CreateRoom(selectedRoomNum);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        this.roomList = roomList;
        Debug.Log(roomList.Count);
        foreach(RoomInfo roomInfo in roomList)
        {
            Slider slider = roomButton[Int32.Parse(roomInfo.Name.Substring(4))-1].GetComponentInChildren<Slider>();
            slider.maxValue = roomInfo.MaxPlayers;
            slider.value = roomInfo.PlayerCount;//slider로 방정보 표시
        }
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Create room");
    }
    private void CreateRoom(int roomNum)
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)GameStaticVariable.maxPlayer };
        PhotonNetwork.CreateRoom("Room" + roomNum, roomOps);
        StopCoroutine("CheckingSetting");
    }
    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        PhotonNetwork.ConnectUsingSettings();
    }
    IEnumerator CheckingSetting()
    {

        while (true)
        {
            if (Int32.TryParse(maxPlayerInput.text, out maxPlayer) &&
                Int32.TryParse(maxHandInput.text, out maxHand) &&
                Int32.TryParse(turnTimeInput.text, out turnTime) &&
                maxPlayer >= 2 && maxPlayer <= 4 &&
                    maxHand >= 2 && maxHand <= 5 &&
                    turnTime >= 5 && turnTime <= 30)//정수가 모두 입력되고 입력값이 범위에 맞다면
            {
                    roomMakingButton.GetComponent<Button>().interactable = true;
            }
            else
                roomMakingButton.GetComponent<Button>().interactable = false;
            yield return null;

        }
    }
}
