using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerNetwork : NetworkBehaviour
{
    private FixedJoystick fJMovement;
    private Button shootBtn;
    private Camera mainCamera;
    private Transform gunPoint;
    public string getPlayerName()
    {
        return LobbyManager.Instance.getPlayerName;
    }

    public override void OnNetworkSpawn()
    {
        fJMovement = GameObject.FindGameObjectWithTag("movementJoystick").GetComponent<FixedJoystick>();
        shootBtn = GameObject.FindGameObjectWithTag("shootBtn").GetComponent<Button>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        gunPoint = transform.GetChild(0);
        if (IsOwner)
            shootBtn.onClick.AddListener(() =>
        {
            MPGameManager.instance.ShootServerRpc(gunPoint.position, gunPoint.position - transform.position);
        });


    }

    void Update()
    {
        if(!IsOwner)
            return;

        float moveSpeed = 5f;
        float rotationSpeed = 100f;

        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z - fJMovement.Direction.x * rotationSpeed * Time.deltaTime);
        transform.position += new Vector3(
            fJMovement.Direction.y * Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * moveSpeed * Time.deltaTime,
            fJMovement.Direction.y * Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * moveSpeed * Time.deltaTime,
            0);

        MoveCamera();

    }

    private void MoveCamera()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);

    }

    
     private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.LogFormat(" OnTriggerEnter2D is called and is owner:  {0}", IsOwner);
        if (collision.CompareTag("Bullet") && IsOwner)
        {
            var name = LobbyManager.Instance.getPlayerName;
            MPGameManager.instance.TakeDamageServerRpc(name);
            Debug.LogFormat(" OnTriggerEnter2D is called and name: {0}", name);
        }

    }
}
