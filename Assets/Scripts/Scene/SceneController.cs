using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject endPrefab;
    [SerializeField] private InputListener input;
    private BallScript targetBallSkript;
    private GameObject targetBall;
    private GameObject targetEnd;
    [SerializeField] private LineRenderer traectory;
    [SerializeField] private Timing timing;

    public void InstantBall(float speed, Vector3[] vertices)
    {
        targetBall = Instantiate(ballPrefab, vertices[0], ballPrefab.transform.rotation);
        targetBallSkript = targetBall.GetComponent<BallScript>();
        targetBallSkript.SetParams(vertices, speed);
    }

    public void InstantLine(Vector3[] vertices)
    {
        traectory.positionCount = vertices.Length;
        traectory.SetPositions(vertices);
    }

    public void BallMesh(bool val)
    {
        targetBall.GetComponent<MeshRenderer>().enabled = val;
    }

    public void ShowEndPos(Vector3 endPos)
    {
        targetEnd = Instantiate(endPrefab, endPos, endPrefab.transform.rotation);
    }

    public void DropAll()
    {
        traectory.positionCount = 1;
        Destroy(targetBall);
        Destroy(targetEnd);
    }

    public void StopBall()
    {
        targetBallSkript.Stop();
    }

    public void ClearScene()
    {
        DropAll();
    }

    public BallScript GetBall()
    {
        return targetBallSkript;
    }

}
