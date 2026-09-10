using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class AnimationAndMovementController : MonoBehaviour
{

    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRuningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    float rotationFactorPerFrame = 15.0f;
    float runMultiplier = 4f;
    float gravity = -9.8f;
    float groundedGravity = -.05f;
   

    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 3.0f;
    float maxJumpTime = 0.75f;
    bool isJumping = false;
    int isJumpingHash;
    bool isJumpingAnimating = false;
    int jumpCount = 0;
    int jumpCountHash;

    [Header("Health")]
    [SerializeField] int health = 100;
    [SerializeField] float extraDelayAfterAnimation = 1.5f; // tiempo extra tras terminar la animación
    [SerializeField] string deathStateName = "Dead";       // nombre EXACTO del estado en el Animator
    bool isDead = false;
    int isDeadHash;


    [Header("Hit Feedback")]
    [SerializeField] Color hitEmission = Color.red;
    [SerializeField] float emissionIntensity = 4f; // sube esto si quieres más brillo

    [SerializeField] Color deathEmission = Color.black;
    [SerializeField] float deathEmissionIntensity = 1f; // sube esto si quieres más brillo al morir]
    [SerializeField] float blinkDuration = 0.1f;
    Renderer[] renderers;
    Color[] defaultEmissions;
    Coroutine blinkRoutine;

    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;

    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
       

        renderers = GetComponentsInChildren<Renderer>();
        defaultEmissions = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.EnableKeyword("_EMISSION");
            defaultEmissions[i] = renderers[i].material.GetColor("_EmissiveColor");
            Debug.Log("Renderer encontrado: " + renderers[i].gameObject.name + " | Shader: " + renderers[i].material.shader.name);
        }

        isWalkingHash = Animator.StringToHash("isWalking");
        isRuningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isDeadHash = Animator.StringToHash("isDead");

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += OnRun;
        playerInput.CharacterControls.Run.canceled += OnRun;
        playerInput.CharacterControls.Jump.started += OnJump;
        playerInput.CharacterControls.Jump.canceled += OnJump;

        setupJumpVariables();
    }

    void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (maxJumpHeight * 1.5f)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight * 1.5f)) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * (maxJumpHeight * 2f)) / Mathf.Pow((timeToApex * 1.5f), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight * 2f)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);

    }
    void handleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            if (jumpCount < 3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
            }
            animator.SetBool(isJumpingHash, true);
            isJumpingAnimating = true;
            isJumping = true;
            jumpCount += 1;

            if (jumpCount > 3)
            {
                jumpCount = 3;
            }

            animator.SetInteger(jumpCountHash, jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount] * 0.5f;
            currentRunMovement.y = initialJumpVelocities[jumpCount] * 0.5f;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }
    IEnumerator jumpResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        jumpCount = 0;

    }


    void OnJump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
    }

    void OnRun(InputAction.CallbackContext ctx)
    {
        isRunPressed = ctx.ReadValueAsButton();
    }
    void onMovementInput(InputAction.CallbackContext ctx)
    {
        currentMovementInput = ctx.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    void handleRotation()
    {
        Vector3 positionToLookAt;
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;
        Quaternion currentRotation = transform.rotation;
        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * rotationFactorPerFrame);
        }
    }
    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRuningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRuningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRuningHash, false);
        }
    }

    void handleGravity()
    {

        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;
        if (characterController.isGrounded)
        {
            if (isJumpingAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                isJumpingAnimating = false;
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if (jumpCountHash >= 3)
                {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else if (isFalling)
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -30f);
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;

        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log("Golpe recibido. Health: " + health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log("Parpadeo por golpe");   // <-- si esto aparece, el feedback SÍ se dispara
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(BlinkEffect());
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool(isDeadHash, true);

        // Cancela el parpadeo de golpe si estaba activo, para que no restaure el color a media muerte
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Aplica el color de muerte y lo deja fijo (no se restaura)
        Color emissive = deathEmission * deathEmissionIntensity;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_EmissiveColor", emissive);
        }

        StartCoroutine(RestartScene());
        Debug.Log("Die");
    }

    IEnumerator RestartScene()
    {
        Debug.Log("1");
        // Espera un frame para que el Animator entre en la transición hacia el estado de muerte
        yield return null;
        
        Debug.Log("2");
        // Espera a que el estado de muerte esté realmente reproduciéndose
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathStateName))
        {
            Debug.Log("2");
            yield return null;
        }
        Debug.Log("3");
        // Espera a que la animación llegue al final (normalizedTime >= 1)
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Tiempo extra una vez terminada la animación
        yield return new WaitForSeconds(extraDelayAfterAnimation);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    void Update()
    {
        if (isDead)
        {
            // Sigue aplicando gravedad para que el cuerpo caiga al suelo si hace falta
            characterController.Move(currentMovement * Time.deltaTime);
            handleGravity();
          
            return;
        }

        handleRotation();
        handleAnimation();
        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
        handleGravity();
        handleJump();
       
    }
    /*void LockZPosition()
    {
        Vector3 pos = transform.position;
        if (pos.z != lockedZ)
        {
            pos.z = lockedZ;
            transform.position = pos;
        }
    }*/

    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }
    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
    IEnumerator BlinkEffect()
    {
        
        Color emissive = hitEmission * emissionIntensity;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_EmissiveColor", emissive);
        }

        yield return new WaitForSeconds(blinkDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_EmissiveColor", defaultEmissions[i]);
        }

        blinkRoutine = null;
    }
}