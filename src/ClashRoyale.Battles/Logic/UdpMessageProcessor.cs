﻿using System;
using System.Linq;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using SharpRaven.Data;

namespace ClashRoyale.Battles.Logic
{
    public class UdpMessageProcessor
    {
        public static async void Process(IChannelHandlerContext ctx, DatagramPacket packet)
        {
            var content = packet.Content;
            var length = content.ReadableBytes;

            Logger.Log($"Received {length} bytes", null, ErrorLevel.Debug);

            if (length == 1400)
            {
                var sessionId = content.ReadLong();
                var uselessBytes = content.ReadBytes(2);

                var sessionBuffer = Unpooled.Buffer();
                sessionBuffer.WriteLong(sessionId);
                sessionBuffer.WriteBytes(uselessBytes);

                Resources.Players.Add(new SessionContext
                {
                    PlayerId = sessionId,
                    EndPoint = packet.Sender
                });

                await ctx.WriteAsync(new DatagramPacket(sessionBuffer, packet.Sender));

                Logger.Log("OK!", null, ErrorLevel.Debug);
            }
            else
            {
                var sessionId = content.ReadLong();
                content.ReadBytes(2);

                var sessionContext = Resources.Players.Get(sessionId);

                if (sessionContext != null)
                {
                    sessionContext.Active = true;
                    sessionContext.Process(content);
                }
                else
                {
                    Logger.Log("Player not logged in.", null, ErrorLevel.Debug);
                }
            }
        }
    }
}
