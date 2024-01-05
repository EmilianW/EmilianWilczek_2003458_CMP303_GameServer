// Emilian Wilczek 2003458
// Based on packet file provided for Unity C# Networking tutorial by Tom Weiland

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ServerPackets
{
    Welcome = 1,
    SpawnPlayer,
    PlayerPosition,
    PlayerRotation,
    PlayerDisconnected,
    PlayerHealth,
    PlayerRespawned
}

public enum ClientPackets
{
    WelcomeReceived = 1,
    PlayerMovement,
    PlayerShoot
}

public sealed class Packet : IDisposable
{
    private List<byte> _buffer;

    private bool _disposed;
    private byte[] _readableBuffer;
    private int _readPos;
    
    public Packet()
    {
        _buffer = new List<byte>();
        _readPos = 0;
    }
    
    public Packet(int id)
    {
        _buffer = new List<byte>();
        _readPos = 0;

        Write(id);
    }
    
    public Packet(byte[] data)
    {
        _buffer = new List<byte>();
        _readPos = 0;

        SetBytes(data);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _buffer = null;
            _readableBuffer = null;
            _readPos = 0;
        }

        _disposed = true;
    }

    #region Functions
    
    public void SetBytes(byte[] data)
    {
        Write(data);
        _readableBuffer = _buffer.ToArray();
    }
    
    public void WriteLength()
    {
        _buffer.InsertRange(0,
            BitConverter.GetBytes(_buffer.Count));
    }
    
    public void InsertInt(int value)
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(value));
    }
    
    public byte[] ToArray()
    {
        _readableBuffer = _buffer.ToArray();
        return _readableBuffer;
    }
    
    public int Length()
    {
        return _buffer.Count;
    }
    
    public int UnreadLength()
    {
        return Length() - _readPos;
    }
    
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            _buffer.Clear();
            _readableBuffer = null;
            _readPos = 0;
        }
        else
        {
            _readPos -= 4; // "Unread" the last read int
        }
    }

    #endregion

    #region Write Data
    
    public void Write(byte value)
    {
        _buffer.Add(value);
    }

    private void Write(byte[] value)
    {
        _buffer.AddRange(value);
    }
    
    public void Write(short value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void Write(int value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void Write(long value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void Write(float value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void Write(bool value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void Write(string value)
    {
        Write(value.Length);
        _buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }
    
    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }
    
    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    #endregion

    #region Read Data
    
    public byte ReadByte(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'byte'!");
        var value = _readableBuffer[_readPos];
        if (moveReadPos) _readPos += 1;
        return value;

    }
    
    public byte[] ReadBytes(int length, bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'byte[]'!");
        var value = _buffer.GetRange(_readPos, length).ToArray();
        if (moveReadPos) _readPos += length;
        return value;

    }
    public short ReadShort(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'short'!");
        var value = BitConverter.ToInt16(_readableBuffer, _readPos);
        if (moveReadPos) _readPos += 2;
        return value;
    }
    
    public int ReadInt(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'int'!");
        var value = BitConverter.ToInt32(_readableBuffer, _readPos);
        if (moveReadPos) _readPos += 4;
        return value;
    }
    
    public long ReadLong(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'long'!");
        var value = BitConverter.ToInt64(_readableBuffer, _readPos);
        if (moveReadPos) _readPos += 8;
        return value;
    }

    private float ReadFloat(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'float'!");
        var value = BitConverter.ToSingle(_readableBuffer, _readPos);
        if (moveReadPos) _readPos += 4;
        return value;
    }
    
    public bool ReadBool(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'bool'!");
        var value = BitConverter.ToBoolean(_readableBuffer, _readPos);
        if (moveReadPos) _readPos += 1;
        return value;

    }
    
    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            var length = ReadInt();
            var value = Encoding.ASCII.GetString(_readableBuffer, _readPos, length);
            if (moveReadPos && value.Length > 0) _readPos += length;
            return value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }
    
    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }
    
    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos),
            ReadFloat(moveReadPos));
    }

    #endregion
}