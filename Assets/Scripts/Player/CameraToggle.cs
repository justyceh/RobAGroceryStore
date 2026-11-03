using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Switches between first-person and third-person views when the ToggleView key (V) is pressed.
// Cinemachine always shows the CinemachineCamera with the highest Priority and blends smoothly
// when that changes, so all this script does is swap two numbers.
public class CameraToggle : MonoBehaviour
{
    [Tooltip("The ToggleView action from InputSystem_Actions (Player/ToggleView), bound to the V key.")]
    [SerializeField] private InputActionReference toggleAction;

    [Tooltip("The first-person Cinemachine Camera (FirstPersonCam in the Hierarchy).")]
    [SerializeField] private CinemachineCamera firstPersonCam;

    [Tooltip("The third-person over-the-shoulder Cinemachine Camera (ThirdPersonCam in the Hierarchy).")]
    [SerializeField] private CinemachineCamera thirdPersonCam;

    [Tooltip("Tick to start the game in first person, untick to start in third person.")]
    [SerializeField] private bool startInFirstPerson = true;

    // The priority given to whichever camera should be live. Any number above the other camera's works.
    private const int ActivePriority = 10;
    private const int InactivePriority = 0;

    private bool isFirstPerson;

    private void OnEnable()
    {
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleAction.action.Disable();
    }

    // Start runs once, just before the first Update, after every object's Awake has run.
    // Setting priorities here means the Inspector's Priority values don't matter.
    private void Start()
    {
        isFirstPerson = startInFirstPerson;
        ApplyView();
    }

    private void Update()
    {
        // WasPressedThisFrame is true only on the frame the key goes down,
        // so holding V doesn't flip the view back and forth every frame.
        if (toggleAction.action.WasPressedThisFrame())
        {
            isFirstPerson = !isFirstPerson;
            ApplyView();
        }
    }

    private void ApplyView()
    {
        firstPersonCam.Priority = isFirstPerson ? ActivePriority : InactivePriority;
        thirdPersonCam.Priority = isFirstPerson ? InactivePriority : ActivePriority;
    }
}
