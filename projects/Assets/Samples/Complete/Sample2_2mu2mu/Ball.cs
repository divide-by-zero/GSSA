using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{
	private int no;
	public GameMain gameMain;
	public SpriteRenderer img;
	public SpriteRenderer ilImg;
	public Sprite[] sprites;

	public int No
	{
		set
		{
			no = value;
			img.color = colors[value];
			ilImg.sprite = sprites[value];
			ilImg.sortingOrder = 10 + value;
		}
		get { return no; }
	}

	public bool IsDrag
	{
		set
		{
			if (value)
			{
				img.color = Color.red;
			}
			else
			{
				No = No;
			}
		}
	}

	private static readonly Color[] colors = new Color[]{
		Color.white,
		Color.blue,
		Color.yellow,
		Color.green
	};

	void Start()
	{
		var scale = Random.Range(5.0f, 6.0f);
		transform.localScale = new Vector3(scale,scale,scale);
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y < -10.0f)Destroy(this.gameObject);
	}

	public void OnMouseOver() {
		if (Input.GetMouseButton(0))
		{
			gameMain.Register(this);
		}
	}
}
