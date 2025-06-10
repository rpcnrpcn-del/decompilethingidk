using System;
using System.Collections.Generic;
using UnityEngine;

public class CardBox : Tool
{
	[Header("Card Spawning")]
	[SerializeField]
	private Transform cardSpawnRoot;

	[SerializeField]
	private float cardRespawnDuration = 0.5f;

	[Header("Words")]
	[SerializeField]
	private TextAsset[] wordListAssets;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip dispenseCard;

	private const string DEFAULT_WORD = "RecRoom";

	private Card[] cards;

	private List<string> wordList;

	private int[] wordIndices;

	private SynchronizedField<float> _lastCardDespawnTime;

	private SynchronizedField<bool> _dirty;

	private SynchronizedField<int> _currentCardIndex;

	private SynchronizedField<int> _previousCardIndex;

	private SynchronizedField<int> _wordIndexShuffleSeed;

	private SynchronizedField<int> _currentWordIndicesIndex;

	private SynchronizedField<int> _previousWordIndex;

	private bool dirty
	{
		get
		{
			return _dirty.Get();
		}
		set
		{
			_dirty.ForceSet(value);
		}
	}

	private float lastDirtyTime
	{
		get
		{
			return _lastCardDespawnTime.Get();
		}
		set
		{
			_lastCardDespawnTime.ForceSet(value);
		}
	}

	private int currentCardIndex
	{
		get
		{
			return _currentCardIndex.Get();
		}
		set
		{
			_currentCardIndex.ForceSet(value);
		}
	}

	private int previousCardIndex
	{
		get
		{
			return _previousCardIndex.Get();
		}
		set
		{
			_previousCardIndex.ForceSet(value);
		}
	}

	private Card previousCard
	{
		get
		{
			return cards[previousCardIndex];
		}
	}

	private Card currentCard
	{
		get
		{
			return cards[currentCardIndex];
		}
	}

	public string CurrentWord
	{
		get
		{
			return (wordList.Count <= 0) ? "RecRoom" : wordList[currentWordIndex];
		}
	}

	public string PreviousWord
	{
		get
		{
			return (wordList.Count <= 0) ? "RecRoom" : wordList[previousWordIndex];
		}
	}

	private int currentWordIndicesIndex
	{
		get
		{
			return _currentWordIndicesIndex.Get();
		}
		set
		{
			_currentWordIndicesIndex.ForceSet(value);
		}
	}

	private int currentWordIndex
	{
		get
		{
			return wordIndices[currentWordIndicesIndex];
		}
	}

	private int previousWordIndex
	{
		get
		{
			return _previousWordIndex.Get();
		}
		set
		{
			_previousWordIndex.ForceSet(value);
		}
	}

	private int wordIndexShuffleSeed
	{
		get
		{
			return _wordIndexShuffleSeed.Get();
		}
		set
		{
			_wordIndexShuffleSeed.ForceSet(value);
		}
	}

	public void UpdateLocalPlayerCardVisibility(bool localPlayerIsPerformer)
	{
		if (previousCard != null)
		{
			previousCard.WordHidden = !localPlayerIsPerformer;
		}
		if (currentCard != null)
		{
			currentCard.WordHidden = !localPlayerIsPerformer;
		}
	}

	public void MasterClearCards()
	{
		if (PhotonNetwork.isMasterClient)
		{
			currentCardIndex = 1;
			previousCardIndex = 0;
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].Disable();
			}
			dirty = true;
			lastDirtyTime = (float)PhotonNetwork.time;
		}
	}

	private string[] LoadWordList(TextAsset wordListAsset)
	{
		return wordListAsset.text.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
	}

	private void IncrementWordIndices()
	{
		previousWordIndex = currentWordIndex;
		if (currentWordIndicesIndex + 1 >= wordIndices.Length)
		{
			wordIndexShuffleSeed = (int)DateTime.Now.Ticks;
			currentWordIndicesIndex = 0;
		}
		else
		{
			currentWordIndicesIndex++;
		}
	}

	private void ShuffleWordIndices()
	{
		int num = wordIndices.Length;
		for (int i = 0; i < num; i++)
		{
			wordIndices[i] = i;
		}
		UnityExtensions.ShuffleArray(wordIndices, wordIndexShuffleSeed);
	}

	private void OnWordIndexShuffleSeedChanges()
	{
		ShuffleWordIndices();
	}

	private void CreateWordList()
	{
		wordList = new List<string>();
		for (int i = 0; i < wordListAssets.Length; i++)
		{
			wordList.AddRange(LoadWordList(wordListAssets[i]));
		}
		wordIndices = new int[wordList.Count];
	}

	protected override void Awake()
	{
		base.Awake();
		_dirty = new SynchronizedField<bool>(this, "DIRTY", true, SetterPermissionMode.ANYONE);
		_lastCardDespawnTime = new SynchronizedField<float>(this, "LAST_CARD_DESPAWN_TIME", -1f, SetterPermissionMode.ANYONE);
		_currentCardIndex = new SynchronizedField<int>(this, "CURRENT_CARD_INDEX", 0, SetterPermissionMode.ANYONE);
		_previousCardIndex = new SynchronizedField<int>(this, "PREVIOUS_CARD_INDEX", 0, SetterPermissionMode.ANYONE);
		_wordIndexShuffleSeed = new SynchronizedField<int>(this, "WORD_SHUFFLE_SEED", 12345, SetterPermissionMode.MASTER, OnWordIndexShuffleSeedChanges);
		_currentWordIndicesIndex = new SynchronizedField<int>(this, "CURRENT_WORD_INDICES_INDEX", 0, SetterPermissionMode.MASTER);
		_previousWordIndex = new SynchronizedField<int>(this, "PREVIOUS_WORD_INDEX", 0, SetterPermissionMode.MASTER);
		CreateWordList();
		if (PhotonNetwork.isMasterClient)
		{
			wordIndexShuffleSeed = (int)DateTime.Now.Ticks;
		}
		ShuffleWordIndices();
		cards = UnityEngine.Object.FindObjectsOfType<Card>();
		for (int i = 0; i < cards.Length; i++)
		{
			cards[i].PickupEvent += OnToolPickup;
			cards[i].ResetEvent += OnToolReset;
		}
		if (PhotonNetwork.isMasterClient && cards.Length <= 1)
		{
			Debug.LogError("Not enough cards in the scene to double buffer cards.");
		}
	}

	protected override void Start()
	{
		base.Start();
		MasterClearCards();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (PhotonNetwork.isMasterClient && dirty && (float)PhotonNetwork.time - lastDirtyTime >= cardRespawnDuration)
		{
			int num = currentCardIndex;
			currentCardIndex = previousCardIndex;
			previousCardIndex = num;
			currentCard.MasterResetToDefault(cardSpawnRoot.position, cardSpawnRoot.rotation);
			IncrementWordIndices();
			currentCard.Word = CurrentWord;
			dirty = false;
		}
	}

	private void OnToolPickup(Tool tool)
	{
		Card component = tool.GetComponent<Card>();
		if (tool.hasAuthority && component != null)
		{
			base.photonView.RPC("RpcMasterRequestCardPickup", PhotonTargets.MasterClient, component.photonView.viewID);
		}
	}

	private void OnToolReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		Card component = tool.GetComponent<Card>();
		if (component != null)
		{
			tool.RigidbodyPickup.Pickup(base.Rigidbody, base.transform.InverseTransformPoint(position), base.transform.InverseTransformRotation(rotation));
		}
	}

	private void MasterRequestCardPickup(Card card)
	{
		if (PhotonNetwork.isMasterClient && card == currentCard)
		{
			previousCard.Disable();
			if (!dirty)
			{
				dirty = true;
				lastDirtyTime = (float)PhotonNetwork.time;
			}
			base.photonView.RPC("RpcOnCardPickup", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void RpcMasterRequestCardPickup(int cardPhotonViewId)
	{
		PhotonView photonView = PhotonView.Find(cardPhotonViewId);
		Card card = ((!(photonView != null)) ? null : photonView.GetComponent<Card>());
		if (card != null)
		{
			MasterRequestCardPickup(card);
		}
	}

	[PunRPC]
	private void RpcOnCardPickup()
	{
		AudioManager.Play3DSFX(dispenseCard, base.transform);
	}
}
