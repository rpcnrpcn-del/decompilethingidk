using System;
using System.Collections.Generic;

namespace RecNet
{
	public class RecRoomConfig : IRecNetObject
	{
		public string MessageOfTheDay { get; set; }

		public MatchmakingConfigParams MatchmakingParams { get; set; }

		public ProgressionManager.Objective[][] DailyObjectives { get; set; }

		public Dictionary<string, string> ConfigTable { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			MessageOfTheDay = Util.GetKey<string>("MessageOfTheDay", dict);
			MatchmakingParams = new MatchmakingConfigParams();
			MatchmakingParams.Deserialize(dict["MatchmakingParams"] as Dictionary<string, object>);
			List<object> list = dict["DailyObjectives"] as List<object>;
			DailyObjectives = new ProgressionManager.Objective[list.Count][];
			for (int i = 0; i < list.Count; i++)
			{
				List<object> list2 = list[i] as List<object>;
				DailyObjectives[i] = new ProgressionManager.Objective[list2.Count];
				for (int j = 0; j < list2.Count; j++)
				{
					Dictionary<string, object> dict2 = list2[j] as Dictionary<string, object>;
					DailyObjectives[i][j] = new ProgressionManager.Objective
					{
						ObjectiveType = (ProgressionManager.ObjectiveType)Util.GetKey<int>("type", dict2),
						RequiredScore = Util.GetKey<int>("score", dict2)
					};
				}
			}
			ConfigTable = new Dictionary<string, string>();
			List<object> list3 = dict["ConfigTable"] as List<object>;
			for (int k = 0; k < list3.Count; k++)
			{
				Dictionary<string, object> dict3 = list3[k] as Dictionary<string, object>;
				ConfigTable.Add(Util.GetKey<string>("Key", dict3), Util.GetKey<string>("Value", dict3));
			}
		}

		public Dictionary<string, object> Serialize()
		{
			throw new NotImplementedException();
		}
	}
}
