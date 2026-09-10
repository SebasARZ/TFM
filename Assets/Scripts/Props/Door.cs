using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] int orvesRequired = 3;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

    }
    private void OnEnable()
    {
        OrveCounter.OnCountChanged += CheckOpen;
    }

    private void OnDisable()
    {
        OrveCounter.OnCountChanged -= CheckOpen;
    }

    private void CheckOpen(int currentTotal)
    {
        if (currentTotal >= orvesRequired)
        {
            Open();
        }
    }

    private void Open()
    {
        Debug.Log(name + " abierta");
        anim.SetBool("canOpen", true);

        // ejemplo: la puerta desaparece
        // o animator.SetTrigger("Open");
        // o transform de la puerta subiendo, etc.
    }
}