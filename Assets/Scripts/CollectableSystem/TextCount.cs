using UnityEngine;
using TMPro;

public class TextCount : MonoBehaviour
{
    private TextMeshProUGUI text;
    int count = 0;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        Orve.OnOrveCollected += UpdateCount;
    }
    private void OnDisable()
    {
        Orve.OnOrveCollected -= UpdateCount;
    }
    private void UpdateCount()
    {
        count++;
        text.text = count.ToString();
    }
}
