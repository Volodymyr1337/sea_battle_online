using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITransitions : MonoBehaviour
{
    /// <summary>
    /// Расстояние от первой до конечной точки в графике
    /// </summary>
    protected const float AMPLITUDE = 1f;

    [SerializeField] protected AnimationCurve Curve;
    [SerializeField] protected float TransitionTime;            // время проигрывания анимации
    [SerializeField] protected bool PlayOnAwake = false;        // Старт при инициализации

    [SerializeField] protected AnimationTypes animationType;

    /// <summary>
    /// События выполняющиеся по окончанию анимации
    /// </summary>
    public Button.ButtonClickedEvent OnFinished;

    protected CanvasRenderer thisCanvasRenderer;                // cr на объекте на который вешаем
    protected CanvasRenderer[] childCanvasRenderer;             // cr на дочерних если есть

        
    protected virtual void Start()
    {
        thisCanvasRenderer = GetComponent<CanvasRenderer>();
        if (thisCanvasRenderer.transform.childCount > 0)
            childCanvasRenderer = GetComponentsInChildren<CanvasRenderer>();

        if (PlayOnAwake)
            Play();
    }

    public virtual void Play()
    {
        if (animationType == AnimationTypes.Once)
            StartCoroutine(Transition());
        else
            StartCoroutine(TransitionLoop());
    }

    public virtual void PlayReverse()
    {
        if (animationType == AnimationTypes.Once)
            StartCoroutine(Transition(true));
        else
            StartCoroutine(TransitionLoop(true));
    }

    protected virtual IEnumerator Transition(bool transition = false)
    {
        yield return null;
    }
    /// <summary>
    /// Проигрывает анимацию в зависимости от выбранного перехода Loop или Ping Pong'а
    /// </summary>
    /// <param name="transition">false если реверсная анимация</param>
    /// <returns></returns>
    protected virtual IEnumerator TransitionLoop(bool transition = false)
    {
        while (true)
        {
            yield return StartCoroutine(Transition(transition));

            if (animationType == AnimationTypes.PingPong)
                transition = !transition;

            yield return null;
        }
    }

    [ContextMenu("Scale tester")]
    public void TestAnim()
    {
        Play();
    }

}
/// <summary>
/// Типы проигрывания анимации (одиночный, циклический, пинг-понг)
/// </summary>
public enum AnimationTypes
{
    Once,
    Loop,
    PingPong
}
