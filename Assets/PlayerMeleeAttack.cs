using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMeleeAttack : MonoBehaviour 
{ 
    const float TIME_BETWEEN_ATTACKS = 0.2f;
    private GameObject slashObject;
    private Transform slashChildren;
    private Animator animator;
    private SpriteRenderer playerOrientation;
    private PlatformMotor2D playerController;
    private float timeCounter = 0;
    private float debuggerTime;
    private PlayerInput playerInput;
    
    
    void Awake()
    {
        SetBehaviors();
    }
    
    void Start()
    {
        playerController = this.GetComponent<PlayerMovementController>().PlayerMotor;
        slashObject.SetActive(false);
    }

    void Update()
    {
        CheckAttack();
    }

    void CheckAttack()
    {
        timeCounter += Time.deltaTime;
        if (!playerInput.Attacked) return;
        if (timeCounter >= TIME_BETWEEN_ATTACKS && !playerController.collisionInfo.isStickedToWall)
        {
            timeCounter = 0;
            StartCoroutine(Slash());
        }
    }
    
    void SetBehaviors()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Slash"))
            {
                slashObject = child.gameObject;
            }
        }
        animator = slashObject.GetComponent<Animator>();
        slashChildren = slashObject.transform;
        playerOrientation = this.GetComponentInParent<SpriteRenderer>();
        playerInput = this.GetComponent<PlayerInput>();
    }

    IEnumerator Slash()
    {
        slashObject.SetActive(true);

        if (playerOrientation.flipX)
        {
            slashChildren.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            slashChildren.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        debuggerTime = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(debuggerTime);
        slashObject.SetActive(false);
    }
}
