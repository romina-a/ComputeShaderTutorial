using UnityEngine;

public class Container : MonoBehaviour
{

    [SerializeField]
    Material material;
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    ComputeShader computeShader;

    const int maxResolution = 1000;

    [SerializeField, Range(10, maxResolution)]
    int resolution = 10;

    [SerializeField]
    Vector3 gravity = new Vector3(0, -10, 0);

    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        containerTransformMatrixId = Shader.PropertyToID("_ContainerTransformMatrix"),
        containerPositionId = Shader.PropertyToID("_ContainerPosition");

    static readonly int kernelIndex = 0;

    float step;

    ComputeBuffer positionsBuffer;

    Vector3 a, v, pos;

    private void InitMovementVariables()
    {
        pos = transform.position;
        v = Vector3.zero;
        a = Vector3.zero;
    } 

    private void UpdateVariables(float deltaTime)
    {
        Vector3 newPos = transform.position;
        Vector3 newV = (newPos - pos) / deltaTime;
        a = (newV - v) / deltaTime;
        v = newV;
        pos = newPos;
    }

    private void OnEnable()
    {
        step = 2f / resolution;

        positionsBuffer = new ComputeBuffer(count: maxResolution * maxResolution, 3 * 4);// the readwrite buffer there

        // set the constants
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        //to find the id of the kernel you can use computeShader.FindKernel("FunctionKernel") which here will be 0 so we added 0
        //why do we determine the kernel? can't other kernel access this?
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer); 
        
        // we give the same buffer to the material to render the squares
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        InitMovementVariables();
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        UpdateVariables(Time.deltaTime);
        UpdateFunctionOnGPU();
    }

    void UpdateFunctionOnGPU()
    {
        computeShader.SetFloat(timeId, Time.time);
        //material.SetMatrix(containerTransformId, transform.localToWorldMatrix);
        material.SetMatrix(containerTransformMatrixId, transform.localToWorldMatrix);
        material.SetVector(containerPositionId, transform.position);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

}
