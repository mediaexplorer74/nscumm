﻿/*
 * This file is part of NScumm.
 * 
 * NScumm is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * NScumm is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with NScumm.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Scumm4
{
    public class XorReader
    {
        private BinaryReader _reader;
        private byte _xor;

        public Stream BaseStream { get { return _reader.BaseStream; } }

        public XorReader(BinaryReader reader, byte xor)
        {
            _reader = reader;
            _xor = xor;
        }

        public byte PeekByte()
        {
            var data = (byte)(_reader.ReadByte() ^ _xor);
            _reader.BaseStream.Seek(-1, SeekOrigin.Current);
            return data;
        }

        public byte ReadByte()
        {
            return (byte)(_reader.ReadByte() ^ _xor);
        }

        public byte[] ReadBytes(int count)
        {
            byte[] data = _reader.ReadBytes(count);
            for (int i = 0; i < count; i++)
            {
                data[i] ^= _xor;
            }
            return data;
        }

        public short ReadInt16()
        {
            var data = _reader.ReadBytes(2);
            var value = data[0] ^ _xor | ((data[1] ^ _xor) << 8);
            return (short)value;
        }

        public ushort ReadUInt16()
        {
            var data = _reader.ReadBytes(2);
            var value = data[0] ^ _xor | ((data[1] ^ _xor) << 8);
            return (ushort)value;
        }

        public ushort PeekUint16()
        {
            var data = _reader.ReadBytes(2);
            _reader.BaseStream.Seek(-2, SeekOrigin.Current);
            var value = data[0] ^ _xor | ((data[1] ^ _xor) << 8);
            return (ushort)value;
        }

        public int ReadInt32()
        {
            var data = _reader.ReadBytes(4);
            return ToInt32(data[0] ^ _xor, data[1] ^ _xor, data[2] ^ _xor, data[3] ^ _xor);
        }

        public uint ReadUInt32()
        {
            var data = _reader.ReadBytes(4);
            for (int i = 0; i < 4; i++)
            {
                data[i] = (byte)(data[i] ^ _xor);
            }
            return ToUInt32(data);
        }

        private static int ToInt32(int b0, int b1, int b2, int b3)
        {
            int value = (b0) | (b1 << 8) | (b2 << 16) | (b3 << 24);
            return value;
        }

        private static uint ToUInt32(byte[] data)
        {
            uint value = data[0] | ((uint)data[1] << 8) | ((uint)data[2] << 16) | ((uint)data[3] << 24);
            return value;
        }
    }
}
