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
using NScumm.Core.IO;
using System;
using System.Linq;

namespace NScumm.Core
{
    [Flags]
    enum MoveFlags
    {
        None = 0,
        NewLeg = 1,
        InLeg = 2,
        Turn = 4,
        LastLeg = 8,
        Frozen = 0x80
    }

    [Flags]
    enum ObjectClass
    {
        NeverClip = 20,
        AlwaysClip = 21,
        IgnoreBoxes = 22,
        YFlip = 29,
        XFlip = 30,
        Player = 31,
        // Actor is controlled by the player
        Untouchable = 32
    }

    struct AdjustBoxResult
    {
        public Point Position;
        public byte Box;
    }

    class Actor
    {
        #region Constants

        public const int InvalidBox = 0xFF;

        #endregion

        #region Private Fields

        /// <summary>
        /// The position of the actor inside the virtual screen.
        /// </summary>
        Point _position;

        protected ScummEngine _scumm;
        protected ushort _targetFacing;
        protected ActorWalkData _walkdata;

        ushort[] _palette = new ushort[256];
        int _elevation;
        ushort _facing;
        uint _speedx, _speedy;
        byte _animProgress, _animSpeed;
        bool _costumeNeedsInit;
        short[] _animVariable = new short[27];
        ushort _talkScript, _walkScript;
        bool _ignoreTurns;
        Point _talkPos;
        bool _flip;
        int _talkFrequency;
        byte _talkVolume;
        byte _talkPan;
        byte _frame;

        #endregion

        #region Public Fields

        public int Top, Bottom;
        public uint Width;

        public ushort BoxScale;
        public byte ScaleX, ScaleY;
        public byte Charset;
        public byte ForceClip;

        public CostumeData Cost;

        #endregion

        #region Properties

        public int Sound { get; set; }

        public bool NeedRedraw { get; set; }

        public bool NeedBackgroundReset { get; set; }

        public byte Number { get; set; }

        public byte[] Name { get; set; }

        public bool IsVisible { get; private set; }

        public ushort Costume { get; private set; }

        public byte InitFrame { get; set; }

        public byte WalkFrame { get; set; }

        public byte StandFrame { get; set; }

        public byte TalkStartFrame { get; set; }

        public byte TalkStopFrame { get; set; }

        public byte Room { get; set; }

        public Point Position
        {
            get { return _position; }
        }

        public ushort Facing
        {
            get { return _facing; }
            private set { _facing = value; }
        }

        public int Elevation
        {
            get { return _elevation; }
            set
            {
                if (_elevation != value)
                {
                    _elevation = value;
                    NeedRedraw = true;
                }
            }
        }

        public MoveFlags Moving { get; set; }

        public byte Walkbox { get; set; }

        public byte ShadowMode { get; set; }

        public byte TalkColor { get; set; }

        public bool IgnoreBoxes { get; set; }

        public bool IsInCurrentRoom
        {
            get { return Room == _scumm.CurrentRoom; }
        }

        public bool IsPlayer
        {
            get{ return IsInClass(ObjectClass.Player); }
        }

        #endregion

        #region ActorWalkData Structures

        protected struct ActorWalkData
        {
            public Point Dest;
            // Final destination point
            public byte DestBox;
            // Final destination box
            public short DestDir;
            // Final destination, direction to face at

            public Point Cur;
            // Last position
            public byte CurBox;
            // Last box

            public Point Next;
            // Next position on our way to the destination, i.e. our intermediate destination

            public Point Point3;
            public int DeltaXFactor, DeltaYFactor;
            public ushort XFrac, YFrac;
        }

        #endregion

        #region Constructor

        public Actor(ScummEngine scumm, byte id)
        {
            _scumm = scumm;
            Number = id;
        }

        #endregion

        #region Public Methods

        public void Show()
        {
            if (_scumm.CurrentRoom == 0 || IsVisible)
                return;

            AdjustActorPos();

            // TODO:
            //_vm->ensureResourceLoaded(rtCostume, _costume);

            if (_costumeNeedsInit)
            {
                StartAnimActor(InitFrame);
                _costumeNeedsInit = false;
            }

            StopActorMoving();
            IsVisible = true;
            NeedRedraw = true;
        }

        public void Hide()
        {
            if (!IsVisible)
                return;

            if (Moving != MoveFlags.None)
            {
                StopActorMoving();
                StartAnimActor(StandFrame);
            }

            IsVisible = false;
            Cost.SoundCounter = 0;
            Cost.SoundPos = 0;
            NeedRedraw = false;
            NeedBackgroundReset = true;
        }

        public void Init(int mode)
        {
            Name = null;
        
            if (mode == -1)
            {
                Top = Bottom = 0;
                NeedRedraw = false;
                NeedBackgroundReset = false;
                _costumeNeedsInit = false;
                IsVisible = false;
                _flip = false;
                _speedx = 8;
                _speedy = 2;
                _frame = 0;
                Walkbox = 0;
                _animProgress = 0;
                _animVariable = new short[27];
                _palette = new ushort[256];
                Sound = 0;
                Cost = new CostumeData();
                _walkdata = new ActorWalkData();
                _walkdata.Point3.X = 32000;
                _walkScript = 0;
            
                Costume = 0;
                Room = 0;
                _position.X = 0;
                _position.Y = 0;
                _facing = 180;
            }

            _elevation = 0;
            Width = 24;
            TalkColor = 15;
            _talkPos = new Point(0, -80);
            BoxScale = ScaleY = ScaleX = 0xFF;
            Charset = 0;
            Sound = 0;
            _targetFacing = _facing;

            ShadowMode = 0;

            StopActorMoving();

            SetActorWalkSpeed(8, 2);

            _animSpeed = 0;

            IgnoreBoxes = false;
            ForceClip = 0;
            _ignoreTurns = false;

            _talkFrequency = 256;
            _talkPan = 64;
            _talkVolume = 127;

            ResetFrames();

            _walkScript = 0;
            _talkScript = 0;

            _scumm.ClassData[Number] = 0;
        }

        public void ResetFrames()
        {
            InitFrame = 1;
            WalkFrame = 2;
            StandFrame = 3;
            TalkStartFrame = 4;
            TalkStopFrame = 5;
        }

        public void PutActor()
        {
            PutActor(_position, Room);
        }

        public void PutActor(byte room)
        {
            PutActor(_position, room);
        }

        public void PutActor(Point pos)
        {
            PutActor(pos, Room);
        }

        public void PutActor(Point pos, byte newRoom)
        {
            if (IsVisible && _scumm.CurrentRoom != newRoom && _scumm.TalkingActor == Number)
            {
                _scumm.StopTalk();
            }

            _position = pos;
            Room = newRoom;
            NeedRedraw = true;

            if (_scumm.Variables[_scumm.VariableEgo.Value] == Number)
            {
                _scumm.EgoPositioned = true;
            }

            if (IsVisible)
            {
                if (IsInCurrentRoom)
                {
                    if (Moving != MoveFlags.None)
                    {
                        StopActorMoving();
                        StartAnimActor(StandFrame);
                    }
                    AdjustActorPos();
                }
                else
                {
                    Hide();
                }
            }
            else
            {
                if (IsInCurrentRoom)
                    Show();
            }
        }

        public void SetActorCostume(ushort costume)
        {
            _costumeNeedsInit = true;

            if (IsVisible)
            {
                Hide();
                Cost.Reset();
                Costume = costume;
                Show();
            }
            else
            {
                Costume = costume;
                Cost.Reset();
            }

            if (_scumm.Game.IsOldBundle)
            {
                for (int i = 0; i < 16; i++)
                    _palette[i] = (byte)i;
            }
            else
            {
                for (int i = 0; i < 32; i++)
                    _palette[i] = 0xFF;
            }
        }

        public void SetActorWalkSpeed(uint newSpeedX, uint newSpeedY)
        {
            if (newSpeedX == _speedx && newSpeedY == _speedy)
                return;

            _speedx = newSpeedX;
            _speedy = newSpeedY;

            if (Moving != MoveFlags.None)
            {
                CalcMovementFactor(_walkdata.Next);
            }
        }

        public AdjustBoxResult AdjustXYToBeInBox(Point dst)
        {
            var thresholdTable = new [] { 30, 80, 0 };
            var abr = new AdjustBoxResult();
            uint tmpDist, bestDist;
            int threshold, numBoxes;
            BoxFlags flags;
            byte bestBox;
            int box;
            int firstValidBox = _scumm.Game.Version < 5 ? 0 : 1;

            abr.Position = dst;
            abr.Box = InvalidBox;

            if (IgnoreBoxes)
                return abr;

            for (int tIdx = 0; tIdx < thresholdTable.Length; tIdx++)
            {
                threshold = thresholdTable[tIdx];

                numBoxes = _scumm.GetNumBoxes() - 1;
                if (numBoxes < firstValidBox)
                    return abr;

                bestDist = 0xFFFF;
                bestBox = InvalidBox;

                // We iterate (backwards) over all boxes, searching the one closest
                // to the desired coordinates.
                for (box = numBoxes; box >= firstValidBox; box--)
                {
                    flags = _scumm.GetBoxFlags((byte)box);

                    // Skip over invisible boxes
                    if (flags.HasFlag(BoxFlags.Invisible) && !(flags.HasFlag(BoxFlags.PlayerOnly)
                        && !IsPlayer))
                        continue;

                    // For increased performance, we perform a quick test if
                    // the coordinates can even be within a distance of 'threshold'
                    // pixels of the box.
                    if (threshold > 0 && _scumm.GetBoxCoordinates(box).InBoxQuickReject(dst, threshold))
                        continue;

                    // Check if the point is contained in the box. If it is,
                    // we don't have to search anymore.
                    if (_scumm.CheckXYInBoxBounds(box, dst))
                    {
                        abr.Position = dst;
                        abr.Box = (byte)box;
                        return abr;
                    }

                    // Find the point in the box which is closest to our point.
                    Point pTmp;
                    tmpDist = ScummMath.GetClosestPtOnBox(_scumm.GetBoxCoordinates(box), dst, out pTmp);

                    // Check if the box is closer than the previous boxes.
                    if (tmpDist < bestDist)
                    {
                        abr.Position = pTmp;

                        if (tmpDist == 0)
                        {
                            abr.Box = (byte)box;
                            return abr;
                        }

                        bestDist = tmpDist;
                        bestBox = (byte)box;
                    }
                }

                // If the closest ('best') box we found is within the threshold, or if
                // we are on the last run (i.e. threshold == 0), return that box.
                if (threshold == 0 || threshold * threshold >= bestDist)
                {
                    abr.Box = bestBox;
                    return abr;
                }
            }

            return abr;
        }

        public void SetDirection(int direction)
        {
            uint aMask;
            int i;
            ushort vald;

            // Do nothing if actor is already facing in the given direction
            if (_facing == direction)
                return;

            // Normalize the angle
            _facing = (ushort)ScummMath.NormalizeAngle(direction);

            // If there is no costume set for this actor, we are finished
            if (Costume == 0)
                return;

            // Update the costume for the new direction (and mark the actor for redraw)
            aMask = 0x8000;
            for (i = 0; i < 16; i++, aMask >>= 1)
            {
                vald = Cost.Frame[i];
                if (vald == 0xFFFF)
                    continue;
                _scumm.CostumeLoader.CostumeDecodeData(this, vald, aMask);
            }

            NeedRedraw = true;
        }

        public void FaceToObject(int obj)
        {
            int dir;

            if (!IsInCurrentRoom)
                return;

            Point p;
            if (!_scumm.GetObjectOrActorXY(obj, out p))
                return;

            dir = (p.X > _position.X) ? 90 : 270;
            TurnToDirection(dir);
        }

        public virtual void Walk()
        {
            int new_dir, next_box;
            Point foundPath;

            if (Moving == MoveFlags.None)
                return;

            if (!Moving.HasFlag(MoveFlags.NewLeg))
            {
                if (Moving.HasFlag(MoveFlags.InLeg) && ActorWalkStep() != 0)
                    return;

                if (Moving.HasFlag(MoveFlags.LastLeg))
                {
                    Moving = MoveFlags.None;
                    SetBox(_walkdata.DestBox);
                    StartAnimActor(StandFrame);
                    if (_targetFacing != _walkdata.DestDir)
                        TurnToDirection(_walkdata.DestDir);
                    return;
                }

                if (Moving.HasFlag(MoveFlags.Turn))
                {
                    new_dir = UpdateActorDirection(false);
                    if (_facing != new_dir)
                        SetDirection(new_dir);
                    else
                        Moving = MoveFlags.None;
                    return;
                }

                SetBox(_walkdata.CurBox);
                Moving &= MoveFlags.InLeg;
            }

            Moving &= ~MoveFlags.NewLeg;
            do
            {
                if (Walkbox == InvalidBox)
                {
                    SetBox(_walkdata.DestBox);
                    _walkdata.CurBox = _walkdata.DestBox;
                    break;
                }

                if (Walkbox == _walkdata.DestBox)
                    break;

                next_box = _scumm.GetNextBox(Walkbox, _walkdata.DestBox);
                if (next_box < 0)
                {
                    _walkdata.DestBox = Walkbox;
                    Moving |= MoveFlags.LastLeg;
                    return;
                }

                _walkdata.CurBox = (byte)next_box;

                if (FindPathTowards(Walkbox, (byte)next_box, _walkdata.DestBox, out foundPath))
                    break;

                if (CalcMovementFactor(foundPath) != 0)
                    return;

                SetBox(_walkdata.CurBox);
            } while (true);

            Moving |= MoveFlags.LastLeg;
            CalcMovementFactor(_walkdata.Dest);
        }

        public void DrawCostume(bool hitTestMode = false)
        {
            if (Costume == 0)
                return;

            if (!hitTestMode)
            {
                if (!NeedRedraw)
                    return;

                NeedRedraw = false;
            }

            SetupActorScale();

            ICostumeRenderer bcr = _scumm.CostumeRenderer;
            PrepareDrawActorCostume(bcr);

            // If the actor is partially hidden, redraw it next frame.
            if ((bcr.DrawCostume(_scumm.MainVirtScreen, _scumm.Gdi.NumStrips, this) & 1) != 0)
            {
                NeedRedraw = true;
            }

            if (!hitTestMode)
            {
                // Record the vertical extent of the drawn actor
                Top = bcr.DrawTop;
                Bottom = bcr.DrawBottom;
            }
        }

        public void StartWalk(Point dest, int dir)
        {
            AdjustBoxResult abr;

            abr.Position = dest;

            if (!IsInCurrentRoom)
            {
                _position = abr.Position;
                if (!_ignoreTurns && dir != -1)
                    _facing = (ushort)dir;
                return;
            }

            if (IgnoreBoxes)
            {
                abr.Box = InvalidBox;
                Walkbox = InvalidBox;
            }
            else
            {
                if (_scumm.CheckXYInBoxBounds(_walkdata.DestBox, abr.Position))
                {
                    abr.Box = _walkdata.DestBox;
                }
                else
                {
                    abr = AdjustXYToBeInBox(abr.Position);
                }
                if (Moving != MoveFlags.None &&
                    _walkdata.DestDir == dir &&
                    _walkdata.Dest.X == abr.Position.X && _walkdata.Dest.Y == abr.Position.Y)
                    return;
            }

            if (_position == abr.Position)
            {
                if (dir != _facing)
                    TurnToDirection(dir);
                return;
            }

            _walkdata.Dest.X = abr.Position.X;
            _walkdata.Dest.Y = abr.Position.Y;
            _walkdata.DestBox = abr.Box;
            _walkdata.DestDir = (short)dir;
            Moving = (Moving & MoveFlags.InLeg) | MoveFlags.NewLeg;
            _walkdata.Point3.X = 32000;

            _walkdata.CurBox = Walkbox;
        }

        public void SetAnimSpeed(byte newAnimSpeed)
        {
            _animSpeed = newAnimSpeed;
            _animProgress = 0;
        }

        public void SetPalette(int idx, ushort val)
        {
            _palette[idx] = val;
            NeedRedraw = true;
        }

        public void SetScale(int sx, int sy)
        {
            if (sx != -1)
                ScaleX = (byte)sx;
            if (sy != -1)
                ScaleY = (byte)sy;
            NeedRedraw = true;
        }

        public void Animate(int anim)
        {
            int cmd = anim / 4;
            int dir = ScummHelper.OldDirToNewDir(anim % 4);

            // Convert into old cmd code
            cmd = 0x3F - cmd + 2;

            switch (cmd)
            {
                case 2:				// stop walking
                    StartAnimActor(StandFrame);
                    StopActorMoving();
                    break;
                case 3:				// change direction immediatly
                    Moving &= ~MoveFlags.Turn;
                    SetDirection(dir);
                    break;
                case 4:				// turn to new direction
                    TurnToDirection(dir);
                    break;
                default:
                    StartAnimActor((byte)anim);
                    break;
            }
        }

        public void AnimateCostume()
        {
            if (Costume == 0)
                return;

            _animProgress++;
            if (_animProgress >= _animSpeed)
            {
                _animProgress = 0;

                _scumm.CostumeLoader.LoadCostume(Costume);
                if (_scumm.CostumeLoader.IncreaseAnims(this) != 0)
                {
                    NeedRedraw = true;
                }
            }
        }

        public void ClassChanged(ObjectClass cls, bool value)
        {
            if (cls == ObjectClass.AlwaysClip)
                ForceClip = value ? (byte)1 : (byte)0;
            if (cls == ObjectClass.IgnoreBoxes)
                IgnoreBoxes = value;
        }

        public void SaveOrLoad(Serializer serializer)
        {
            var actorEntries = new[]
            {
                LoadAndSaveEntry.Create(reader => _position.X = reader.ReadInt16(), writer => writer.WriteInt16(_position.X), 8),
                LoadAndSaveEntry.Create(reader => _position.Y = reader.ReadInt16(), writer => writer.WriteInt16(_position.Y), 8),
                                                    
                LoadAndSaveEntry.Create(reader => reader.ReadInt16(), writer => writer.WriteInt16(0xCDCD), 32),
                LoadAndSaveEntry.Create(reader => reader.ReadInt16(), writer => writer.WriteInt16(0xCDCD), 32),
                LoadAndSaveEntry.Create(reader => Top = reader.ReadInt16(), writer => writer.WriteInt16(Top), 8),
                LoadAndSaveEntry.Create(reader => Bottom = reader.ReadInt16(), writer => writer.WriteInt16(Bottom), 8),
                LoadAndSaveEntry.Create(reader => _elevation = reader.ReadInt16(), writer => writer.WriteInt16(_elevation), 8),
                LoadAndSaveEntry.Create(reader => Width = reader.ReadUInt16(), writer => writer.WriteUInt16(Width), 8),
                LoadAndSaveEntry.Create(reader => _facing = reader.ReadUInt16(), writer => writer.WriteUInt16(_facing), 8),
                LoadAndSaveEntry.Create(reader => Costume = reader.ReadUInt16(), writer => writer.WriteUInt16(Costume), 8),
                LoadAndSaveEntry.Create(reader => Room = reader.ReadByte(), writer => writer.WriteByte(Room), 8),
                LoadAndSaveEntry.Create(reader => TalkColor = reader.ReadByte(), writer => writer.WriteByte(TalkColor), 8),
                LoadAndSaveEntry.Create(reader => _talkFrequency = reader.ReadInt16(), writer => writer.WriteInt16(_talkFrequency), 16),
                LoadAndSaveEntry.Create(reader => _talkPan = (byte)reader.ReadInt16(), writer => writer.WriteInt16(_talkPan), 24),
                LoadAndSaveEntry.Create(reader => _talkVolume = (byte)reader.ReadInt16(), writer => writer.WriteInt16(_talkVolume), 29),
                LoadAndSaveEntry.Create(reader => BoxScale = reader.ReadUInt16(), writer => writer.WriteUInt16(BoxScale), 34),
                LoadAndSaveEntry.Create(reader => ScaleX = reader.ReadByte(), writer => writer.WriteByte(ScaleX), 8),
                LoadAndSaveEntry.Create(reader => ScaleY = reader.ReadByte(), writer => writer.WriteByte(ScaleY), 8),
                LoadAndSaveEntry.Create(reader => Charset = reader.ReadByte(), writer => writer.WriteByte(Charset), 8),
		            
                // Actor sound grew from 8 to 32 bytes and switched to uint16 in HE games
                LoadAndSaveEntry.Create(
                    reader => Sound = reader.ReadBytes(8).ToArray()[0],
                    writer =>
                    {
                        var sounds = new byte[8];
                        sounds[0] = (byte)Sound;
                        writer.Write(sounds);
                    },
                    8, 36),
                LoadAndSaveEntry.Create(
                    reader => Sound = reader.ReadBytes(32).ToArray()[0],
                    writer =>
                    {
                        var sounds = new byte[32];
                        sounds[0] = (byte)Sound;
                        writer.Write(sounds);
                    },
                    37, 61),
                LoadAndSaveEntry.Create(
                    reader => Sound = (int)reader.ReadUInt16s(32)[0],
                    writer =>
                    {
                        var sounds = new ushort[32];
                        sounds[0] = (ushort)Sound;
                        writer.WriteUInt16s(sounds, 32);
                    },
                    62),
                    
                // Actor animVariable grew from 8 to 27
                LoadAndSaveEntry.Create(reader => _animVariable = reader.ReadInt16s(8), writer => writer.WriteInt16s(_animVariable, 8), 8, 40),
                LoadAndSaveEntry.Create(reader => _animVariable = reader.ReadInt16s(27), writer => writer.WriteInt16s(_animVariable, 27), 41),
                                                   
                LoadAndSaveEntry.Create(reader => _targetFacing = reader.ReadUInt16(), writer => writer.WriteUInt16(_targetFacing), 8),
                LoadAndSaveEntry.Create(reader => Moving = (MoveFlags)reader.ReadByte(), writer => writer.WriteByte((byte)Moving), 8),
                LoadAndSaveEntry.Create(reader => IgnoreBoxes = reader.ReadByte() != 0, writer => writer.WriteByte(IgnoreBoxes), 8),
                LoadAndSaveEntry.Create(reader => ForceClip = reader.ReadByte(), writer => writer.WriteByte(ForceClip), 8),
                LoadAndSaveEntry.Create(reader => InitFrame = reader.ReadByte(), writer => writer.WriteByte(InitFrame), 8),
                LoadAndSaveEntry.Create(reader => WalkFrame = reader.ReadByte(), writer => writer.WriteByte(WalkFrame), 8),
                LoadAndSaveEntry.Create(reader => StandFrame = reader.ReadByte(), writer => writer.WriteByte(StandFrame), 8),
                LoadAndSaveEntry.Create(reader => TalkStartFrame = reader.ReadByte(), writer => writer.WriteByte(TalkStartFrame), 8),
                LoadAndSaveEntry.Create(reader => TalkStopFrame = reader.ReadByte(), writer => writer.WriteByte(TalkStopFrame), 8),
                LoadAndSaveEntry.Create(reader => _speedx = reader.ReadUInt16(), writer => writer.WriteUInt16(_speedx), 8),
                LoadAndSaveEntry.Create(reader => _speedy = reader.ReadUInt16(), writer => writer.WriteUInt16(_speedy), 8),
                LoadAndSaveEntry.Create(reader => Cost.AnimCounter = reader.ReadUInt16(), writer => writer.WriteUInt16(Cost.AnimCounter), 8),
                LoadAndSaveEntry.Create(reader => Cost.SoundCounter = reader.ReadByte(), writer => writer.WriteByte(Cost.SoundCounter), 8),
                LoadAndSaveEntry.Create(reader => reader.ReadByte(), writer => writer.WriteByte(0), 32),
                LoadAndSaveEntry.Create(reader => _flip = reader.ReadByte() != 0, writer => writer.WriteByte(_flip), 32),
                LoadAndSaveEntry.Create(reader => reader.ReadByte(), writer => writer.WriteByte(0xCD), 32),

                // Actor palette grew from 64 to 256 bytes and switched to uint16 in HE games
                LoadAndSaveEntry.Create(
                    reader => _palette = reader.ReadBytes(64).Cast<ushort>().ToArray(),
                    writer => writer.WriteBytes(_palette, 64),
                    8, 9),
                LoadAndSaveEntry.Create(
                    reader => _palette = reader.ReadBytes(256).Cast<ushort>().ToArray(),
                    writer => writer.WriteBytes(_palette, 256),
                    10, 79),
                LoadAndSaveEntry.Create(
                    reader => _palette = reader.ReadUInt16s(256),
                    writer => writer.WriteUInt16s(_palette, 256)
                        , 80),

                LoadAndSaveEntry.Create(reader => reader.ReadByte(), writer => writer.WriteByte(0), 8, 9),
                LoadAndSaveEntry.Create(reader => ShadowMode = reader.ReadByte(), writer => writer.WriteByte(ShadowMode), 8),
                LoadAndSaveEntry.Create(reader => IsVisible = reader.ReadByte() != 0, writer => writer.WriteByte(IsVisible), 8),
                LoadAndSaveEntry.Create(reader => _frame = reader.ReadByte(), writer => writer.WriteByte(_frame), 8),
                LoadAndSaveEntry.Create(reader => _animSpeed = reader.ReadByte(), writer => writer.WriteByte(_animSpeed), 8),
                LoadAndSaveEntry.Create(reader => _animProgress = reader.ReadByte(), writer => writer.WriteByte(_animProgress), 8),
                LoadAndSaveEntry.Create(reader => Walkbox = reader.ReadByte(), writer => writer.WriteByte(Walkbox), 8),
                LoadAndSaveEntry.Create(reader => NeedRedraw = reader.ReadByte() != 0, writer => writer.WriteByte(NeedRedraw), 8),
                LoadAndSaveEntry.Create(reader => NeedBackgroundReset = reader.ReadByte() != 0, writer => writer.WriteByte(NeedBackgroundReset), 8),
                LoadAndSaveEntry.Create(reader => _costumeNeedsInit = reader.ReadByte() != 0, writer => writer.WriteByte(_costumeNeedsInit), 8),
                LoadAndSaveEntry.Create(reader => reader.ReadUInt32(), writer => writer.WriteUInt32(0xCDCDCDCD), 38),
                LoadAndSaveEntry.Create(reader => reader.ReadUInt32(), writer => writer.WriteUInt32(0xCDCDCDCD), 59),
                LoadAndSaveEntry.Create(reader => reader.ReadUInt32(), writer => writer.WriteUInt32(0xCDCDCDCD), 59),

                LoadAndSaveEntry.Create(reader =>
                    {
                        _talkPos = new Point(reader.ReadInt16(), reader.ReadInt16());
                    }, writer =>
                    {
                        writer.WriteInt16(_talkPos.X);
                        writer.WriteInt16(_talkPos.Y);
                    }, 8),
                LoadAndSaveEntry.Create(reader => _ignoreTurns = reader.ReadByte() != 0, writer => writer.WriteByte(_ignoreTurns), 8),

                // Actor layer switched to int32 in HE games
                LoadAndSaveEntry.Create(reader => reader.ReadByte(), writer => writer.WriteByte(0), 8, 57),
                LoadAndSaveEntry.Create(reader => reader.ReadInt32(), writer => writer.WriteInt32(0), 58),
                                             
                LoadAndSaveEntry.Create(reader => _talkScript = reader.ReadUInt16(), writer => writer.WriteUInt16(_talkScript), 8),
                LoadAndSaveEntry.Create(reader => _walkScript = reader.ReadUInt16(), writer => writer.WriteUInt16(_walkScript), 8),

                LoadAndSaveEntry.Create(reader => _walkdata.Dest.X = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Dest.X), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.Dest.Y = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Dest.Y), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.DestBox = reader.ReadByte(), writer => writer.WriteByte(_walkdata.DestBox), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.DestDir = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.DestDir), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.CurBox = reader.ReadByte(), writer => writer.WriteByte(_walkdata.CurBox), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.Cur.X = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Cur.X), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.Cur.Y = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Cur.Y), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.Next.X = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Next.X), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.Next.Y = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Next.Y), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.DeltaXFactor = reader.ReadInt32(), writer => writer.WriteInt32(_walkdata.DeltaXFactor), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.DeltaYFactor = reader.ReadInt32(), writer => writer.WriteInt32(_walkdata.DeltaYFactor), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.XFrac = reader.ReadUInt16(), writer => writer.WriteUInt16(_walkdata.XFrac), 8),
                LoadAndSaveEntry.Create(reader => _walkdata.YFrac = reader.ReadUInt16(), writer => writer.WriteUInt16(_walkdata.YFrac), 8),

                LoadAndSaveEntry.Create(reader => _walkdata.Point3.X = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Point3.X), 42),
                LoadAndSaveEntry.Create(reader => _walkdata.Point3.Y = reader.ReadInt16(), writer => writer.WriteInt16(_walkdata.Point3.Y), 42),

                LoadAndSaveEntry.Create(reader => Cost.Active = reader.ReadBytes(16), writer => writer.WriteBytes(Cost.Active, 16), 8),
                LoadAndSaveEntry.Create(reader => Cost.Stopped = reader.ReadUInt16(), writer => writer.WriteUInt16(Cost.Stopped), 8),
                LoadAndSaveEntry.Create(reader => Cost.Curpos = reader.ReadUInt16s(16), writer => writer.WriteUInt16s(Cost.Curpos, 16), 8),
                LoadAndSaveEntry.Create(reader => Cost.Start = reader.ReadUInt16s(16), writer => writer.WriteUInt16s(Cost.Start, 16), 8),
                LoadAndSaveEntry.Create(reader => Cost.End = reader.ReadUInt16s(16), writer => writer.WriteUInt16s(Cost.End, 16), 8),
                LoadAndSaveEntry.Create(reader => Cost.Frame = reader.ReadUInt16s(16), writer => writer.WriteUInt16s(Cost.Frame, 16), 8),
                                             
                LoadAndSaveEntry.Create(reader => reader.ReadUInt16s(16), writer => writer.WriteUInt16s(new ushort[16], 16), 65),
                LoadAndSaveEntry.Create(reader => reader.ReadUInt16s(16), writer => writer.WriteUInt16s(new ushort[16], 16), 65),
                LoadAndSaveEntry.Create(reader => reader.ReadUInt32s(16), writer => writer.WriteUInt32s(new uint[16], 16), 65),
            };

            if (serializer.IsLoading)
            {
                // Not all actor data is saved; so when loading, we first reset
                // the actor, to ensure completely reproducible behavior (else,
                // some not saved value in the actor class can cause odd things)
                Init(-1);
            }

            Array.ForEach(actorEntries, e => e.Execute(serializer));
        }

        public void RunTalkScript(int frame)
        {
            if (_scumm.TalkingActor == 0 || Room != _scumm.CurrentRoom || _frame == frame)
                return;

            if (_talkScript != 0)
            {
                _scumm.RunScript((byte)_talkScript, true, false, new int[] { frame, Number });
            }
            else
            {
                StartAnimActor((byte)frame);
            }
        }

        public void StartAnimActor(byte frame)
        {
            switch (frame)
            {
                case 0x38:
                    frame = InitFrame;
                    break;
                case 0x39:
                    frame = WalkFrame;
                    break;
                case 0x3A:
                    frame = StandFrame;
                    break;
                case 0x3B:
                    frame = TalkStartFrame;
                    break;
                case 0x3C:
                    frame = TalkStopFrame;
                    break;
            }

            if (IsInCurrentRoom && Costume != 0)
            {
                _animProgress = 0;
                NeedRedraw = true;
                Cost.AnimCounter = 0;
                // V1 - V2 games don't seem to need a _cost.reset() at this point.
                // Causes Zak to lose his body in several scenes, see bug #771508
                if (frame == InitFrame)
                {
                    Cost.Reset();
                }
                _scumm.CostumeLoader.CostumeDecodeData(this, frame, uint.MaxValue);
                _frame = frame;
            }
        }

        public void StopActorMoving()
        {
            if (_walkScript != 0)
                _scumm.StopScript(_walkScript);

            Moving = MoveFlags.None;
        }

        #endregion

        #region Private Methods

        protected void TurnToDirection(int newdir)
        {
            if (newdir == -1 || _ignoreTurns)
                return;

            Moving = MoveFlags.Turn;
            _targetFacing = (ushort)newdir;
        }

        void PrepareDrawActorCostume(ICostumeRenderer bcr)
        {
            bcr.ActorID = Number;
            bcr.ActorX = _position.X - _scumm.MainVirtScreen.XStart;
            bcr.ActorY = _position.Y - _elevation;

            if (_scumm.Game.Version == 4 && (BoxScale & 0x8000) != 0)
            {
                bcr.ScaleX = bcr.ScaleY = (byte)_scumm.GetScaleFromSlot((BoxScale & 0x7fff) + 1, _position.X, _position.Y);
            }
            else
            {
                bcr.ScaleX = ScaleX;
                bcr.ScaleY = ScaleY;
            }

            bcr.ShadowMode = ShadowMode;
            if (_scumm.Game.Version >= 5)
            {
                bcr.ShadowTable = _scumm.ShadowPalette;
            }

            bcr.SetCostume(Costume, 0);
            bcr.SetPalette(_palette);
            bcr.SetFacing(this);


            if (ForceClip > 0)
                bcr.ZBuffer = ForceClip;
            else if (IsInClass(ObjectClass.NeverClip))
                bcr.ZBuffer = 0;
            else
            {
                bcr.ZBuffer = _scumm.GetBoxMask(Walkbox);
                if (bcr.ZBuffer > _scumm.Gdi.NumZBuffer - 1)
                    bcr.ZBuffer = (byte)(_scumm.Gdi.NumZBuffer - 1);
            }

            bcr.DrawTop = 0x7fffffff;
            bcr.DrawBottom = 0;
        }

        bool IsInClass(ObjectClass cls)
        {
            return _scumm.GetClass(Number, cls);
        }

        void AdjustActorPos()
        {
            var abr = AdjustXYToBeInBox(_position);

            _position = abr.Position;
            _walkdata.DestBox = abr.Box;

            SetBox(abr.Box);

            _walkdata.Dest.X = -1;

            StopActorMoving();
            Cost.SoundCounter = 0;
            Cost.SoundPos = 0;

            if (Walkbox != InvalidBox)
            {
                int flags = (int)_scumm.GetBoxFlags(Walkbox);
                if ((flags & 7) != 0)
                {
                    TurnToDirection(_facing);
                }
            }
        }

        protected int CalcMovementFactor(Point next)
        {
            if (_position == next)
                return 0;

            int diffX = next.X - _position.X;
            int diffY = next.Y - _position.Y;
            int deltaYFactor = (int)_speedy << 16;

            if (diffY < 0)
                deltaYFactor = -deltaYFactor;

            int deltaXFactor = deltaYFactor * diffX;
            if (diffY != 0)
            {
                deltaXFactor /= diffY;
            }
            else
            {
                deltaYFactor = 0;
            } 

            if ((uint)Math.Abs(deltaXFactor) > (_speedx << 16))
            {
                deltaXFactor = (int)(_speedx << 16);
                if (diffX < 0)
                    deltaXFactor = -deltaXFactor;

                deltaYFactor = deltaXFactor * diffY;
                if (diffX != 0)
                {
                    deltaYFactor /= diffX;
                }
                else
                {
                    deltaXFactor = 0;
                }
            }

            _walkdata.Cur = _position;
            _walkdata.Next = next;
            _walkdata.DeltaXFactor = deltaXFactor;
            _walkdata.DeltaYFactor = deltaYFactor;
            _walkdata.XFrac = 0;
            _walkdata.YFrac = 0;

            _targetFacing = (ushort)ScummMath.GetAngleFromPos(deltaXFactor, deltaYFactor, false);

            return ActorWalkStep();
        }

        protected int ActorWalkStep()
        {
            NeedRedraw = true;

            int nextFacing = UpdateActorDirection(true);
            if (!Moving.HasFlag(MoveFlags.InLeg) || _facing != nextFacing)
            {
                if (WalkFrame != _frame || _facing != nextFacing)
                {
                    StartWalkAnim(nextFacing);
                }
                Moving |= MoveFlags.InLeg;
            }

            if (Walkbox != _walkdata.CurBox &&
                _scumm.CheckXYInBoxBounds(_walkdata.CurBox, _position))
            {
                SetBox(_walkdata.CurBox);
            }

            int distX = Math.Abs(_walkdata.Next.X - _walkdata.Cur.X);
            int distY = Math.Abs(_walkdata.Next.Y - _walkdata.Cur.Y);

            if (Math.Abs(_position.X - _walkdata.Cur.X) >= distX && Math.Abs(_position.Y - _walkdata.Cur.Y) >= distY)
            {
                Moving &= ~MoveFlags.InLeg;
                return 0;
            }

            int tmpX = (_position.X << 16) + _walkdata.XFrac + (_walkdata.DeltaXFactor >> 8) * ScaleX;
            _walkdata.XFrac = (ushort)tmpX;
            _position.X = (short)(tmpX >> 16);

            int tmpY = (_position.Y << 16) + _walkdata.YFrac + (_walkdata.DeltaYFactor >> 8) * ScaleY;
            _walkdata.YFrac = (ushort)tmpY;
            _position.Y = (short)(tmpY >> 16);

            if (Math.Abs(_position.X - _walkdata.Cur.X) > distX)
            {
                _position.X = _walkdata.Next.X;
            }

            if (Math.Abs(_position.Y - _walkdata.Cur.Y) > distY)
            {
                _position.Y = _walkdata.Next.Y;
            }

            if (_scumm.Game.Version >= 4 && _position == _walkdata.Next)
            {
                Moving &= ~MoveFlags.InLeg;
                return 0;
            }
            return 1;
        }

        int RemapDirection(int dir, bool isWalking)
        {
            BoxFlags flags;
            bool flipX;
            bool flipY;

            // FIXME: It seems that at least in The Dig the original code does
            // check _ignoreBoxes here. However, it breaks some animations in Loom,
            // causing Bobbin to face towards the camera instead of away from it
            // in some places: After the tree has been destroyed by lightning, and
            // when entering the dark tunnels beyond the dragon's lair at the very
            // least. Possibly other places as well.
            //
            // The Dig also checks if the actor is in the current room, but that's
            // not necessary here because we never call the function unless the
            // actor is in the current room anyway.

            if (!IgnoreBoxes)
            {
                flags = _scumm.GetBoxFlags(Walkbox);

                flipX = (_walkdata.DeltaXFactor > 0);
                flipY = (_walkdata.DeltaYFactor > 0);

                // Check for X-Flip
                if (flags.HasFlag(BoxFlags.XFlip) || IsInClass(ObjectClass.XFlip))
                {
                    dir = 360 - dir;
                    flipX = !flipX;
                }
                // Check for Y-Flip
                if (flags.HasFlag(BoxFlags.YFlip) || IsInClass(ObjectClass.YFlip))
                {
                    dir = 180 - dir;
                    flipY = !flipY;
                }

                switch ((byte)flags & 7)
                {
                    case 1:
                        {
                            if (isWalking)	                       // Actor is walking
                                return flipX ? 90 : 270;	                               // Actor is standing/turning
                            return (dir == 90) ? 90 : 270;
                        }
                    case 2:
                        {
                            if (isWalking)	                       // Actor is walking
                                return flipY ? 180 : 0;	                               // Actor is standing/turning
                            return (dir == 0) ? 0 : 180;
                        }
                    case 3:
                        return 270;
                    case 4:
                        return 90;
                    case 5:
                        return 0;
                    case 6:
                        return 180;
                }
            }
            // OR 1024 in to signal direction interpolation should be done
            return ScummMath.NormalizeAngle(dir) | 1024;
        }

        protected virtual void SetupActorScale()
        {
            if (IgnoreBoxes)
                return;

            BoxScale = (ushort)_scumm.GetBoxScale(Walkbox);

            var scale = _scumm.GetScale(Walkbox, _position.X, _position.Y);

            ScaleX = ScaleY = (byte)scale;
        }

        protected void SetBox(byte box)
        {
            Walkbox = box;
            SetupActorScale();
        }

        protected int UpdateActorDirection(bool isWalking)
        {
            int from = ScummMath.ToSimpleDir(false, _facing);
            int dir = RemapDirection(_targetFacing, isWalking);

            bool shouldInterpolate = (dir & 1024) != 0;
            dir &= 1023;

            if (shouldInterpolate)
            {
                int to = ScummMath.ToSimpleDir(false, dir);
                int num = 4;

                // Turn left or right, depending on which is shorter.
                int diff = to - from;
                if (Math.Abs(diff) > (num >> 1))
                    diff = -diff;

                if (diff > 0)
                {
                    to = from + 1;
                }
                else if (diff < 0)
                {
                    to = from - 1;
                }

                dir = ScummMath.FromSimpleDirection((to + num) % num);
            }

            return dir;
        }

        void StartWalkAnim(int angle)
        {
            if (angle == -1)
                angle = _facing;

            /* Note: walk scripts aren't required to make the Dig
             * work as usual
             */
            if (_walkScript != 0)
            {
                var args = new int[] { Number, 1, angle };
                _scumm.RunScript((byte)_walkScript, true, false, args);
            }
            else
            {
                SetDirection(angle);
                StartAnimActor(WalkFrame);
            }
        }

        bool FindPathTowards(byte box1nr, byte box2nr, byte box3nr, out Point foundPath)
        {
            foundPath = new Point();
            var box1 = _scumm.GetBoxCoordinates(box1nr);
            var box2 = _scumm.GetBoxCoordinates(box2nr);
            Point tmp;
            int flag;
            int q, pos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (box1.UpperLeft.X == box1.UpperRight.X && box1.UpperLeft.X == box2.UpperLeft.X && box1.UpperLeft.X == box2.UpperRight.X)
                    {
                        flag = 0;
                        if (box1.UpperLeft.Y > box1.UpperRight.Y)
                        {
                            ScummHelper.Swap(ref box1.UpperLeft.Y, ref box1.UpperRight.Y);
                            flag |= 1;
                        }

                        if (box2.UpperLeft.Y > box2.UpperRight.Y)
                        {
                            ScummHelper.Swap(ref box2.UpperLeft.Y, ref box2.UpperRight.Y);
                            flag |= 2;
                        }

                        if (box1.UpperLeft.Y > box2.UpperRight.Y || box2.UpperLeft.Y > box1.UpperRight.Y ||
                            ((box1.UpperRight.Y == box2.UpperLeft.Y || box2.UpperRight.Y == box1.UpperLeft.Y) &&
                            box1.UpperLeft.Y != box1.UpperRight.Y && box2.UpperLeft.Y != box2.UpperRight.Y))
                        {
                            if ((flag & 1) != 0)
                                ScummHelper.Swap(ref box1.UpperLeft.Y, ref box1.UpperRight.Y);
                            if ((flag & 2) != 0)
                                ScummHelper.Swap(ref box2.UpperLeft.Y, ref box2.UpperRight.Y);
                        }
                        else
                        {
                            pos = _position.Y;
                            if (box2nr == box3nr)
                            {
                                int diffX = _walkdata.Dest.X - _position.X;
                                int diffY = _walkdata.Dest.Y - _position.Y;
                                int boxDiffX = box1.UpperLeft.X - _position.X;

                                if (diffX != 0)
                                {
                                    diffY *= boxDiffX;
                                    int t = diffY / diffX;
                                    if (t == 0 && (diffY <= 0 || diffX <= 0)
                                        && (diffY >= 0 || diffX >= 0))
                                        t = -1;
                                    pos = _position.Y + t;
                                }
                            }

                            q = pos;
                            if (q < box2.UpperLeft.Y)
                                q = box2.UpperLeft.Y;
                            if (q > box2.UpperRight.Y)
                                q = box2.UpperRight.Y;
                            if (q < box1.UpperLeft.Y)
                                q = box1.UpperLeft.Y;
                            if (q > box1.UpperRight.Y)
                                q = box1.UpperRight.Y;
                            if (q == pos && box2nr == box3nr)
                                return true;
                            foundPath.Y = (short)q;
                            foundPath.X = box1.UpperLeft.X;
                            return false;
                        }
                    }

                    if (box1.UpperLeft.Y == box1.UpperRight.Y && box1.UpperLeft.Y == box2.UpperLeft.Y && box1.UpperLeft.Y == box2.UpperRight.Y)
                    {
                        flag = 0;
                        if (box1.UpperLeft.X > box1.UpperRight.X)
                        {
                            ScummHelper.Swap(ref box1.UpperLeft.X, ref box1.UpperRight.X);
                            flag |= 1;
                        }

                        if (box2.UpperLeft.X > box2.UpperRight.X)
                        {
                            ScummHelper.Swap(ref box2.UpperLeft.X, ref box2.UpperRight.X);
                            flag |= 2;
                        }

                        if (box1.UpperLeft.X > box2.UpperRight.X || box2.UpperLeft.X > box1.UpperRight.X ||
                            ((box1.UpperRight.X == box2.UpperLeft.X || box2.UpperRight.X == box1.UpperLeft.X) &&
                            box1.UpperLeft.X != box1.UpperRight.X && box2.UpperLeft.X != box2.UpperRight.X))
                        {
                            if ((flag & 1) != 0)
                                ScummHelper.Swap(ref box1.UpperLeft.X, ref box1.UpperRight.X);
                            if ((flag & 2) != 0)
                                ScummHelper.Swap(ref box2.UpperLeft.X, ref box2.UpperRight.X);
                        }
                        else
                        {

                            if (box2nr == box3nr)
                            {
                                int diffX = _walkdata.Dest.X - _position.X;
                                int diffY = _walkdata.Dest.Y - _position.Y;
                                int boxDiffY = box1.UpperLeft.Y - _position.Y;

                                pos = _position.X;
                                if (diffY != 0)
                                {
                                    pos += diffX * boxDiffY / diffY;
                                }
                            }
                            else
                            {
                                pos = _position.X;
                            }

                            q = pos;
                            if (q < box2.UpperLeft.X)
                                q = box2.UpperLeft.X;
                            if (q > box2.UpperRight.X)
                                q = box2.UpperRight.X;
                            if (q < box1.UpperLeft.X)
                                q = box1.UpperLeft.X;
                            if (q > box1.UpperRight.X)
                                q = box1.UpperRight.X;
                            if (q == pos && box2nr == box3nr)
                                return true;
                            foundPath.X = (short)q;
                            foundPath.Y = box1.UpperLeft.Y;
                            return false;
                        }
                    }
                    tmp = box1.UpperLeft;
                    box1.UpperLeft = box1.UpperRight;
                    box1.UpperRight = box1.LowerRight;
                    box1.LowerRight = box1.LowerLeft;
                    box1.LowerLeft = tmp;
                }
                tmp = box2.UpperLeft;
                box2.UpperLeft = box2.UpperRight;
                box2.UpperRight = box2.LowerRight;
                box2.LowerRight = box2.LowerLeft;
                box2.LowerLeft = tmp;
            }
            return false;
        }

        #endregion
    }
}
