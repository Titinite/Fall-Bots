using UnityEngine;

public class CarManager : MonoBehaviour
{
    public static CarManager Instance
    {
        get; private set;
    }

    public float speed = 6f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        
    }
}
