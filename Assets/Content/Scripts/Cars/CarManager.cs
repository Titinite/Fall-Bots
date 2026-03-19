using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance
    {
        get; private set;
    }

    public float speed = 6f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
