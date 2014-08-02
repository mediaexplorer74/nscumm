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

using NScumm.Core.Graphics;
using System.Collections.Generic;

namespace NScumm.Core
{
    public class RoomHeader
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort NumObjects { get; set; }
    }

    public class Room
    {
        public RoomHeader Header { get; set; }
        public Palette Palette { get; private set; }
        public ScriptData[] LocalScripts { get; private set; }
        public ScriptData EntryScript { get; private set; }
        public ScriptData ExitScript { get; private set; }
        public List<ObjectData> Objects { get; private set; }
        public List<Box> Boxes { get; private set; }
        public List<byte> BoxMatrix { get; private set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public ScaleSlot[] Scales { get; set; }
        public byte TransparentColor { get; set; }
        public bool HasPalette { get; set; }
        public ColorCycle[] ColorCycle { get; set; }

        public Room()
        {
            this.Boxes = new List<Box>();
            this.Objects = new List<ObjectData>();
            this.BoxMatrix = new List<byte>();
            this.Palette = new Palette();
            this.EntryScript = new ScriptData();
            this.ExitScript = new ScriptData();
            this.LocalScripts = new ScriptData[1024];
            this.TransparentColor = 255;
            this.Scales = new ScaleSlot[0];
            this.ColorCycle = new ColorCycle[16];
			for (int i = 0; i < ColorCycle.Length; i++) {
				this.ColorCycle [i] = new NScumm.Core.ColorCycle ();
			}
        }
    }
}
