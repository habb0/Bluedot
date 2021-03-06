﻿using System;

using Bluedot.HabboServer.Events;
using Bluedot.HabboServer.Habbos;
using Bluedot.HabboServer.Network;
using Bluedot.HabboServer.Useful;

namespace Bluedot.HabboServer.ApiUsage.Plugins.DefaultLoginFunctions
{
    internal static partial class PacketHandlers
    {
        internal static void ProcessSSOTicket(Habbo sender, IncomingMessage message)
        {
            ClassicIncomingMessage classicMessage = (ClassicIncomingMessage)message;

            Habbo fullHabbo = CoreManager.ServerCore.HabboDistributor.GetHabboFromSSOTicket(classicMessage.PopPrefixedString());

            if (fullHabbo == null)
            {
                new MConnectionClosed
                {
                    Reason = ConnectionClosedReason.InvalidSSOTicket
                }.Send(sender);
                
                sender.Socket.Disconnect("Invalid SSO Ticket");
            }
            else
            {
                // If this Habbo is already logged in...
                if (fullHabbo.LoggedIn)
                {
                    // Disconnect them.
                    new MConnectionClosed
                        {
                            Reason = ConnectionClosedReason.ConcurrentLogin
                        }.Send(fullHabbo);
                    fullHabbo.Socket.Disconnect("Concurrent Login");
                }

                LoginMerge(fullHabbo, sender);
            }
        }


        #region Method: LoginMerge
        private static void LoginMerge(Habbo fullHabbo, Habbo connectionHabbo)
        {
            CancelReasonEventArgs eventArgs = new CancelReasonEventArgs();
            CoreManager.ServerCore.EventManager.Fire("habbo_login", EventPriority.Before, fullHabbo, eventArgs);

            if (eventArgs.Cancel)
            {
                if (connectionHabbo.Socket != null)
                    connectionHabbo.Socket.Disconnect(eventArgs.CancelReason);
                return;
            }

            connectionHabbo.Socket.Habbo = fullHabbo;
            fullHabbo.Socket = connectionHabbo.Socket;

            fullHabbo.LastAccess = DateTime.Now;
            CoreManager.ServerCore.EventManager.Fire("habbo_login", EventPriority.After, fullHabbo, eventArgs);
        }
        #endregion
    }
}
