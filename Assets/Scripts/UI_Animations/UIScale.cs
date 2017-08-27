using System.Collections;
using UnityEngine;

public class UIScale : UITransitions
{
    /// <summary>
    /// Старт
    /// </summary>
    public override void Play()
    {
        base.Play();
    }
    /// <summary>
    /// Обратное выполнение
    /// </summary>
    public override void PlayReverse()
    {
        base.PlayReverse();
    }
    /// <summary>
    /// Масштабируем изображение по кривульке
    /// </summary>
    /// <param name="fadeIn">Передав true будет реверсное выполнение</param>
    protected override IEnumerator Transition(bool reverse = false)
    {
        float keys = !reverse ? 0f : 1f;    // 1 если реверс (старт с конца)

        float _speed = TransitionTime / Time.deltaTime;

        // если пролаг или фпс ниже 20
        _speed = _speed < 20 ? 20 : _speed;

        while (!reverse ? (keys < AMPLITUDE) : keys > 0)
        {
            thisCanvasRenderer.transform.localScale = new Vector3(Curve.Evaluate(keys), Curve.Evaluate(keys));

            if (!reverse)
                keys += (AMPLITUDE / _speed);
            else
                keys -= (AMPLITUDE / _speed);
            yield return null;
        }

        thisCanvasRenderer.transform.localScale = !reverse ? new Vector3(Curve.keys[Curve.keys.Length -1].value, Curve.keys[Curve.keys.Length - 1].value) :
                                                             new Vector3(Curve.keys[0].value, Curve.keys[0].value);

        if (OnFinished != null)
            OnFinished.Invoke();
    }

    protected override IEnumerator TransitionLoop(bool reverse = false)
    {
        StartCoroutine(base.TransitionLoop(reverse));
        yield return null;
    }
}
