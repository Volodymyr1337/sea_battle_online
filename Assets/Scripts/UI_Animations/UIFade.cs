using System.Collections;
using UnityEngine;

public class UIFade : UITransitions
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
    /// Затемняем изображение по кривульке
    /// </summary>
    /// <param name="reverse">Передав true будет реверсное выполнение</param>
    protected override IEnumerator Transition(bool reverse = false)
    {
        float keys = !reverse ? 0f : 1f;    // 1 если реверс (старт с конца)

        float _speed = TransitionTime / Time.deltaTime;

        // если пролаг или фпс ниже 20
        _speed = _speed < 20 ? 20 : _speed;
        
        while (!reverse ? (keys < AMPLITUDE) : keys > 0)
        {
            thisCanvasRenderer.SetAlpha(Curve.Evaluate(keys));

            if (childCanvasRenderer != null)
                foreach(CanvasRenderer cr in childCanvasRenderer)
                {
                    cr.SetAlpha(Curve.Evaluate(keys));
                }

            if (!reverse)
                keys += (AMPLITUDE / _speed);
            else
                keys -= (AMPLITUDE / _speed);
            yield return null;
        }

        thisCanvasRenderer.SetAlpha(!reverse ? 1f : 0f);

        if (childCanvasRenderer != null)
            foreach (CanvasRenderer cr in childCanvasRenderer)
            {
                cr.SetAlpha(!reverse ? 1f : 0f);
            }

        if (OnFinished != null)
            OnFinished.Invoke();
    }

    protected override IEnumerator TransitionLoop(bool reverse = false)
    {
        StartCoroutine(base.TransitionLoop(reverse));
        yield return null;
    }
}
