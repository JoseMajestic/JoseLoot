using UnityEngine;

/// <summary>
/// Sistema de dinero del jugador.
/// Gestiona las monedas del jugador y notifica cambios mediante eventos.
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Dinero inicial del jugador")]
    [SerializeField] private int initialMoney = 0;

    private int money = 0;

    // Eventos
    public System.Action<int> OnMoneyChanged; // Nueva cantidad de dinero
    public System.Action<int> OnMoneyAdded;   // Cantidad añadida
    public System.Action<int> OnMoneySubtracted; // Cantidad restada

    private void Start()
    {
        money = initialMoney;
        OnMoneyChanged?.Invoke(money);
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
}

