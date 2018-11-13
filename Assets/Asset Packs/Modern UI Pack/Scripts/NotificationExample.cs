using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NotificationExample : MonoBehaviour {

	private GameObject notificationObject;
	private Animator notificationAnimator;

	[Header("OBJECT")]
	public Text titleObject;
	public Text descriptionObject;
    public Image icon;

	[Header("VARIABLES")]
	public string titleText;
	public string descriptionText;
    public Color textColor;
    [SerializeField] private string animationNameIn;
	[SerializeField] private string animationNameOut;

	void Awake()
	{
        notificationAnimator = GetComponent<Animator>();
        notificationObject = this.gameObject;
		notificationObject.SetActive (false);

        //Set the default color to what it already is
        textColor = titleObject.color;
	}

	public void ShowNotification () 
	{
		notificationObject.SetActive (true);
		titleObject.text = titleText;
		descriptionObject.text = descriptionText;
        titleObject.color = textColor;
        descriptionObject.color = textColor;

		notificationAnimator.Play (animationNameIn);
		//notificationAnimator.Play (animationNameOut);
	
	}
}
