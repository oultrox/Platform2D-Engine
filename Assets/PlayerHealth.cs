using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

    [SerializeField] private int playerHP;
	

    //test
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Me encontró! °.°");
            TakenDamage(collision.gameObject.GetComponent<Enemy>().TouchDamage);
        }
    }

    private void TakenDamage(int amount)
    {
        playerHP -= amount;
    }
}
