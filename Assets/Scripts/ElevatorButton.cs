using UnityEngine;

public class ElevatorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private Elevator elevator;
    [SerializeField] private string targetScene;
    [SerializeField] private string prompt = "Go up";

    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private Color emissionColor = Color.white;
    [SerializeField] private float litIntensity = 3f;
    [SerializeField] private float unlitIntensity = 0f;

    private Material _mat;
    public string TargetScene => targetScene;
    public string InteractPrompt => prompt;

    public void Interact()
    {
        Debug.Log($"{name} → GoToFloor({targetScene})");
        elevator.GoToFloor(targetScene);
    }

    void Awake()
    {
        _mat = buttonRenderer.material;      // instance, not shared
        _mat.EnableKeyword("_EMISSION");
        SetLit(false);
    }

    public void SetLit(bool lit)
    {
        float intensity = lit ? litIntensity : unlitIntensity;
        _mat.SetColor("_EmissionColor", emissionColor * intensity);
    }
}