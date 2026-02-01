using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonHandler : MonoBehaviour
{
    public UnityAction onClickAction;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            Debug.LogWarning($"Added Button component to {gameObject.name}");
        }

        button.onClick.AddListener(() => onClickAction?.Invoke());
    }

    public void SetClickAction(UnityAction action)
    {
        onClickAction = action;
    }
}