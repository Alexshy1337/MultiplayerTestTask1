using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetwork : NetworkBehaviour
{
    FixedJoystick fJMovement;
    Button shootBtn;

    public override void OnNetworkSpawn()
    {
        fJMovement = GameObject.FindGameObjectWithTag("movementJoystick").GetComponent<FixedJoystick>();
        shootBtn = GameObject.FindGameObjectWithTag("shootBtn").GetComponent<Button>();

        shootBtn.onClick.AddListener(() =>
        {

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
    }
}
