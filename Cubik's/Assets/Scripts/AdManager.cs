using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager {
	public bool testAd = true;

    public AdManager() {
    	MobileAds.Initialize(initStatus => { });
        RequestInterstitialAds();
    }

    private InterstitialAd interstitial;
    void RequestInterstitialAds() {
    	string adID;
		if(testAd) {
			adID = "ca-app-pub-3940256099942544/1033173712";
		} else {
			adID = "realID";
		}

		interstitial = new InterstitialAd(adID);
		AdRequest request = new AdRequest.Builder().Build();
	    interstitial.LoadAd(request);
    }

    private BannerView bannerView;
    public void ShowBannerAds() {
    	string adID;
		if(testAd) {
			adID = "ca-app-pub-3940256099942544/6300978111";
		} else {
			adID = "realID";
		}

		bannerView = new BannerView(adID, AdSize.Banner, AdPosition.Bottom);
		AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }

    public void ShowInterstitialAds() {
    	if(interstitial.IsLoaded()) {
			interstitial.Show();
		}
    }

    public void HideBannerAds() {
    	bannerView.Destroy();
    }
}