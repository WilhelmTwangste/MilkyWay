using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public int index;           // Порядковый номер диалога
    public string characterName;// Имя персонажа
    [TextArea(3, 10)]
    public string text;         // Текст диалога
    public string background;   // Имя спрайта фона
    public string characterSprite; // Имя спрайта персонажа
    public string characterPosition;
    public bool hasChoices;     // Есть ли выборы
    public Choice[] choices;    // Варианты выбора
}

[System.Serializable]
public class Choice
{
    public string choiceText;   // Текст варианта
    public int branchIndex;    // Индекс начала ветки
}