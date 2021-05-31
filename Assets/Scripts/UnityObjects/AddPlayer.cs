using Assets.Scripts;
using UnityEngine;
using UnityEngine.Advertisements;

public class AddPlayer : MonoBehaviour
{
    private bool testMode = true;
    private bool addRun = false;

    // Start is called before the first frame update
    void Start()
    {
        Advertisement.Initialize(PersistentData.Instance.AndroidGameId, testMode);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!addRun)
        {
            ShowInterstitialAd();
        }
    }

    public void ShowInterstitialAd()
    {
        // Check if UnityAds ready before calling Show method:
        if (Advertisement.IsReady())
        {
            addRun = true;

            // Id in parenteses is found on the unity dashboard
            Advertisement.Show("Interstitial_Android");
        }
        else
        {
            Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
        }
    }
}
