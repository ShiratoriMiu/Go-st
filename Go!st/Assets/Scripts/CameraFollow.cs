using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float speed;

    GameObject player;
    Vector3 offset;

    PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        offset = player.transform.position - this.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!playerController.GetIsSkill())
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position - offset, speed);
        }
    }
}
