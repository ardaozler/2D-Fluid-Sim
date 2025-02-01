using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FluidSimulation : MonoBehaviour
{
    public GameObject drop;

    [Header("Start params")] public int count;
    public Vector2 startPos;
    public Vector2 startOffset;

    [Header("Physical params")] public float gravity;
    public float viscosity;

    [Header("Drop interaction params")] public float influenceRadius;
    public float pushForce;
    public AnimationCurve pushForceCurve;

    private GameObject[] _drops;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _drops = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            var randSpawnPoint = startPos +
                                 new Vector2(startOffset.x * Random.Range(-1f, 1f),
                                     startOffset.y * Random.Range(-1f, 1f));
            var dropInstance = Instantiate(drop, randSpawnPoint, Quaternion.identity);
            _drops[i] = dropInstance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Run2DSimulation();
    }

    public void Run2DSimulation()
    {
        for (var index = 0; index < _drops.Length; index++)
        {
            var drop = _drops[index];
            
        }
    }
}