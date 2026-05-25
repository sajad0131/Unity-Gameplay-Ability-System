using UnityEngine;

namespace UnityGAS
{
    [CreateAssetMenu(fileName = "NewGameplayTag", menuName = "GAS/Gameplay Tag")]
    public class GameplayTag : ScriptableObject
    {
        [TextArea] public string developerNote;
    }
}