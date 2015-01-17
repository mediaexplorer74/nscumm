//
//  ResourceFile4.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using NScumm.Core.Graphics;
using System.Linq;
using System.Runtime.InteropServices;

namespace NScumm.Core.IO
{
    class ResourceFile7: ResourceFile6
    {
        public ResourceFile7(string path)
            : base(path, 0)
        {
        }

        protected override RoomHeader ReadRMHD()
        {
            var version = _reader.ReadUInt32();
            var header = new RoomHeader
            {
                Width = _reader.ReadUInt16(),
                Height = _reader.ReadUInt16(),
                NumObjects = _reader.ReadUInt16()
            };
            return header;
        }

        protected override void ReadImageHeader(ObjectData od)
        {
            // image header
            var version = _reader.ReadUInt32();
            od.Number = _reader.ReadUInt16();
            var numImnn = _reader.ReadUInt16();
            od.Position = new Point(_reader.ReadInt16(), _reader.ReadInt16());
            od.Width = _reader.ReadUInt16();
            od.Height = _reader.ReadUInt16();
            var unknown1 = _reader.ReadBytes(3);
            od.ActorDir = _reader.ReadByte();
            var numHotspots = _reader.ReadUInt16();
            for (int i = 0; i < numHotspots; i++)
            {
                od.Hotspots.Add(new Point(_reader.ReadInt16(), _reader.ReadInt16()));
            }
        }

        protected override ObjectData ReadCDHD()
        {
            var version = _reader.ReadUInt32();
            var obj = new ObjectData
            {
                Number = _reader.ReadUInt16(),
                Parent = _reader.ReadByte(),
                ParentState = _reader.ReadByte()
            };
            obj.Flags = DrawBitmaps.AllowMaskOr;
            return obj;
        }

        protected override int ReadScriptIndex()
        {
            return _reader.ReadUInt16();
        }

        protected override int GetNumGlobalScripts()
        {
            return 2000;
        }

        public override byte[] ReadCostume(long offset)
        {
            GotoResourceHeader(offset);
            var chunk = ChunkIterator5.ReadChunk(_reader);
            if (chunk.Tag != "AKOS")
                throw new NotSupportedException("Expected AKOS block.");
            return _reader.ReadBytes((int)chunk.Size - 8);
        }

        public static byte[] ReadData(byte[] input, string tag)
        {
            using (var ms = new MemoryStream(input))
            {
                var reader = new XorReader(ms, 0);
                var it = new ChunkIterator5(reader, input.Length);
                while (it.MoveNext())
                {
                    if (it.Current.Tag == tag)
                    {
                        return reader.ReadBytes((int)(it.Current.Size - 8));
                    }
                }
            }
            return null;
        }

        public static long FindOffset(byte[] input, string tag)
        {
            using (var ms = new MemoryStream(input))
            {
                var reader = new XorReader(ms, 0);
                var it = new ChunkIterator5(reader, input.Length);
                while (it.MoveNext())
                {
                    if (it.Current.Tag == tag)
                    {
                        return reader.BaseStream.Position;
                    }
                }
            }
            return -1;
        }

        public static T ReadData<T>(byte[] input, string tag)
        {
            return (T)ReadData(input, tag, typeof(T));
        }

        public static object ReadData(byte[] input, string tag, Type type)
        {
            var data = ReadData(input, tag);
            if (data == null)
                return null;

            return ToStructure(data, 0, type);
        }

        public static object ToStructure(byte[] data, int offset, Type type)
        {
            object obj;
            var size = Marshal.SizeOf(type);
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(data, offset, ptr, size);
                obj = Marshal.PtrToStructure(ptr, type);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return obj;
        }

        public static T ToStructure<T>(byte[] data, int offset)
        {
            return (T)ToStructure(data, offset, typeof(T));
        }
    }
 
}