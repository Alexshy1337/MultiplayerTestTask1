using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNetwork : NetworkBehaviour
{
    private FixedJoystick fJMovement;
    private Button shootBtn;
    private Camera mainCamera;
    private Transform gunPoint;
    private short health = MPGameManager.PLAYER_HEALTH;
    private TextMeshProUGUI healthText;

    public override void OnNetworkSpawn()
    {
        healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();
        fJMovement = GameObject.FindGameObjectWithTag("movementJoystick").GetComponent<FixedJoystick>();
        shootBtn = GameObject.FindGameObjectWithTag("shootBtn").GetComponent<Button>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        gunPoint = transform.GetChild(0);
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

    private void TakeDamage()
    {
        health -= MPGameManager.BULLLET_DAMAGE;
        UpdateHealth();
        //if(health <= 0)
    }

    void UpdateHealth()
    {
        healthText.text = "Health: " + health.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
            TakeDamage();
    }
}
