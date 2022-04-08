using UnityEngine;

public class GPUGraph : MonoBehaviour
{

    [SerializeField]
    Material material;
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    ComputeShader computeShader;

    const int maxResolution = 1000;

    [SerializeField, Range(10,maxResolution)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    [SerializeField, Min(0f)]
    float functionDuration = 1f , transitionDuration = 1f;

    public enum TransitionMode { Cycle, Random }

    [SerializeField]
    TransitionMode transitionMode;

    // Each name has an id in every run. you can test with getting propertytoid for any random word! weird!
    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress");


    bool transitioning;
    float duration;
    FunctionLibrary.FunctionName transitionFunction;

    ComputeBuffer positionsBuffer;// to save the position of the cubes on GPU


    /* Changed this from Awake to OnEnable because the elements would
     * dissapear after each hot reload so if you change things in play mode,
     * the buffer would dissapear if it was created in Awake, 
     * but onEnable is called after hot reload, so the elements would be created again*/
    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(count: maxResolution * maxResolution, 3 * 4);
        /* number of elements, the size of each element in bytes.
         * for us each element is a position with three floats */
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        /* Release the GPU memory when the object is disabled (which also happens before hot reloads)
         * I don't know if we don't do this the GPU will get over full, or there is some sort of garbage
         * collection in the GPU, but now we are sure that the memroy is released.*/
        positionsBuffer = null; 
        /* We release the corresponding C# object (on CPU?) so Unity Garbage collectors will get rid of it*/
        /* If you don't release the buffer but make it null, 
         * when the garbage collectors collect the object the memory will be released
         * what if we do nothing in OnDisable? I have no idea what would happen*/
    }

    private void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitionFunction = function;
            PickNextFunction();
            transitioning = true;
        }
        UpdateFunctionOnGPU();
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(
                transitionProgressId,
                Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
            );
        }
        var kernelIndex =
            (int)function + (int)(transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer); //to find the id of the kernel you can use computeShader.FindKernel("FunctionKernel") which here will be 0 so we added 0

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution*resolution);
        /*The first is the kernel index and the other three are the amount of groups to run,
         * again split per dimension. Using 1 for all dimensions would mean
         * only the first group of 8×8 positions gets calculated.
         * 
         */
    }

}
