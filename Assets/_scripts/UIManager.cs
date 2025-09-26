using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIPlayGame myUIPlayGame;

    public void StartGame()
    {
        myUIPlayGame.ToggleStart(false);
    }

 
}
