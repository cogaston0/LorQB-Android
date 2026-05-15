using UnityEngine;

public class LorQBSceneBuilder : MonoBehaviour
{
    void Start()
    {
        BuildScene();
        Camera.main.transform.position = new Vector3(0f, 3f, -4f);
        Camera.main.transform.LookAt(new Vector3(0f, 0.5f, 0f));
    }

    void BuildScene()
    {
        CreateCube("Cube_Blue", new Vector3(0.51f, 0.5f, 0.51f), new Color(0f, 0f, 1f, 0.5f));
        CreateCube("Cube_Red", new Vector3(0.51f, 0.5f, -0.51f), new Color(1f, 0f, 0f, 0.5f));
        CreateCube("Cube_Green", new Vector3(-0.51f, 0.5f, -0.51f), new Color(0f, 1f, 0f, 0.5f));
        CreateCube("Cube_Yellow", new Vector3(-0.51f, 0.5f, 0.51f), new Color(1f, 1f, 0f, 0.5f));

        CreateHinge("Hinge_Blue_Red", new Vector3(0.51f, 1f, 0f));
        CreateHinge("Hinge_Red_Green", new Vector3(0f, 1f, -0.51f));
        CreateHinge("Hinge_Green_Yellow", new Vector3(-0.51f, 1f, 0f));

        CreateBall(new Vector3(0.51f, 0.25f, 0.51f));

        Debug.Log("LorQB Scene Built.");
    }

    void CreateCube(string cubeName, Vector3 pos, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = cubeName;
        cube.transform.position = pos;
        Renderer rend = cube.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        rend.material = mat;
    }

    void CreateHinge(string hingeName, Vector3 pos)
    {
        GameObject hinge = new GameObject(hingeName);
        hinge.transform.position = pos;
    }

    void CreateBall(Vector3 pos)
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.position = pos;
        ball.transform.localScale = Vector3.one * 0.5f;
    }
}
