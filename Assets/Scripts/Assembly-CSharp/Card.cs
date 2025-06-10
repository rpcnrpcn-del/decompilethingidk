using UnityEngine;
using UnityEngine.UI;

public class Card : Tool
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private string hiddenWordString = "No Peeking!";

	private SynchronizedField<string> _word;

	private bool _wordHidden;

	public string Word
	{
		get
		{
			return _word.Get();
		}
		set
		{
			_word.ForceSet(value);
		}
	}

	public bool WordHidden
	{
		get
		{
			return _wordHidden;
		}
		set
		{
			_wordHidden = value;
			UpdateUIText();
		}
	}

	private void OnWordChange()
	{
		UpdateUIText();
	}

	private void UpdateUIText()
	{
		if (WordHidden)
		{
			text.text = hiddenWordString;
		}
		else
		{
			text.text = Word;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_word = new SynchronizedField<string>(this, "WORD", hiddenWordString, SetterPermissionMode.ANYONE, OnWordChange);
		SetCollisionLayer(Layers.DynamicPhysicsIgnoreDynamicPhysics);
		base.Rigidbody.isKinematic = true;
		ToolCleanup component = GetComponent<ToolCleanup>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdateUIText();
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
		SetCollisionLayer(Layers.DynamicPhysicsIgnoreDynamicPhysics);
		base.Rigidbody.isKinematic = true;
	}
}
