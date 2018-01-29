using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CatEyeMove : MonoBehaviour {

    [SerializeField]
    GameObject Eyeball;

    public void EyeMove()
    {
        StartCoroutine(StartMove());
    }

    public void StopEyeMove()
    {
        StartCoroutine(StartMove());
        Eyeball.transform.DOLocalMoveX(0f, 1f);
    }

    IEnumerator StartMove()
    {
        Eyeball.transform.DOLocalMoveX(-10f, 1f);
        yield return new WaitForSeconds(1.5f);
        Eyeball.transform.DOLocalMoveX(5f, 1f);
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(StartMove());
    }
	
}
