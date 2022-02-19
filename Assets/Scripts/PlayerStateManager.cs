using UnityEngine;
using UnityEngine.UI;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] private Image foodProgressFillerImage = null;
    [SerializeField] private Image waterProgressFillerImage = null;
    [SerializeField] private Image lifeProgressFillerImage = null;
    [SerializeField] private Image virusProgressFillerImage = null;

    [SerializeField] private float loseFoodSpeed = 0.05f;
    [SerializeField] private float loseWaterSpeed = 0.05f;
    [SerializeField] private float loseLifeSpeed = 0.05f;
    [SerializeField] private float gainVirusSpeed = 0.05f;

    private float currentFoodAmount;
    private float currentWaterAmount;
    private float currentLifeAmount;
    private float currentVirusAmount;

    private void Start()
    {
        currentFoodAmount = 1;
        currentWaterAmount = 1;
        currentLifeAmount = 1;
        currentVirusAmount = 0;
    }

    private void Update()
    {
        currentFoodAmount = UpdateProgress(foodProgressFillerImage, -loseFoodSpeed, currentFoodAmount);
        currentWaterAmount = UpdateProgress(waterProgressFillerImage, -loseWaterSpeed, currentWaterAmount);
        currentLifeAmount = UpdateProgress(lifeProgressFillerImage, -loseLifeSpeed, currentLifeAmount);
        currentVirusAmount = UpdateProgress(virusProgressFillerImage, +gainVirusSpeed, currentVirusAmount);
    }

    private float UpdateProgress(Image progressFiller, float amountToAdd, float currentAmount)
    {
        currentAmount = Mathf.Clamp01(currentAmount + amountToAdd * Time.deltaTime);

        if (progressFiller == null)
        {
            Debug.LogWarning("[PlayerStateManager] No progress bar serialized !");
            return currentAmount;
        }
        
        progressFiller.fillAmount = currentAmount;

        return currentAmount;
    }
}
