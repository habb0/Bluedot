﻿#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

using System.Collections.Generic;
using Bluedot.HabboServer.Habbos;
using Bluedot.HabboServer.Network;

namespace Bluedot.HabboServer.ApiUsage.Packets
{
    public class MBadgeListing : OutgoingMessage
    {
        #region Property: AllBadges
        /// <summary>
        /// 
        /// </summary>
        public ICollection<BadgeType> AllBadges
        {
            get;
            set;
        }
        #endregion
        #region Property: BadgeSlots
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<BadgeSlot, BadgeType> BadgeSlots
        {
            get;
            set;
        }
        #endregion

        public override OutgoingMessage Send(IMessageable target)
        {
            if (InternalOutgoingMessage.ID == 0)
            {
                InternalOutgoingMessage.Initialize(229)
                    .AppendInt32(AllBadges.Count);
                foreach (BadgeType badge in AllBadges)
                {
                    InternalOutgoingMessage
                        .AppendString(badge.Code);
                }
                foreach (KeyValuePair<BadgeSlot, BadgeType> slotBadge in BadgeSlots)
                {
                    InternalOutgoingMessage
                        .AppendInt32((int) slotBadge.Key)
                        .AppendString(slotBadge.Value.Code);
                }
            }

            target.SendMessage(InternalOutgoingMessage);
            return this;
        }
    }
}