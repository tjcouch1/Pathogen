using UnityEngine;

public class LUI_PAK : MonoBehaviour {

	[Header("VARIABLES")]
	public GameObject mainCanvas;
	public GameObject scriptObject;
    public GameObject PAK;
	public Animator animatorComponent;
	public string animName;
    public int waitTime;
    private float time;

	void Start ()
	{
		animatorComponent.GetComponent<Animator>();
	}

	void Update ()
	{
        time += Time.deltaTime;
        if(time >= waitTime)
        {
            PAK.SetActive(true);
            if (Input.anyKeyDown)
            {
                animatorComponent.Play(animName);
                mainCanvas.SetActive(true);
                Destroy(scriptObject);
            }
        }
	}
}