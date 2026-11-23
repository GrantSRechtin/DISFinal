using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

[RequireComponent(typeof(Collider))]
public class NextLevelTrigger : MonoBehaviour
{
    private bool isLoadingScene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerInputHandler>() != null)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (isLoadingScene)
        {
            return;
        }

        int nextBuildIndex = UnitySceneManager.GetActiveScene().buildIndex + 1;
        if (nextBuildIndex <= 0 || nextBuildIndex >= UnitySceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("There is no next enabled scene in Build Settings.", this);
            return;
        }

        isLoadingScene = true;
        Time.timeScale = 1f;
        UnitySceneManager.LoadScene(nextBuildIndex);
    }

    private void OnValidate()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null && !trigger.isTrigger)
        {
            Debug.LogWarning("NextLevelTrigger requires its Collider to have Is Trigger enabled.", this);
        }
    }
}
