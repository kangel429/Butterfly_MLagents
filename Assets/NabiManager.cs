using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NabiManager : MonoBehaviour
{
    public int userCount;
    public GameObject mPlayer;
    public List<GameObject> mUserList = new List<GameObject>();

//    bool initSetting = true;

    public bool trainMode;

    // Start is called before the first frame update
    void Awake()
    {
        if (trainMode)
        {
            for (int i = 0; i < userCount; i++)
            {
                GameObject player = Instantiate(mPlayer) as GameObject;
                player.transform.parent = this.transform;
                player.GetComponentInChildren<SkinnedMeshRenderer>().material.color = Color.red;
                mPlayer.SetActive(false);
                player.SetActive(true);
                mUserList.Add(player);
            }


        }

    }

    // Update is called once per frame
    void Update()
    {
    }
}
