using UnityEngine;
using UnityEngine.UI;

namespace IvyMoon
{
    public class HealthBar : MonoBehaviour
    {
        public RectTransform healthFill; // Reference to the HealthFill RectTransform
        private float maxWidth; // Stores the full width of the health bar

        private void Start()
        {
            if (healthFill != null)
            {
                maxWidth = healthFill.sizeDelta.x; // Save the initial width of the health bar
            }
            else
            {
                Debug.LogWarning("HealthFill RectTransform is not assigned.");
            }
        }

        // Method to update the health bar based on the current health
        public void UpdateHealthUI(int currentHealth, int maxHealth)
        {
            if (healthFill == null)
            {
                Debug.LogWarning("HealthFill RectTransform is not assigned.");
                return;
            }

            if (maxHealth <= 0)
            {
                Debug.LogError("Max health must be greater than zero.");
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth);

            // Calculate the new width based on the health percentage
            float healthPercent = (float)currentHealth / maxHealth;
            healthFill.sizeDelta = new Vector2(maxWidth * healthPercent, healthFill.sizeDelta.y);

            // Debug log to verify values
            #if UNITY_EDITOR
            Debug.Log($"Health updated: {currentHealth}/{maxHealth}, Width set to: {maxWidth * healthPercent}");
            #endif
        }
    }
}
