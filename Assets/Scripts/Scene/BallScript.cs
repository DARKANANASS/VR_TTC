using System;
using UnityEngine;
using UnityEngine.UIElements;

public class BallScript : MonoBehaviour /*MonoBehaviour is a base class that many Unity scripts derive from*/
{
    private bool stoped;
    private Vector3[] Points;
    private int currentIndex = 0;
    private float speed;
    public Action Finish;

    void Start()
    {
    }

    void FixedUpdate()
    {
        if (!stoped)
        {
            Move();
        }
    }

    private void Move()
    {
        if (currentIndex <= Points.Length - 1)
        {
            var targPoint = Points[currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, targPoint, speed);
            //старый метод
            //transform.position = Vector3.MoveTowards(transform.position, targPoint, speed * Time.fixedDeltaTime);
            if (transform.position == targPoint)
            {
                currentIndex++;
                if (targPoint == Points[Points.Length - 1])
                {
                    Finish?.Invoke();
                }
            }
        }
        else
        {
            transform.position += GetDir(currentIndex) * speed * Time.deltaTime;
        }
    }

    private Vector3 GetDir(int ind)
    {
        var max = Points.Length - 1;
        if (ind >= max) return (Points[max] - Points[max - 1]).normalized;

        return (Points[ind + 1] - Points[ind]).normalized;
    }


    public void SetParams(Vector3[] newPoints, float newSpeed)
    {
        Points = newPoints;
        speed = newSpeed;
        stoped = false;
    }

    public void Stop()
    {
        stoped = true;
    }
}
