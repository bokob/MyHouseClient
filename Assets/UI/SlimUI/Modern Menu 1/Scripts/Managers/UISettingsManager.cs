using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu{
	public class UISettingsManager : MonoBehaviour {
		// sliders
		public GameObject musicSlider;
		private float sliderValue = 0.0f;

		public void  Start (){
			// check slider values
			musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
		}

		public void Update (){
			sliderValue = musicSlider.GetComponent<Slider>().value;
		}

		public void MusicSlider (){
			PlayerPrefs.SetFloat("MusicVolume", sliderValue);
		}
	}
}