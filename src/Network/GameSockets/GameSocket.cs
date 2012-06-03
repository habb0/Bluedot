﻿using System;
using System.Net;
using System.Net.Sockets;
using Bluedot.HabboServer.Habbos;
using Nito.Async;
using Nito.Async.Sockets;

namespace Bluedot.HabboServer.Network
{
    public class GameSocket
    {
        #region Events
        #region Event: PacketArrived
        /// <summary>
        /// Indicates the completion of a packet read from the socket.
        /// </summary>
        public event Action<AsyncResultEventArgs<byte[]>> PacketArrived;
        #endregion
        #endregion

        #region Fields
        private readonly ServerChildTcpSocket _internalSocket;
        private int _bytesReceived;
        private readonly byte[] _lengthBuffer;
        private byte[] _dataBuffer;
        private readonly GameSocketReader _protocolReader;
        #endregion

        #region Properties
        #region Property: PacketHandlers
        public GameSocketMessageHandlerInvoker PacketHandlers
        {
            get;
            set;
        }
        #endregion
        #region Property: Habbo
        public Habbo Habbo
        {
            get;
            internal set;
        }
        #endregion
        #region Property: IPAddress
        public IPAddress IPAddress
        {
            get
            {
                try
                {
                    if (_internalSocket.RemoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        byte[] ipv6Bytes = new byte[]
                                               {
                                                   // First 80 bits should be 0.
                                                   // Next 16 bits should be 1.
                                                   // The renaming 32 bits should be the IPv4 bits.
                                                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 0, 0, 0
                                               };

                        byte[] ipv4Bytes = _internalSocket.RemoteEndPoint.Address.GetAddressBytes();
                        ipv4Bytes.CopyTo(ipv6Bytes, 12);

                        return new IPAddress(ipv6Bytes);
                    }
                    return _internalSocket.RemoteEndPoint.Address;
                }
                catch (ObjectDisposedException)
                {
                    return null;
                }
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Method: GameSocket (Constructor)
        internal GameSocket(ServerChildTcpSocket socket, GameSocketReader protocolReader)
        {
            _internalSocket = socket;
            _protocolReader = protocolReader;
            _lengthBuffer = new byte[_protocolReader.LengthBytes];
            PacketHandlers = new GameSocketMessageHandlerInvoker();

            Habbo = HabboDistributor.GetPreLoginHabbo(this);
        }
        #endregion

        #region Method: Start
        /// <summary>
        /// Begins reading from the socket.
        /// </summary>
        internal GameSocket Start()
        {
            _internalSocket.ReadCompleted += SocketReadCompleted;
            PacketArrived += ParsePacket;

            ContinueReading();
            return this;
        }
        #endregion
        #region Method: Disconnect
        public GameSocket Disconnect()
        {
            if (_internalSocket != null)
                _internalSocket.Close();

            PacketHandlers = null;
            Habbo = null;
            return this;
        }
        #endregion

        #region Method: ContinueReading
        /// <summary>
        /// Requests a read directly into the correct buffer.
        /// </summary>
        private void ContinueReading()
        {
            try
            {
                // Read into the appropriate buffer: length or data
                if (_dataBuffer != null)
                {
                    _internalSocket.ReadAsync(_dataBuffer, _bytesReceived, _dataBuffer.Length - _bytesReceived);
                }
                else
                {
                    _internalSocket.ReadAsync(_lengthBuffer, _bytesReceived, _lengthBuffer.Length - _bytesReceived);
                }
            }
            catch (ObjectDisposedException) { } // Socket closed.
        }
        #endregion
        #region Method: SocketReadCompleted
        private void SocketReadCompleted(AsyncResultEventArgs<int> args)
        {
            if (args.Error != null)
            {
                if (PacketArrived != null)
                    PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(args.Error));

                return;
            }

            _bytesReceived += args.Result;

            if (args.Result == 0)
            {
                if (PacketArrived != null)
                    PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(null as byte[]));
                return;
            }

            if (_dataBuffer == null)
            {
                if (_bytesReceived != _protocolReader.LengthBytes)
                {
                    ContinueReading();
                }
                else
                {
                    int length = _protocolReader.ParseLength(_lengthBuffer);

                    _dataBuffer = new byte[length];
                    _bytesReceived = 0;
                    ContinueReading();
                }
            }
            else
            {
                if (_bytesReceived != _dataBuffer.Length)
                {
                    ContinueReading();
                }
                else
                {
                    if (PacketArrived != null)
                        PacketArrived.Invoke(new AsyncResultEventArgs<byte[]>(_dataBuffer));

                    _dataBuffer = null;
                    _bytesReceived = 0;
                    ContinueReading();
                }
            }
        }
        #endregion
        #region Method: ParseByteData
        /// <summary>
        /// Parses a byte array as a packet.
        /// </summary>
        /// <param name="data">The byte array to parse.</param>
        public GameSocket ParseByteData(byte[] data)
        {
            IncomingMessage message = _protocolReader.ParseMessage(data);
            PacketHandlers.Invoke(Habbo, message);

            return this;
        }
        #endregion
        #region Method: ParsePacket
        private void ParsePacket(AsyncResultEventArgs<byte[]> args)
        {
            try
            {
                if (args.Error != null)
                    throw args.Error;

                if (args.Result == null)
                {
                    if (Habbo.LoggedIn)
                        Habbo.LoggedIn = false;
                    CoreManager.ServerCore.StandardOut.PrintNotice("Client Connection Closed: Gracefully close.");
                    Disconnect();
                    return;
                }

                ParseByteData(args.Result);
            }
            catch (Exception)
            {
                if (args.Error != null)
                {
                    CoreManager.ServerCore.StandardOut.PrintError("Client Connection Killed: Socket read error!");
                    CoreManager.ServerCore.StandardOut.PrintException(args.Error);
                }
            }

            return;
        }
        #endregion

        #region Method: Send
        public GameSocket Send(byte[] data)
        {
            _internalSocket.WriteAsync(data);
            return this;
        }

        #endregion

        #region Method: ToString
        public override string ToString()
        {
            return _internalSocket.ToString();
        }
        #endregion
        #endregion
    }
}