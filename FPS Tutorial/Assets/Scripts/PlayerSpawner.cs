using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{

    public static PlayerSpawner instance;
    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;

    public float respawnTime = 5f; 
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name,spawnPoint.position, spawnPoint.rotation);
    }

    public void Die(string damager)
    {
        // log the death message
        UIController.instance.deathText.text = "You were killed by " + damager;
        //
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
    
        if(player != null)
        {
            StartCoroutine(DieCo());
        }
    }
    public IEnumerator DieCo()
    {
        PhotonNetwork.Destroy(player);
        UIController.instance.deathScrean.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScrean.SetActive(false);
        SpawnPlayer();
    }
}
