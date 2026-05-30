using UnityEngine;

// C13_PilotRunner.cs (Unity 6, LorQB-Android)
// Minimal runner to attach C13_RedToGreen to scene
// Add this component to any GameObject to run the C13 test

public class C13_PilotRunner : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("[C13] PilotRunner loaded");

        // Add C13_RedToGreen component if not already present
        if (GetComponent<C13_RedToGreen>() == null)
        {
            gameObject.AddComponent<C13_RedToGreen>();
            Debug.Log("[C13] C13_RedToGreen component added");
        }
    }
}
