using System.Collections.Generic;
using System.Linq;
using RecNet;
using UnityEngine;

public class DebugManager : SingletonMonoBehaviour<DebugManager>
{
	public enum RandomName
	{
		Default = -1,
		Girl = 0,
		Honeypot = 1,
		Elven = 2
	}

	[Header("Random name generation")]
	[SerializeField]
	private List<string> normalGirlNames = new List<string> { "CatGirl", "Sunny", "Tiffany", "Princesse", "NerdQueen", "Dreamdance", "Jen112" };

	[SerializeField]
	private List<string> honeypotGirlNames = new List<string> { "Feminista", "Waifu", "Fembot", "QueerGirl", "TransQueen", "Babygirl" };

	[SerializeField]
	private List<string> elvenNames = new List<string>
	{
		"Gaeralagossil", "Dúvengwen", "Dúvenith", "Grogriel", "Nalladis", "Esgarbes", "Sileviel", "Loeniel", "Peniril", "Heriadis",
		"Echuiben", "Gaelben", "Gliror", "Dregor", "Taureth", "Lhingiel", "Helviel", "Hadis", "Bastadis", "Falastor",
		"Duirronir", "Dinalagossion", "Narchon", "Orthellon", "Mallostor", "Eredhon", "Úhaelion", "Amarthon", "Thadhrion", "Nîdhel",
		"Cugiel", "Nûril", "Pedrien", "Leithriel", "Glîndaer", "Lîron", "Uilchon", "Barthion", "Ristedir"
	};

	private void Awake()
	{
		SingletonMonoBehaviour<DebugManager>.Instance = this;
	}

	public void SetRandomName(RandomName type)
	{
		if (type == RandomName.Default)
		{
			PhotonNetwork.player.name = Profiles.LocalProfile.DisplayName;
			ShowDebugMessage("Name: " + PhotonNetwork.player.name);
			return;
		}
		List<string> otherPlayerNames = new List<string>();
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			otherPlayerNames.Add(photonPlayer.name);
		}
		List<string> list = null;
		switch (type)
		{
		case RandomName.Girl:
			list = normalGirlNames;
			break;
		case RandomName.Honeypot:
			list = honeypotGirlNames;
			break;
		default:
			list = elvenNames;
			break;
		}
		List<string> list2 = ((otherPlayerNames == null || otherPlayerNames.Count <= 0) ? list : (from d in list
			from o in otherPlayerNames
			where !d.Contains(o)
			select d).ToList());
		if (list2.Count == 0)
		{
			ShowDebugMessage("Failed to get random name");
			return;
		}
		string text = list2.Random();
		ShowDebugMessage("Random name: " + text);
		PhotonNetwork.player.name = text;
	}

	private void ShowDebugMessage(string message)
	{
		MenuNotification.PlayNext(message, 1.5f, null, true, null, 0.5f);
	}
}
