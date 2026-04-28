using System.Collections.Generic;
using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Tracks how many memory fragments the player has gathered and
    /// stores the text of each remembered memory for the win-screen log.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public int FragmentsCollected { get; private set; }
        public List<string> CollectedMemories { get; } = new List<string>();

        public System.Action OnFragmentCollected;

        public void CollectFragment(string memoryText)
        {
            FragmentsCollected++;
            if (!string.IsNullOrEmpty(memoryText))
                CollectedMemories.Add(memoryText);

            OnFragmentCollected?.Invoke();
        }
    }
}
