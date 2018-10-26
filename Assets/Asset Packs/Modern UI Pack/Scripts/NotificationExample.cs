using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NotificationExample : MonoBehaviour {

	private GameObject notificationObject;
	private Animator notificationAnimator;

	[Header("OBJECT")]
	public Text titleObject;
	public Text descriptionObject;

	[Header("VARIABLES")]
	public string titleText;
	public string descriptionText;
	[SerializeField] private string animationNameIn;
	[SerializeField] private string animationNameOut;

	void Awake()
	{
        notificationAnimator = GetComponent<Animator>();
        notificationObject = this.gameObject;
		notificationObject.SetActive (false);
	}

	public void ShowNotification () 
	{
		notificationObject.SetActive (true);
		titleObject.text = titleText;
		descriptionObject.text = descriptionText;

		notificationAnimator.Play (animationNameIn);
		//notificationAnimator.Play (animationNameOut);
	
	}
}
