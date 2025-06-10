using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithRoom : Message<Oculus.Platform.Models.Room>
	{
		public MessageWithRoom(IntPtr c_message)
			: base(c_message)
		{
		}

		public override Oculus.Platform.Models.Room GetRoom()
		{
			return base.Data;
		}

		protected override Oculus.Platform.Models.Room GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetRoom(obj);
			return new Oculus.Platform.Models.Room(o);
		}
	}
}
