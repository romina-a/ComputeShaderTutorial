using System;
using UnityEngine;

public class ActingContainer : MonoBehaviour
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
    [SerializeField]
    float waterHeight = 0; //todo: apply changes to container position based on height
    [SerializeField]
    float density = 1f;
    [SerializeField, Range(1, 10000)]
    int acceleration_correction = 1000;

    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        velocitiesId = Shader.PropertyToID("_Velocities"),
        relativePositionsId = Shader.PropertyToID("_RelativePositions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        etimeId = Shader.PropertyToID("_Etime"),
        dtimeId = Shader.PropertyToID("_Dtime");

    static int ReactKernelIndex, InitKernelIndex;

    float step, elapsed_time, delta_time;

    ComputeBuffer positionsBuffer;
    ComputeBuffer relativePositionsBuffer;
    ComputeBuffer velocitiesBuffer;

    Vector3 acceleration, velocity, pos, normal;

    private void InitMovementVariables()
    {
        pos = transform.position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        normal = -gravity;
    }

    private void UpdateVariables(float deltaTime)
    {
        Vector3 newPos = transform.position;
        Vector3 newV = (newPos - pos) / deltaTime;
        acceleration = (newV - velocity) / deltaTime;
        if (acceleration == Vector3.zero)
        {
            elapsed_time += deltaTime;
        }
        else
        {
            elapsed_time = 0;
        }
        velocity = newV;
        pos = newPos;
    }

    private void OnEnable()
    {
        step = 2f / resolution;
        elapsed_time = 0;

        positionsBuffer = new ComputeBuffer(count: maxResolution * maxResolution, 3 * 4);// the readwrite buffer there
        velocitiesBuffer = new ComputeBuffer(count: maxResolution * maxResolution, 3 * 4);// the readwrite buffer there
        relativePositionsBuffer = new ComputeBuffer(count: maxResolution * maxResolution, 3 * 4);// the readwrite buffer there

        ReactKernelIndex = computeShader.FindKernel("ReactKernel");
        InitKernelIndex = computeShader.FindKernel("InitKernel");

        // set the constants
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);

        //to find the id of the kernel you can use computeShader.FindKernel("FunctionKernel") which here will be 0 so we added 0
        //why do we determine the kernel? can't other kernel access this?
        computeShader.SetBuffer(ReactKernelIndex, positionsId, positionsBuffer);
        computeShader.SetBuffer(ReactKernelIndex, velocitiesId, velocitiesBuffer);
        computeShader.SetBuffer(ReactKernelIndex, relativePositionsId, relativePositionsBuffer);

        computeShader.SetBuffer(InitKernelIndex, positionsId, positionsBuffer);
        computeShader.SetBuffer(InitKernelIndex, velocitiesId, velocitiesBuffer);
        computeShader.SetBuffer(InitKernelIndex, relativePositionsId, relativePositionsBuffer);

        // we give the same buffer to the material to render the squares
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        InitMovementVariables();

        int groups = Mathf.CeilToInt(resolution / 1f);
        computeShader.Dispatch(InitKernelIndex, groups, groups, 1);
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        velocitiesBuffer.Release();
        relativePositionsBuffer.Release();
        positionsBuffer = null;
        velocitiesBuffer = null;
        relativePositionsBuffer = null;
    }

    private void Update()
    {
        UpdateVariables(Time.deltaTime);
        UpdateFunctionOnGPU();
    }

    void UpdateFunctionOnGPU()
    {
        computeShader.SetFloats("_WaterPosition", new float[3] { transform.position.x, transform.position.y, transform.position.z });

        Vector3 force = density * ((acceleration / (acceleration_correction+1)) - gravity);
        normal.x = Mathf.Lerp(normal.x, force.x, Time.deltaTime);
        normal.y = Mathf.Lerp(normal.y, force.y, Time.deltaTime);
        normal.z = Mathf.Lerp(normal.z, force.z, Time.deltaTime);
        float[] normalarr = new float[3] { force.x, force.y, force.z };
        float[] gravityarr = new float[3] { gravity.x, gravity.y, gravity.z };

        Debug.Log("normal is : sdfsdf" + force.x + ","+ force.y+","+force.z);
        Debug.DrawLine(transform.position, transform.position + 30 * force);

        computeShader.SetFloat(etimeId, elapsed_time);
        computeShader.SetFloat(dtimeId, Time.deltaTime);
        computeShader.SetFloats("_WaterNormal", normalarr);
        computeShader.SetFloats("_GravityNormal", gravityarr);

        int groups = Mathf.CeilToInt(resolution / 1f);
        computeShader.Dispatch(ReactKernelIndex, groups, groups, 1);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

}
