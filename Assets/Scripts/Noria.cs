using UnityEngine;

/// <summary>
/// Rota un cilindro continuamente en el eje Y para crear un efecto de paisaje giratorio.
/// </summary>
public class Noria : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Tooltip("Si está activado, la rotación se invierte (gira en sentido contrario)")]
    [SerializeField] private bool reverseRotation = false;
    
    [Tooltip("Si está activado, la rotación se pausa")]
    [SerializeField] private bool pauseRotation = false;

    private void Update()
    {
        if (pauseRotation)
            return;

        // Calcular la rotación para este frame
        float rotationAmount = rotationSpeed * Time.deltaTime;
        
        // Invertir si es necesario
        if (reverseRotation)
            rotationAmount = -rotationAmount;
        
        // Rotar en el eje Y (vertical)
        transform.Rotate(0f, rotationAmount, 0f, Space.Self);
    }

    /// <summary>
    /// Establece la velocidad de rotación.
    /// </summary>
    /// <param name="speed">Velocidad en grados por segundo</param>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// Obtiene la velocidad de rotación actual.
    /// </summary>
    public float GetRotationSpeed()
    {
        return rotationSpeed;
    }

    /// <summary>
    /// Invierte la dirección de rotación.
    /// </summary>
    public void ToggleReverseRotation()
    {
        reverseRotation = !reverseRotation;
    }

    /// <summary>
    /// Pausa o reanuda la rotación.
    /// </summary>
    public void TogglePause()
    {
        pauseRotation = !pauseRotation;
    }
}

