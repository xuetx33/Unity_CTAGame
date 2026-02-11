using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class JumpArea : MonoBehaviour
{
    public Transform jumpPointA;
    public Transform jumpPointB;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Luna"))
        {
            LunaController LuNaController = collision.transform.GetComponent<LunaController>();
            Transform targetTrans= Vector3.Distance(LuNaController.transform.position, jumpPointA.position)
                > Vector3.Distance(LuNaController.transform.position, jumpPointB.position)?
                jumpPointA:jumpPointB;
            LuNaController.Jump(true);
            Sequence sequence = DOTween.Sequence();
            LuNaController.transform.DOMove(targetTrans.position, 0.5f).
                SetEase(Ease.Linear).OnComplete(() => { LuNaController.Jump(false); });
            Transform lunaLocalTrans= LuNaController.transform.GetChild(0);
            sequence.Append(lunaLocalTrans.DOLocalMoveY(1.5f,0.25f).SetEase(Ease.InOutSine));
            sequence.Append(lunaLocalTrans.DOLocalMoveY(0.547f, 0.25f).SetEase(Ease.InOutSine));
            sequence.Play();
        }
    }
}
