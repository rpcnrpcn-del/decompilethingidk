using System;
using System.IO;
using UnityEngine;

namespace UTJ
{
	[Serializable]
	public class DataPath
	{
		public enum Root
		{
			CurrentDirectory = 0,
			PersistentDataPath = 1,
			StreamingAssetsPath = 2,
			TemporaryCachePath = 3,
			DataPath = 4
		}

		public Root m_root;

		public string m_leaf;

		public DataPath()
		{
		}

		public DataPath(Root root, string leaf)
		{
			m_root = root;
			m_leaf = leaf;
		}

		public string GetPath()
		{
			string text = string.Empty;
			switch (m_root)
			{
			case Root.CurrentDirectory:
				text += ".";
				break;
			case Root.PersistentDataPath:
				text += Application.persistentDataPath;
				break;
			case Root.StreamingAssetsPath:
				text += Application.streamingAssetsPath;
				break;
			case Root.TemporaryCachePath:
				text += Application.temporaryCachePath;
				break;
			case Root.DataPath:
				text += Application.dataPath;
				break;
			}
			if (m_leaf.Length > 0)
			{
				text += "/";
				text += m_leaf;
			}
			return text;
		}

		public void CreateDirectory()
		{
			Directory.CreateDirectory(GetPath());
		}
	}
}
