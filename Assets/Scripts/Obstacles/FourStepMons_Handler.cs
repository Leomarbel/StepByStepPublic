using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourStepMons_Handler : AObstacles
{
[Header("Transform References")]

    //[SerializeField] private GameOverHandler _gameOverFs;
    [SerializeField] private Transform initialState;
    [SerializeField] private Transform finalState;
    [SerializeField] private GameObject obs;
    [SerializeField] private float appearDuration = 0.75f;


    [Header("Statistics")]
    [SerializeField] private int minStuckPresses;
    [SerializeField] private int maxStuckPresses;

    void Awake()
    {
        initialState.position = obs.transform.position;
    }
    public override void OnPlayerHit(PlayerController player)
    {
        Debug.Log("hit");
        player.Stuck(minStuckPresses, maxStuckPresses);
    }

    public override void Appear()
    {
        StartCoroutine(AppearAnimation());
    }

    IEnumerator AppearAnimation()
    {
        Vector3 startPos = initialState.position;
        Vector3 endPos = finalState.position;       //must cache first else lerping will not work

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / appearDuration;
            float smoothT = t * t * (3f - 2f * t);
            obs.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }
        obs.transform.position = endPos;
    }

}
