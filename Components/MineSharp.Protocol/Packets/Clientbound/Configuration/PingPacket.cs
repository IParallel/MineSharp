﻿using MineSharp.Core.Common;
using MineSharp.Data;
using MineSharp.Data.Protocol;

namespace MineSharp.Protocol.Packets.Clientbound.Configuration;
#pragma warning disable CS1591
/// <summary>
///     Ping packet
/// </summary>
public class PingPacket : IPacket
{
    /// <summary>
    ///     Create a new instance
    /// </summary>
    /// <param name="id"></param>
    public PingPacket(int id)
    {
        Id = id;
    }

    /// <summary>
    ///     The id of the ping
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public PacketType Type => PacketType.CB_Configuration_Ping;

    /// <inheritdoc />
    public void Write(PacketBuffer buffer, MinecraftData version)
    {
        buffer.WriteInt(Id);
    }

    /// <inheritdoc />
    public static IPacket Read(PacketBuffer buffer, MinecraftData version)
    {
        return new PingPacket(
            buffer.ReadInt());
    }
}
#pragma warning restore CS1591
