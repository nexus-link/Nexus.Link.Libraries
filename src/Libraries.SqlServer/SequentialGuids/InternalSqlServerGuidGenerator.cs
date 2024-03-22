using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

namespace Nexus.Link.Libraries.SqlServer.SequentialGuids;

/// <summary>
/// Optimized Guid generator for SQL Server.
/// </summary>
/// Source: https://github.com/phatboyg/NewId/tree/develop/src/NewId (simplified and modified by IRM)
internal class InternalSqlServerGuidGenerator : IGuidGenerator
{
    private readonly int _c;
    private readonly int _d;
    private readonly short _gb;
    private readonly short _gc;
    private int _a;
    private int _b;
    private long _lastTick;
    private int _sequence;

    private SpinLock _spinLock = new SpinLock(false);

    /// <summary>
    /// A guid generator specifically made for Microsoft SQL Server
    /// </summary>
    /// <param name="workerIndex"></param>
    /// <param name="useProcessId"></param>
    public InternalSqlServerGuidGenerator(int workerIndex = 0, bool useProcessId = false)
    {
        var workerId = GetWorkerId(workerIndex);

        _c = workerId[0] << 24 | workerId[1] << 16 | workerId[2] << 8 | workerId[3];

        if (useProcessId)
        {
            var processId = GetProcessId();
            _d = processId[0] << 24 | processId[1] << 16;
        }
        else
        {
            _d = workerId[4] << 24 | workerId[5] << 16;
        }

        _gb = (short)_c;
        _gc = (short)(_c >> 16);
    }

    #region Initialization
    private static byte[] GetProcessId()
    {
        var processId = BitConverter.GetBytes(Process.GetCurrentProcess().Id);

        if (processId.Length < 2)
            throw new InvalidOperationException("Current Process Id is of insufficient length");

        return processId;
    }

    public byte[] GetWorkerId(int index)
    {
        var exceptions = new List<Exception>();

        try
        {
            return GetNetworkAddressWorkerId(index);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            return GetHostNameWorkerId(index);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        throw new AggregateException(exceptions);
    }

    private byte[] GetHostNameWorkerId(int index)
    {
        try
        {
            var hostName = Dns.GetHostName();

            byte[] hash;
            using (var hasher = SHA1.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            }

            var bytes = new byte[6];
            Buffer.BlockCopy(hash, 12, bytes, 0, 6);
            bytes[0] |= 0x80;

            return bytes;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to retrieve hostname", ex);
        }
    }

    private byte[] GetNetworkAddressWorkerId(int index)
    {
        return GetNetworkAddress(index);
    }

    static byte[] GetNetworkAddress(int index)
    {
        var network = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                || x.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                || x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx
                || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT)
            .Select(x => x.GetPhysicalAddress())
            .Where(x => x != null)
            .Select(x => x.GetAddressBytes())
            .Where(x => x.Length == 6)
            .Skip(index)
            .FirstOrDefault();

        if (network == null)
            throw new InvalidOperationException("Unable to find usable network adapter for unique address");

        return network;
    }

    #endregion

    public Guid NewGuid()
    {
        var ticks = DateTime.UtcNow.Ticks;

        int a;
        int b;
        int sequence;

        var lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);

            if (ticks > _lastTick)
                UpdateTimestamp(ticks);
            else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                UpdateTimestamp(_lastTick + 1);

            sequence = _sequence++;

            a = _a;
            b = _b;
        }
        finally
        {
            if (lockTaken)
                _spinLock.Exit();
        }

        var d = (byte)(b >> 8);
        var e = (byte)b;
        var f = (byte)(a >> 24);
        var g = (byte)(a >> 16);
        var h = (byte)(a >> 8);
        var i = (byte)a;
        var j = (byte)(b >> 24);
        var k = (byte)(b >> 16);

        // swapping high and low byte, because SQL-server is doing the wrong ordering otherwise
        var sequenceSwapped = (sequence << 8 | sequence >> 8 & 0x00FF) & 0xFFFF;

        return new Guid(_d | sequenceSwapped, _gb, _gc, d, e, f, g, h, i, j, k);
    }


    void UpdateTimestamp(long tick)
    {
        _b = (int)(tick & 0xFFFFFFFF);
        _a = (int)(tick >> 32);

        _sequence = 0;
        _lastTick = tick;
    }
}