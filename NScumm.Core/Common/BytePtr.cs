//
//  BytePtr.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2016 scemino
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace NScumm.Core
{
    public struct BytePtr
    {
        public int Offset;
        public byte[] Data;

        public static readonly BytePtr Null = new BytePtr(null);

        public byte Value
        {
            get { return Data[Offset]; }
            set { Data[Offset] = value; }
        }

        public byte this[int index]
        {
            get { return Data[Offset + index]; }
            set { Data[Offset + index] = value; }
        }

        public BytePtr(BytePtr ptr, int offset)
        {
            Data = ptr.Data;
            Offset = ptr.Offset + offset;
        }

        public BytePtr(byte[] data, int offset = 0)
        {
            Data = data;
            Offset = offset;
        }

        public static implicit operator BytePtr(ByteAccess ba)
        {
            return new BytePtr(ba.Data, ba.Offset);
        }

        public static implicit operator BytePtr(byte[] ba)
        {
            return new BytePtr(ba);
        }

        public static implicit operator ByteAccess(BytePtr p)
        {
            return new ByteAccess(p.Data, p.Offset);
        }

        public static bool operator ==(BytePtr p1, BytePtr p2)
        {
            return p1.Data == p2.Data &&
                     p1.Offset == p2.Offset;
        }

        public static bool operator !=(BytePtr p1, BytePtr p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BytePtr)) return false;
            return this == (BytePtr)obj;
        }

        public override int GetHashCode()
        {
            return Data == null ? 0 : Data.GetHashCode() ^ Offset ;
        }
    }
    
}