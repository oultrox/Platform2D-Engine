using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour 
{
    [SerializeField] private int playerHP;
    [SerializeField] private float GhostTime = 1;
    private SpriteRenderer sprite;
    private bool isFlashing;

    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFlashing)
        {
            return;
        }
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Me encontró! °.°");
            TakenDamage(collision.gameObject.GetComponent<Enemy>().TouchDamage);
            StartCoroutine(Flash());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isFlashing)
        {
            Debug.Log("Me encontró! °.°");
            TakenDamage(collision.gameObject.GetComponent<Enemy>().TouchDamage);
            StartCoroutine(Flash());
        }
    }

    private void TakenDamage(int amount)
    {
        playerHP -= amount;
    }

    private IEnumerator Flash()
    {
        isFlashing = true;
        WaitForSeconds delay = new WaitForSeconds(0.1f);
        int ghostTimeLoopDuration = (int)GhostTime * 5;
        Color colorAux = sprite.color;
        for (int i = 0; i < (ghostTimeLoopDuration); i++)
        {
            colorAux.a = 0f;
            sprite.color = colorAux;
            yield return delay;
            colorAux.a = 1f;
            sprite.color = colorAux;
            yield return delay;
        }
        isFlashing = false;
    }

}
