using UnityEngine;

public class test_speed : MonoBehaviour
{
    public GameObject target;
    public float speed;
    public float timer=0;
    public float curtime=2100;
    public float length=4.2f;
    void Start()
    {
        timer = 0;
    }

    void FixedUpdate()
    {
        speed = (length / curtime) * 2*10;
        if (transform.position != target.transform.position)
        {
            timer += Time.deltaTime;
        }
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed);
    }
}
