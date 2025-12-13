using UnityEngine;

/// <summary>
/// Sistema de dinero del jugador.
/// Gestiona las monedas del jugador y notifica cambios mediante eventos.
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Dinero inicial del jugador (solo se usa cuando se hace un reset completo del juego)")]
    [SerializeField] private int initialMoney = 0;

    private int money = 0;
    private bool moneyLoadedFromProfile = false;

    // Eventos
    public System.Action<int> OnMoneyChanged; // Nueva cantidad de dinero
    public System.Action<int> OnMoneyAdded;   // Cantidad añadida
    public System.Action<int> OnMoneySubtracted; // Cantidad restada

    private void Start()
    {
        // SOLUCIÓN: Las monedas se cargan desde GameDataManager.LoadPlayerProfile()
        // Si no se cargaron desde el perfil (perfil nuevo), usar las monedas iniciales
        // Esperar un frame para asegurar que GameDataManager ya cargó el perfil
        StartCoroutine(InitializeMoney());
    }
    
    /// <summary>
    /// Inicializa las monedas después de que GameDataManager haya cargado el perfil.
    /// Si hay un perfil guardado, las monedas ya fueron cargadas por GameDataManager.
    /// Si es un perfil nuevo, usar las monedas iniciales del Inspector.
    /// </summary>
    private System.Collections.IEnumerator InitializeMoney()
    {
        yield return null; // Esperar un frame para que GameDataManager cargue el perfil
        
        // Si las monedas no fueron cargadas desde el perfil (perfil nuevo), usar las iniciales
        if (!moneyLoadedFromProfile)
        {
            money = initialMoney;
            OnMoneyChanged?.Invoke(money);
        }
    }
    
    /// <summary>
    /// Marca que las monedas fueron cargadas desde el perfil guardado.
    /// Se llama desde GameDataManager cuando carga un perfil existente.
    /// </summary>
    public void MarkMoneyLoadedFromProfile()
    {
        moneyLoadedFromProfile = true;
    }

    public int GetMoney() => money;

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Intentando añadir cantidad negativa de dinero. Usa SubtractMoney en su lugar.");
            return;
        }

        money += amount;
        OnMoneyAdded?.Invoke(amount);
        OnMoneyChanged?.Invoke(money);
    }

    public void SubtractMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Intentando restar cantidad negativa de dinero. Usa AddMoney en su lugar.");
            return;
        }

        money = Mathf.Max(0, money - amount);
        OnMoneySubtracted?.Invoke(amount);
        OnMoneyChanged?.Invoke(money);
    }

    public void SetMoney(int amount)
    {
        int previousMoney = money;
        money = Mathf.Max(0, amount);
        
        int difference = money - previousMoney;
        if (difference > 0)
        {
            OnMoneyAdded?.Invoke(difference);
        }
        else if (difference < 0)
        {
            OnMoneySubtracted?.Invoke(Mathf.Abs(difference));
        }
        
        OnMoneyChanged?.Invoke(money);
    }
    
    /// <summary>
    /// Resetea las monedas a las monedas iniciales del Inspector.
    /// Se usa cuando se hace un reset completo del juego.
    /// </summary>
    public void ResetToInitialMoney()
    {
        SetMoney(initialMoney);
        moneyLoadedFromProfile = false; // Permitir que se reinicialice si es necesario
    }
    
    /// <summary>
    /// Obtiene las monedas iniciales configuradas en el Inspector.
    /// </summary>
    public int GetInitialMoney()
    {
        return initialMoney;
    }
}

