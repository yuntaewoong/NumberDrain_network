using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonRoom : MonoBehaviourPunCallbacks,IInRoomCallbacks
{
    public static PhotonRoom room;
    public int multiplayerScene;
    
    
    private Button readyButton;
    private Text playerIdText;
    private PhotonView pv;
    private int currentScene;
    

    private void Awake()
    {
        if (room == null)
        {
            room = this;
        }
        else
        {
            if (room != this)
            {
                Destroy(room.gameObject);
                room = this;
            }
        }
        DontDestroyOnLoad(room.gameObject);
        pv = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
        
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
        
    }
    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        
        if (!PhotonNetwork.IsMasterClient)
            return;
        StartGame();
    }
    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.LoadLevel(multiplayerScene);
    }
    private void OnSceneFinishedLoading(Scene scene,LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if (currentScene == multiplayerScene)
        {
            PhotonPeer.RegisterType(typeof(Card), (byte)'C', Card.Serialize, Card.Deserialize);
            playerIdText = GameObject.Find("Canvas").transform.Find("IDText").GetComponent<Text>();
            playerIdText.text = "Your Id : " + PhotonNetwork.LocalPlayer.ActorNumber;
        }
    }
    
}
