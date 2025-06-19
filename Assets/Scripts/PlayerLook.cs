using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float xRotation = 0f;

    // Плавность поворота (чем меньше — тем сильнее сглаживание)
    public float smoothTime = 0.075f;

    private float currentXRotVelocity;
    private float currentYRotVelocity;

    private float targetXRotation;
    private float targetYRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        targetXRotation = 0f;
        targetYRotation = playerBody.eulerAngles.y;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        targetXRotation -= mouseY;
        targetXRotation = Mathf.Clamp(targetXRotation, -80f, 80f);

        targetYRotation += mouseX;

        // Плавный поворот камеры по X (вверх/вниз)
        xRotation = Mathf.SmoothDamp(xRotation, targetXRotation, ref currentXRotVelocity, smoothTime);

        // Плавный поворот игрока по Y (влево/вправо)
        float smoothYRotation = Mathf.SmoothDampAngle(playerBody.eulerAngles.y, targetYRotation, ref currentYRotVelocity, smoothTime);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0f, smoothYRotation, 0f);
    }
}
