using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleRepositionManager : MonoBehaviour
{
    [SerializeField] private GameObject obstacle;
    [SerializeField] private Vector2 sampleSize;

    [SerializeField] private float minRadius;
    [SerializeField] private float maxRadius;

    [SerializeField] private float yPosAbove;
    [SerializeField] private float yPosDown;

    [SerializeField] private float obstacleTimeInterval;

    List<GameObject> obstacleList;
    void Start()
    {
        obstacleList = new List<GameObject>();
        GeneralFunctions.AddTimer(RedoPosition, 3f);
    }

    void RedoPosition()
    {
        ObstacleLeaveAnimation();
        GeneralFunctions.AddTimer(RedoPosition, obstacleTimeInterval);
    }

    void ObstacleReposition()
    {
        //Set new randon range for obstacles
        Random.InitState(System.DateTime.Now.Millisecond);
        float radius = Random.Range(minRadius, maxRadius);

        List<Vector2> obstaclePositions = PoissonDiscSampling.GeneratePoints(radius, sampleSize);
        foreach (Vector2 pos in obstaclePositions)
        {
            float chance = Mathf.RoundToInt(Random.value);
            if (chance == 0)
                continue;

            //Create new obtacle
            float rotation = Random.Range(0, 360);
            GameObject obstac = GameObject.Instantiate(obstacle, new Vector3(pos.x, -10f, pos.y), Quaternion.Euler(new Vector3(-90, rotation, 0)));
            obstac.transform.parent = this.transform;
            obstacleList.Add(obstac);
        }

        ObstacleAppearAnimation();
    }

    void ClearArena()
    {
        if (obstacleList.Count == 0)
            goto Reset;

        foreach (GameObject obj in obstacleList)
        {
            Destroy(obj);
        }

        Reset:
        obstacleList = new List<GameObject>();
        this.transform.position = new Vector3(0, 0, 0);
        ObstacleReposition();
    }

    public void ObstacleLeaveAnimation() => GeneralFunctions.AddProgressiveTimer(ClearArena, delegate (float timer) { SmoothPosYChange(yPosDown); }, 1f);

    void ObstacleAppearAnimation() => GeneralFunctions.AddProgressiveTimer(null, delegate (float timer) { SmoothPosYChange(yPosAbove); }, 1f);

    void SmoothPosYChange(float yPos) => transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, yPos, Time.deltaTime), transform.position.z);
}