using System.Collections.Generic;
using Photon;
using UnityEngine;

public class PlayingCardDeck : Photon.MonoBehaviour
{
	private const int DECK_SIZE = 52;

	[SerializeField]
	private PhysicalButton recallCardsButton;

	private PlayingCard[] cards;

	private List<PlayingCard> recalledCards = new List<PlayingCard>();

	private HashSet<ToolGroup> recalledGroups = new HashSet<ToolGroup>();

	private Vector3 initialPosition = Vector3.zero;

	private Quaternion initialRotation = Quaternion.identity;

	protected override void Awake()
	{
		initialPosition = base.transform.position;
		initialRotation = base.transform.rotation;
		cards = GetComponentsInChildren<PlayingCard>();
		if (cards.Length != 52)
		{
			Debug.LogError("Deck does not have the correct number of cards.  Has " + cards.Length + " and requires " + 52 + " cards.");
		}
		if (recallCardsButton != null)
		{
			recallCardsButton.PushEvent += OnRecallButtonPressed;
		}
	}

	private void Start()
	{
		if (!base.hasAuthority)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 13; j++)
			{
				cards[i * 13 + j].Suit = (PlayingCard.PlayingCardSuit)i;
				cards[i * 13 + j].Value = (PlayingCard.PlayingCardValue)j;
			}
		}
		CreateGroup(cards);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (recallCardsButton != null)
		{
			recallCardsButton.PushEvent -= OnRecallButtonPressed;
		}
	}

	private void CreateGroup(PlayingCard[] cardsToAdd)
	{
		PlayingCardGroup playingCardGroup = cardsToAdd[0].CreateNewGroup() as PlayingCardGroup;
		playingCardGroup.IsDeck = true;
		playingCardGroup.AuthorityAddTools(cardsToAdd);
		playingCardGroup.AuthorityShuffle();
		playingCardGroup.transform.position = initialPosition;
		playingCardGroup.transform.rotation = initialRotation;
	}

	private void OnRecallButtonPressed(PhysicalButton thisButton, Player pushPlayer)
	{
		if (pushPlayer.isLocal)
		{
			RequestRecall();
		}
	}

	private void RequestRecall()
	{
		base.photonView.RPC("RpcMasterRecall", PhotonTargets.MasterClient);
	}

	private void MasterRecall()
	{
		recalledCards.Clear();
		recalledGroups.Clear();
		for (int i = 0; i < cards.Length; i++)
		{
			if (cards[i].IsInGroup && !cards[i].Group.IsHeld && !recalledGroups.Contains(cards[i].Group))
			{
				recalledGroups.Add(cards[i].Group);
			}
		}
		foreach (ToolGroup recalledGroup in recalledGroups)
		{
			if (!recalledGroup.hasAuthority)
			{
				recalledGroup.photonView.TransferOwnership(PhotonNetwork.player);
			}
			recalledGroup.AuthorityClearTools();
			recalledGroup.AuthorityDestroy();
		}
		for (int j = 0; j < cards.Length; j++)
		{
			if (!cards[j].IsInGroup && !cards[j].IsHeld)
			{
				recalledCards.Add(cards[j]);
			}
		}
		CreateGroup(recalledCards.ToArray());
	}

	[PunRPC]
	private void RpcMasterRecall()
	{
		MasterRecall();
	}
}
