using UnityEngine;

public class Arsenal : MonoBehaviour
{
    public GameObject ArsenalPanel;     // Панель с вооружением
    RectTransform ArsenalTransform;     // её позиционирование
    public bool reposition = true;      // флаг для позиционирования панели
    public float Speed;                 // !! 1000 должна без остатка делиться на скорость!!    

    private static Arsenal _instance;
    public static Arsenal Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Start()
    {
        _instance = this;

        ArsenalTransform = ArsenalPanel.GetComponent<RectTransform>();
    }
    //
    // Выдвижение/Задвижение панели осуществляется посредством нажатия на кнопку "Вооружение" (ArsenalPanelReposition)
    //
    private void FixedUpdate()
    {
        if (reposition)
        {
            if (Mathf.Abs(ArsenalTransform.offsetMax.y) < 1000f)
            {
                ArsenalTransform.offsetMin = new Vector2(ArsenalTransform.offsetMin.x, ArsenalTransform.offsetMin.y - Speed);
                ArsenalTransform.offsetMax = new Vector2(ArsenalTransform.offsetMax.x, ArsenalTransform.offsetMax.y - Speed);
            }
        }
        else if (!reposition)
        {
            if (Mathf.Abs(ArsenalTransform.offsetMax.y) > 0f)
            {
                ArsenalTransform.offsetMin = new Vector2(ArsenalTransform.offsetMin.x, ArsenalTransform.offsetMin.y + Speed);
                ArsenalTransform.offsetMax = new Vector2(ArsenalTransform.offsetMax.x, ArsenalTransform.offsetMax.y + Speed);
            }
        }
    }

    public void ArsenalPanelReposition()
    {
        reposition = reposition ? false : true;
    }
}
