using UnityEngine;

public class CampfireSeat : MonoBehaviour
{
    public Transform forwardView;
    public Transform leftView;
    public Transform rightView;
    public Transform backView;


    public float lookSpeed = 5f;

    // The transform the camera lerps toward
    private Transform targetView;

    void Start()
    {
        // Start at the forward view
        targetView = forwardView;
        SnapToView(forwardView);
    }

    void Update()
    {
        HandleInput();

        // Smoothly move toward target view
        transform.position = Vector3.Lerp(
            transform.position,
            targetView.position,
            Time.deltaTime * lookSpeed
        );

        // Smoothly rotate toward target view
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetView.rotation,
            Time.deltaTime * lookSpeed
        );
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            targetView = forwardView;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            targetView = leftView;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            targetView = rightView;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            targetView = backView;
        }
    }

    // Instantly sets camera to a view
    private void SnapToView(Transform view)
    {
        transform.position = view.position;
        transform.rotation = view.rotation;
    }
}