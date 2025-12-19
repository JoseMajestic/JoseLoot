using UnityEngine;
using System.Collections;

/// <summary>
/// Rota un cilindro continuamente en el eje Y para crear un efecto de paisaje giratorio.
/// También cambia aleatoriamente la textura del cilindro entre un array de sprites.
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

    [Header("Configuración de Textura")]
    [Tooltip("Material del cilindro que contiene la textura a cambiar")]
    [SerializeField] private Material cylinderMaterial;
    
    [Tooltip("Array de sprites que se aplicarán aleatoriamente a la textura del cilindro")]
    [SerializeField] private Sprite[] backgroundSprites;
    
    [Tooltip("Tiempo en milisegundos para cambiar aleatoriamente entre los sprites")]
    [SerializeField] private float changeIntervalMs = 5000f;

    private Renderer cylinderRenderer;
    private Coroutine textureChangeCoroutine;
    private float timer = 0f;

    private void Awake()
    {
        // Intentar obtener el Renderer si no se asignó el Material directamente
        if (cylinderMaterial == null)
        {
            cylinderRenderer = GetComponent<Renderer>();
            if (cylinderRenderer != null)
            {
                cylinderMaterial = cylinderRenderer.material;
            }
        }
    }

    private void OnEnable()
    {
        // Reiniciar el cambio de texturas cuando se activa el objeto
        if (textureChangeCoroutine != null)
        {
            StopCoroutine(textureChangeCoroutine);
        }
        timer = 0f;
        textureChangeCoroutine = StartCoroutine(ChangeTextureCoroutine());
    }

    private void OnDisable()
    {
        // Detener la corrutina cuando se desactiva el objeto
        if (textureChangeCoroutine != null)
        {
            StopCoroutine(textureChangeCoroutine);
            textureChangeCoroutine = null;
        }
    }

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
    /// Corrutina que cambia aleatoriamente la textura del cilindro.
    /// </summary>
    private IEnumerator ChangeTextureCoroutine()
    {
        while (true)
        {
            // Convertir milisegundos a segundos
            float intervalSeconds = changeIntervalMs / 1000f;
            
            yield return new WaitForSeconds(intervalSeconds);
            
            // Cambiar la textura si hay sprites disponibles
            if (backgroundSprites != null && backgroundSprites.Length > 0 && cylinderMaterial != null)
            {
                ChangeToRandomSprite();
            }
        }
    }

    /// <summary>
    /// Cambia la textura del cilindro a un sprite aleatorio del array.
    /// </summary>
    private void ChangeToRandomSprite()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0 || cylinderMaterial == null)
            return;

        // Seleccionar un sprite aleatorio
        int randomIndex = Random.Range(0, backgroundSprites.Length);
        Sprite selectedSprite = backgroundSprites[randomIndex];

        if (selectedSprite != null)
        {
            // Convertir el sprite a Texture2D
            Texture2D texture = SpriteToTexture2D(selectedSprite);
            
            if (texture != null)
            {
                // Aplicar la textura al material
                cylinderMaterial.mainTexture = texture;
            }
        }
    }

    /// <summary>
    /// Convierte un Sprite a Texture2D usando la textura directamente.
    /// </summary>
    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite == null)
            return null;

        // Usar directamente la textura del sprite (no requiere Read/Write Enabled)
        // Si el sprite es parte de un atlas, esto devolverá la textura completa del atlas
        return sprite.texture;
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

