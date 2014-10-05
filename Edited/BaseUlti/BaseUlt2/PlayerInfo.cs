using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace BaseUlt2
{
	internal class PlayerInfo
	{
		public Obj_AI_Hero Champ;
		public Dictionary<int, float> IncomingDamage;
		public int LastSeen;
		public Packet.S2C.Recall.Struct Recall;
		public PlayerInfo(Obj_AI_Hero champ)
		{
			this.Champ = champ;
			this.Recall = new Packet.S2C.Recall.Struct(champ.NetworkId, Packet.S2C.Recall.RecallStatus.Unknown, Packet.S2C.Recall.ObjectType.Player, 0);
			this.IncomingDamage = new Dictionary<int, float>();
		}
		public PlayerInfo UpdateRecall(Packet.S2C.Recall.Struct newRecall)
		{
			this.Recall = newRecall;
			return this;
		}
		public int GetRecallStart()
		{
			int status = (int)this.Recall.Status;
			if (status == 0 || status == 4)
			{
				return Program.RecallT[this.Recall.UnitNetworkId];
			}
			return 0;
		}
		public int GetRecallEnd()
		{
			return this.GetRecallStart() + this.Recall.Duration;
		}
		public int GetRecallCountdown()
		{
			int num = this.GetRecallEnd() - Environment.TickCount;
			if (num >= 0)
			{
				return num;
			}
			return 0;
		}
		public override string ToString()
		{
			string text = this.Champ.ChampionName + ": " + this.Recall.Status;
			float num = (float)this.GetRecallCountdown() / 1000f;
			if (num > 0f)
			{
				text = text + " (" + num.ToString("0.00", CultureInfo.InvariantCulture) + "s)";
			}
			return text;
		}
	}
}
