using System;
using System.Net;
using System.Net.Sockets;

namespace UnityEngine.PSVita
{
	[Serializable]
	public class SceEndPoint : EndPoint
	{
		private IPAddress address;

		private int port;

		private int vport;

		public const int MaxPort = 65535;

		public const int MinPort = 0;

		public override AddressFamily AddressFamily => AddressFamily.InterNetwork;

		public int Port
		{
			get
			{
				return port;
			}
			set
			{
				if (value < 0 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("Invalid port");
				}
				port = value;
			}
		}

		public int VPort
		{
			get
			{
				return vport;
			}
			set
			{
				if (value < 0 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("Invalid port");
				}
				vport = value;
			}
		}

		public IPAddress Address
		{
			get
			{
				return address;
			}
			set
			{
				address = value;
			}
		}

		public SceEndPoint(IPAddress address, int port, int vport)
		{
			Address = address;
			Port = port;
			VPort = vport;
		}

		public SceEndPoint(long iaddr, int port, int vport)
		{
			Address = new IPAddress(iaddr);
			Port = port;
			VPort = vport;
		}

		public override EndPoint Create(SocketAddress socketAddress)
		{
			if (socketAddress == null)
			{
				throw new ArgumentNullException("socketAddress");
			}
			if (socketAddress.Family != AddressFamily)
			{
				throw new ArgumentException(string.Concat("The IPEndPoint was created using ", AddressFamily, " AddressFamily but SocketAddress contains ", socketAddress.Family, " instead, please use the same type."));
			}
			int size = socketAddress.Size;
			AddressFamily family = socketAddress.Family;
			SceEndPoint sceEndPoint = null;
			if (family == AddressFamily.InterNetwork)
			{
				if (size < 16)
				{
					return null;
				}
				int num = (socketAddress[2] << 8) + socketAddress[3];
				int num2 = (socketAddress[8] << 8) + socketAddress[9];
				long iaddr = ((long)(int)socketAddress[7] << 24) + ((long)(int)socketAddress[6] << 16) + ((long)(int)socketAddress[5] << 8) + (int)socketAddress[4];
				return new SceEndPoint(iaddr, num, num2);
			}
			return null;
		}

		public override SocketAddress Serialize()
		{
			SocketAddress socketAddress = null;
			AddressFamily addressFamily = address.AddressFamily;
			if (addressFamily == AddressFamily.InterNetwork)
			{
				socketAddress = new SocketAddress(AddressFamily.InterNetwork, 16);
				socketAddress[0] = 16;
				socketAddress[1] = 2;
				socketAddress[2] = (byte)((uint)(port >> 8) & 0xFFu);
				socketAddress[3] = (byte)((uint)port & 0xFFu);
				byte[] addressBytes = address.GetAddressBytes();
				socketAddress[4] = addressBytes[0];
				socketAddress[5] = addressBytes[1];
				socketAddress[6] = addressBytes[2];
				socketAddress[7] = addressBytes[3];
				socketAddress[8] = (byte)((uint)(vport >> 8) & 0xFFu);
				socketAddress[9] = (byte)((uint)vport & 0xFFu);
				SocketAddress socketAddress2 = socketAddress;
				byte b2 = (socketAddress[15] = 0);
				b2 = (socketAddress[14] = b2);
				b2 = (socketAddress[13] = b2);
				b2 = (socketAddress[12] = b2);
				b2 = (socketAddress[11] = b2);
				socketAddress2[10] = b2;
			}
			return socketAddress;
		}
	}
}
