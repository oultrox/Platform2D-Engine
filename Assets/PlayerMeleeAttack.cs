using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMeleeAttack : MonoBehaviour {

    [SerializeField] private float timeBetweenAttacks;
    private GameObject slashObject;
    private Transform slashChildren;
    private Animator animator;
    private SpriteRenderer playerOrientation;
    private PlatformMotor2D playerController;
    private float timeCounter = 0;
    private float debuggerTime;
    private PlayerInput playerInput;
    
    private void Awake()
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
        playerController = this.GetComponent<PlatformMotor2D>();
        playerInput = this.GetComponent<PlayerInput>();
    }

    private void Start()
    {
        slashObject.SetActive(false);
    }

    private void Update()
    {
        GetAttackInput();
    }

    private void GetAttackInput()
    {
        timeCounter += Time.deltaTime;
        if(playerInput.Attacked)
        {
            if (timeCounter >= timeBetweenAttacks && !playerController.collisionInfo.isStickedToWall)
            {
                timeCounter = 0;
                StartCoroutine(Slash());
            }
        }
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
