using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithNetworkingPeer : Message<Oculus.Platform.Models.NetworkingPeer>
	{
		public MessageWithNetworkingPeer(IntPtr c_message)
			: base(c_message)
		{
		}

		public override Oculus.Platform.Models.NetworkingPeer GetNetworkingPeer()
		{
			return base.Data;
		}

		protected override Oculus.Platform.Models.NetworkingPeer GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNetworkingPeer(c_message);
			return new Oculus.Platform.Models.NetworkingPeer(CAPI.ovr_NetworkingPeer_GetID(obj), CAPI.ovr_NetworkingPeer_GetState(obj));
		}
	}
}
