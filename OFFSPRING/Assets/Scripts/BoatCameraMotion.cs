using UnityEngine;

public class BoatCameraMotion : MonoBehaviour
{
    [Header("Oscilaci�n Vertical (oleaje)")]
    public float bobbingAmplitude = 0.5f;
    public float bobbingFrequency = 0.5f;

    [Header("Balanceo (rotaci�n lateral)")]
    public float rollAmplitude = 2f;
    public float rollFrequency = 0.3f;

    [Header("Cabeceo (rotaci�n hacia adelante y atr�s)")]
    public float pitchAmplitude = 1.5f;
    public float pitchFrequency = 0.4f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float time = Time.time;

        // Movimiento vertical (bobbing)
        float newY = initialPosition.y + Mathf.Sin(time * bobbingFrequency * 2 * Mathf.PI) * bobbingAmplitude;

        // Rotaci�n de balanceo (Z)
        float roll = Mathf.Sin(time * rollFrequency * 2 * Mathf.PI) * rollAmplitude;

        // Rotaci�n de cabeceo (X)
        float pitch = Mathf.Sin(time * pitchFrequency * 2 * Mathf.PI) * pitchAmplitude;

        // Aplicar posici�n
        transform.localPosition = new Vector3(initialPosition.x, newY, initialPosition.z);

        // Aplicar rotaci�n
        Quaternion targetRotation = Quaternion.Euler(pitch, 0f, roll);
        transform.localRotation = initialRotation * targetRotation;
    }
}