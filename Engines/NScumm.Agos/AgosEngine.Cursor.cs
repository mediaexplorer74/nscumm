﻿//
//  AgosEngine.Cursor.cs
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

using NScumm.Core;
using NScumm.Core.Graphics;

namespace NScumm.Agos
{
    partial class AGOSEngine
    {
        protected virtual void DrawMousePointer()
        {
            if (GameType == SIMONGameType.GType_SIMON2)
            {
                OSystem.GraphicsManager.SetCursor(_simon2_cursors[_mouseCursor], 16, 16, new Point(7, 7), 0xFF);
            }
            else if (GameType != SIMONGameType.GType_SIMON1)
            {
                int i, j;
                Ptr<ushort> src;
                _mouseData.Data.Set(_mouseData.Offset, 0xFF, _maxCursorWidth * _maxCursorHeight);

                if (GameType == SIMONGameType.GType_WW)
                {
                    if (_mouseCursor == 0)
                    {
                        src = _common_mouseInfo;
                    }
                    else if (_mouseCursor == 2)
                    {
                        src = _common_handInfo;
                    }
                    else if (_mouseCursor == 3)
                    {
                        src = _common_swordInfo1;
                    }
                    else if (_mouseCursor == 4)
                    {
                        src = _common_moveForward;
                    }
                    else if (_mouseCursor == 5)
                    {
                        src = _common_moveRight;
                    }
                    else if (_mouseCursor == 6)
                    {
                        src = _common_moveBack;
                    }
                    else if (_mouseCursor == 7)
                    {
                        src = _common_moveLeft;
                    }
                    else
                    {
                        src = _common_mouseInfo;
                    }
                }
                else if (GameType == SIMONGameType.GType_ELVIRA2)
                {
                    if (_mouseCursor == 0)
                    {
                        src = _common_mouseInfo;
                    }
                    else if (_mouseCursor == 1)
                    {
                        src = _common_shieldInfo1;
                    }
                    else if (_mouseCursor == 2)
                    {
                        src = _common_shieldInfo2;
                    }
                    else if (_mouseCursor == 3)
                    {
                        src = _common_swordInfo1;
                    }
                    else if (_mouseCursor == 4)
                    {
                        src = _common_swordInfo2;
                    }
                    else if (_mouseCursor == 5)
                    {
                        src = _common_lightningInfo1;
                    }
                    else if (_mouseCursor == 6)
                    {
                        src = _common_lightningInfo2;
                    }
                    else if (_mouseCursor == 7)
                    {
                        src = _common_crossbowInfo1;
                    }
                    else
                    {
                        src = _common_crossbowInfo2;
                    }
                }
                else
                {
                    if (_mouseCursor == 0)
                    {
                        src = _common_mouseInfo;
                    }
                    else if (_mouseCursor == 1)
                    {
                        src = _common_shieldInfo1;
                    }
                    else
                    {
                        src = _common_swordInfo1;
                    }
                }

                if (_dragFlag)
                    src = _common_handInfo;

                for (i = 0; i < 16; i++)
                {
                    for (j = 0; j < 16; j++)
                    {
                        if ((src[0] & (1 << (15 - (j % 16)))) != 0)
                        {
                            if ((src[1] & (1 << (15 - (j % 16)))) != 0)
                            {
                                _mouseData[16 * i + j] = 1;
                            }
                            else
                            {
                                _mouseData[16 * i + j] = 0;
                            }
                        }
                    }
                    src.Offset += 2;
                }

                OSystem.GraphicsManager.SetCursor(_mouseData, 16, 16, new Point(), 0xFF);
            }
        }

        protected virtual void InitMouse()
        {
            InitMouseCore();
        }

        protected void InitMouseCore()
        {
            _maxCursorWidth = 16;
            _maxCursorHeight = 16;
            _mouseData = new byte[_maxCursorWidth * _maxCursorHeight];

            _mouseData.Data.Set(_mouseData.Offset, 0xFF, _maxCursorWidth * _maxCursorHeight);

// CursorMan.replaceCursorPalette(mouseCursorPalette, 0, ARRAYSIZE(mouseCursorPalette) / 3);
        }

        private void MouseOn()
        {
            _videoLockOut |= 1;

            if (_mouseHideCount != 0)
                _mouseHideCount--;

            _videoLockOut = (ushort) (_videoLockOut & ~1);
        }

        protected virtual void HandleMouseMoved()
        {
            if (_mouseHideCount != 0)
            {
                OSystem.GraphicsManager.IsCursorVisible = false;
                return;
            }

            OSystem.GraphicsManager.IsCursorVisible = true;
            _mouse = OSystem.InputManager.GetMousePosition();

            if (GameType == SIMONGameType.GType_WW)
            {
                if (_variableArray[51] != 0 && _mouseCursor != _variableArray[51])
                {
                    _mouseCursor = (byte) _variableArray[51];
                    _needHitAreaRecalc++;
                }
            }
            else if (GameType == SIMONGameType.GType_ELVIRA2)
            {
                if (_mouseCursor != _variableArray[72])
                {
                    _mouseCursor = (byte) _variableArray[72];
                    _needHitAreaRecalc++;
                }
            }
            else if (GameType == SIMONGameType.GType_ELVIRA1)
            {
                if (_mouseCursor != _variableArray[438])
                {
                    _mouseCursor = (byte) _variableArray[438];
                    _needHitAreaRecalc++;
                }
            }

            if (_leftClick)
            {
                _leftClick = false;
                if (_dragMode)
                {
                    _dragEnd = true;
                }
                else
                {
                    _oneClick = 1;
                }
                _dragCount = 0;
            }

            if (_mouse != _mouseOld)
                _needHitAreaRecalc++;

            if (_leftButtonOld == 0 && _leftButton != 0)
            {
                _lastClickRem = null;
                BoxController((uint) _mouse.X, (uint) _mouse.Y, 3);
            }
            _leftButtonOld = _leftButton;

            uint x;
            if (_dragMode || _lastHitArea3 != null)
            {
                x = 0;
                if (_needHitAreaRecalc == 0)
                    goto get_out;
                else
                    goto boxstuff;
            }

            if (_leftButton != 0 && _dragAccept && _lastClickRem != null)
            {
                _dragCount++;
                if (_dragCount == 20)
                {
                    _dragMode = true;
                    _dragFlag = true;
                    _needHitAreaRecalc++;
                }
            }

            x = 0;
            if (_oneClick > 0)
            {
                _oneClick = 0;
                x = 1;
            }
            else
            {
                if (!_litBoxFlag && _needHitAreaRecalc == 0)
                    goto get_out;
            }

            boxstuff:
            BoxController((uint) _mouse.X, (uint) _mouse.Y, x);
            _lastHitArea3 = _lastHitArea;
            get_out:
            _mouseOld = _mouse;
            DrawMousePointer();

            _needHitAreaRecalc = 0;
            _litBoxFlag = false;
        }

        protected static readonly ushort[] _common_mouseInfo =
        {
            0xC000, 0x8000, 0xE000, 0xC000, 0xF000, 0xE000, 0xF800, 0xF000,
            0xFC00, 0xF800, 0xFE00, 0xFC00, 0xFC00, 0xF000, 0x9800, 0x9000,
            0x0C00, 0x0800, 0x0C00, 0x0800, 0x0600, 0x0400, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
        };

        static readonly ushort[] _common_handInfo =
        {
            0x01C0, 0x01C0, 0x07F0, 0x0770, 0x07F0, 0x0550, 0x07FC, 0x055C,
            0x07FC, 0x0554, 0x3FFC, 0x3D54, 0x3FFC, 0x2554, 0x3FFE, 0x2402,
            0x1FFC, 0x1204, 0x1FFC, 0x1004, 0x0FF8, 0x0808, 0x07F8, 0x0408,
            0x03F8, 0x03F8, 0x03F8, 0x0208, 0x03F8, 0x03F8, 0x0000, 0x0000
        };

        static readonly ushort[] _common_shieldInfo1 =
        {
            0x0000, 0x0000, 0x2184, 0x2004, 0x33CC, 0x300C, 0x3FFC, 0x3E7C,
            0x3FFC, 0x3E7C, 0x3FFC, 0x3E7C, 0x3FFC, 0x0000, 0x3FFC, 0x3E7C,
            0x3FFC, 0x3E7C, 0x1FF8, 0x1E78, 0x1FF8, 0x1E78, 0x0FF0, 0x0E70,
            0x07E0, 0x0660, 0x03C0, 0x0240, 0x0180, 0x0000, 0x0000, 0x0000
        };

        static readonly ushort[] _common_shieldInfo2 =
        {
            0x0000, 0x0000, 0x2184, 0x2004, 0x33CC, 0x300C, 0x3FFC, 0x3E7C,
            0x3FFC, 0x2004, 0x3FFC, 0x2004, 0x3FFC, 0x0000, 0x3FFC, 0x2000,
            0x3FFC, 0x2004, 0x1FF8, 0x1008, 0x1FF8, 0x1008, 0x0FF0, 0x0810,
            0x07E0, 0x0420, 0x03C0, 0x0240, 0x0180, 0x0000, 0x0000, 0x0000,
        };

        static readonly ushort[] _common_swordInfo1 =
        {
            0x0000, 0x0000, 0xC000, 0x4000, 0xF000, 0x7000, 0x7800, 0x3800,
            0x3C00, 0x1C00, 0x1E00, 0x0E00, 0x0F00, 0x0700, 0x0780, 0x0380,
            0x03C6, 0x01C0, 0x01FF, 0x00E6, 0x00FE, 0x007C, 0x007C, 0x0038,
            0x00FC, 0x0078, 0x01FE, 0x00CC, 0x01CF, 0x0086, 0x0086, 0x0000
        };

        static readonly ushort[] _common_swordInfo2 =
        {
            0x0000, 0x0000, 0x4000, 0x0000, 0x7000, 0x0000, 0x3800, 0x0000,
            0x1C00, 0x0000, 0x0E00, 0x0000, 0x0700, 0x0000, 0x0380, 0x0000,
            0x01C0, 0x0000, 0x00E6, 0x0000, 0x007C, 0x0000, 0x0038, 0x0000,
            0x0078, 0x0000, 0x00CC, 0x0000, 0x0086, 0x0000, 0x0000, 0x0000,
        };

        static readonly ushort[] _common_lightningInfo1 =
        {
            0x0000, 0x0000, 0xC000, 0x4000, 0xE000, 0x6000, 0xF020, 0x7000,
            0x7860, 0x3820, 0x7CE0, 0x3C60, 0x3FF0, 0x1EF0, 0x3FF0, 0x1FF0,
            0x1FF8, 0x0FF8, 0x1FF8, 0x0FF8, 0x0F7C, 0x073C, 0x0E3C, 0x061C,
            0x061E, 0x020E, 0x040E, 0x0006, 0x0006, 0x0002, 0x0002, 0x0000,
        };

        static readonly ushort[] _common_lightningInfo2 =
        {
            0x0000, 0x0000, 0xC000, 0x0000, 0xE000, 0x0000, 0xF020, 0x0000,
            0x7860, 0x0000, 0x7CE0, 0x0000, 0x3FF0, 0x0000, 0x3FF0, 0x0000,
            0x1FF8, 0x0000, 0x1FF8, 0x0000, 0x0F7C, 0x0000, 0x0E3C, 0x0000,
            0x061E, 0x0000, 0x040E, 0x0000, 0x0006, 0x0000, 0x0002, 0x0000,
        };

        static readonly ushort[] _common_crossbowInfo1 =
        {
            0x0000, 0x0000, 0x03E0, 0x07C0, 0x1FF8, 0x1FF0, 0x3FFC, 0x3FF8,
            0x7FFE, 0x7FFC, 0x7FFE, 0x7FFC, 0xF39E, 0xF39E, 0xE38E, 0xE38E,
            0xC386, 0xC386, 0xC386, 0xC386, 0xC386, 0xC386, 0xC386, 0xC386,
            0x8002, 0x8002, 0x8002, 0x8002, 0x0000, 0x0000, 0x0000, 0x0000,
        };

        static readonly ushort[] _common_crossbowInfo2 =
        {
            0x0000, 0x0000, 0x03E0, 0x0000, 0x1FF8, 0x0000, 0x3FFC, 0x0000,
            0x7FFE, 0x0000, 0x7FFE, 0x0000, 0xF39E, 0x0000, 0xE38E, 0x0000,
            0xC386, 0x0000, 0xC386, 0x0000, 0xC386, 0x0000, 0xC386, 0x0000,
            0x8002, 0x0000, 0x8002, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        };

        static readonly ushort[] _common_moveForward =
        {
            0x0180, 0x0100, 0x03C0, 0x0380, 0x07E0, 0x07C0, 0x0FF0, 0x0FE0,
            0x1FF8, 0x1FF0, 0x03C0, 0x0380, 0x03C0, 0x0380, 0x03C0, 0x0380,
            0x03C0, 0x0380, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
        };

        static readonly ushort[] _common_moveRight =
        {
            0x00C0, 0x0080, 0x00E0, 0x00C0, 0x00F0, 0x00E0, 0x1FF8, 0x1FF0,
            0x3FFC, 0x3FF8, 0x3FF8, 0x3FF0, 0x3CF0, 0x38E0, 0x3CE0, 0x38C0,
            0x3CC0, 0x3880, 0x3C00, 0x3800, 0x3C00, 0x3800, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
        };

        static readonly ushort[] _common_moveBack =
        {
            0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
            0x03C0, 0x0380, 0x03C0, 0x0380, 0x03C0, 0x0380, 0x03C0, 0x0380,
            0x1FF8, 0x1FF0, 0x0FF0, 0x0FE0, 0x07E0, 0x07C0, 0x03C0, 0x0380,
            0x0180, 0x0100, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
        };

        static readonly ushort[] _common_moveLeft =
        {
            0x0C00, 0x0800, 0x1C00, 0x1800, 0x3C00, 0x3800, 0x7FE0, 0x7FC0,
            0xFFF0, 0xFFE0, 0x7FF0, 0x7FE0, 0x3CF0, 0x38E0, 0x1CF0, 0x18E0,
            0x0CF0, 0x08E0, 0x00F0, 0x00E0, 0x00F0, 0x00E0, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000
        };

        static readonly byte[][] _simon2_cursors =
        {
// cross hair
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xec, 0xec, 0xec, 0xec, 0xec, 0xef, 0xff, 0xea, 0xff, 0xef, 0xec, 0xec, 0xec, 0xec, 0xec, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// examine
            new byte[]
            {
                0xff, 0xff, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xef, 0xee, 0xeb, 0xe4, 0xe4, 0xe4, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xee, 0xeb, 0xee, 0xef, 0xef, 0xee, 0xec, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xeb, 0xee, 0xef, 0xee, 0xee, 0xef, 0xee, 0xe4, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xeb, 0xef, 0xef, 0xef, 0xec, 0xee, 0xef, 0xe4, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xeb, 0xef, 0xef, 0xee, 0xef, 0xef, 0xef, 0xe4, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xeb, 0xee, 0xef, 0xef, 0xef, 0xef, 0xee, 0xe4, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xef, 0xee, 0xeb, 0xee, 0xef, 0xef, 0xee, 0xe4, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xef, 0xee, 0xeb, 0xeb, 0xeb, 0xeb, 0xee, 0xe4, 0xec, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xeb, 0xe4, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xee, 0xe4, 0xeb, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xe4, 0xeb, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xec, 0xeb, 0xef, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xe4, 0xef, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// pick up
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xe6, 0xe7, 0xe7, 0xe6, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe7, 0xe7, 0xe7, 0xe8, 0xe8, 0xe8, 0xe8, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe6, 0xe7, 0xe7, 0xe7, 0xe7, 0xe7, 0xe7, 0xe8, 0xe9, 0xe7, 0xe5, 0xff, 0xff,
                0xff, 0xe5, 0xe6, 0xe7, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xe8, 0xe6, 0xe5, 0xff,
                0xff, 0xe5, 0xe7, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe8, 0xe7, 0xe5, 0xff,
                0xff, 0xe5, 0xe7, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe7, 0xe5, 0xff,
                0xff, 0xef, 0xeb, 0xeb, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xeb, 0xef, 0xff,
                0xff, 0xef, 0xee, 0xeb, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff, 0xef, 0xee, 0xeb, 0xee, 0xef, 0xff,
                0xff, 0xff, 0xef, 0xeb, 0xeb, 0xef, 0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xeb, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xef, 0xee, 0xe4, 0xee, 0xef, 0xff, 0xff, 0xef, 0xee, 0xe4, 0xee, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xe4, 0xeb, 0xef, 0xff, 0xff, 0xef, 0xeb, 0xe4, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xeb, 0xeb, 0xeb, 0xef, 0xef, 0xeb, 0xeb, 0xeb, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xef, 0xee, 0xee, 0xee, 0xee, 0xe1, 0xe1, 0xef, 0xff, 0xff, 0xff, 0xe4,
                0xef, 0xee, 0xeb, 0xeb, 0xeb, 0xeb, 0xeb, 0xe4, 0xe4, 0xe4, 0xe4, 0xe4, 0xe4, 0xe4, 0xeb, 0xec,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe4
            },
// give
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe9, 0xe7, 0xe8, 0xe8, 0xe8, 0xe7, 0xe9, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe7, 0xea, 0xe8, 0xe8, 0xe8, 0xea, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe7, 0xe8, 0xe8, 0xea, 0xe9, 0xea, 0xe8, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xe5, 0xe7, 0xe9, 0xe8, 0xe8, 0xe9, 0xec, 0xe9, 0xe8, 0xe8, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff,
                0xe5, 0xe7, 0xe7, 0xe9, 0xe8, 0xec, 0xe9, 0xec, 0xe8, 0xe9, 0xe7, 0xe6, 0xe5, 0xff, 0xff, 0xff,
                0xe5, 0xe7, 0xe7, 0xe8, 0xec, 0xe9, 0xe9, 0xe9, 0xec, 0xe7, 0xe6, 0xe6, 0xe5, 0xff, 0xff, 0xff,
                0xe5, 0xe7, 0xe7, 0xea, 0xe8, 0xe9, 0xe9, 0xe9, 0xe7, 0xec, 0xec, 0xe4, 0xe5, 0xff, 0xff, 0xff,
                0xe5, 0xe7, 0xe7, 0xe9, 0xe7, 0xe8, 0xe9, 0xe7, 0xe6, 0xec, 0xe4, 0xec, 0xe4, 0xef, 0xff, 0xff,
                0xe5, 0xe6, 0xe7, 0xe9, 0xe7, 0xe7, 0xe8, 0xe6, 0xe6, 0xe4, 0xec, 0xe4, 0xec, 0xe4, 0xef, 0xff,
                0xff, 0xe5, 0xe6, 0xe9, 0xe7, 0xe7, 0xe8, 0xe6, 0xe6, 0xe8, 0xe4, 0xec, 0xe4, 0xec, 0xeb, 0xff,
                0xff, 0xff, 0xe5, 0xe9, 0xe7, 0xe7, 0xe8, 0xe6, 0xe6, 0xe8, 0xe6, 0xe4, 0xec, 0xeb, 0xef, 0xff,
                0xff, 0xff, 0xff, 0xe8, 0xe7, 0xe7, 0xe8, 0xe6, 0xe6, 0xe7, 0xff, 0xef, 0xeb, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe8, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// talk
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe7, 0xe8, 0xe8, 0xe8, 0xe7, 0xe6, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe6, 0xe9, 0xea, 0xe6, 0xea, 0xe9, 0xe8, 0xe9, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe7, 0xe5, 0xef, 0xe5, 0xec, 0xea, 0xe5, 0xea, 0xec, 0xe5, 0xe9, 0xe6, 0xff, 0xff,
                0xff, 0xe5, 0xe6, 0xe5, 0xef, 0xef, 0xef, 0xe5, 0xef, 0xef, 0xe5, 0xef, 0xef, 0xe8, 0xe5, 0xff,
                0xff, 0xe5, 0xe9, 0xea, 0xe5, 0xe8, 0xe7, 0xe6, 0xe6, 0xe8, 0xe7, 0xe5, 0xec, 0xe9, 0xe5, 0xff,
                0xff, 0xe5, 0xe9, 0xe8, 0xe5, 0xe7, 0xe8, 0xe8, 0xe9, 0xe9, 0xe8, 0xe5, 0xe9, 0xe9, 0xe5, 0xff,
                0xff, 0xe5, 0xe6, 0xec, 0xea, 0xe5, 0xe6, 0xe6, 0xe7, 0xe7, 0xe6, 0xe5, 0xec, 0xe8, 0xe5, 0xff,
                0xff, 0xff, 0xe5, 0xe9, 0xe8, 0xe9, 0xe5, 0xe8, 0xe5, 0xe8, 0xe5, 0xe9, 0xe9, 0xe7, 0xe5, 0xff,
                0xff, 0xff, 0xe5, 0xe7, 0xe9, 0xec, 0xe8, 0xec, 0xe8, 0xec, 0xe8, 0xec, 0xe8, 0xe5, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe6, 0xe8, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe8, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// use
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xee, 0xe1, 0xeb, 0xee, 0xef, 0xef, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xef, 0xef, 0xe4, 0xeb, 0xee, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xeb, 0xe4, 0xe4, 0xeb, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xeb, 0xe4, 0xec, 0xe4, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xeb, 0xeb, 0xe4, 0xe4, 0xee, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xee, 0xeb, 0xeb, 0xeb, 0xe1, 0xef, 0xee, 0xef,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xef, 0xef, 0xee, 0xeb, 0xeb, 0xe4, 0xee,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xec, 0xeb, 0xef,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xee, 0xe4, 0xeb, 0xef, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xef, 0xee, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe6, 0xe8, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe6, 0xe8, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xe5, 0xe6, 0xe8, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xe5, 0xe6, 0xe6, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// wear
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xeb, 0xed, 0xe4, 0xe2, 0xeb, 0xee, 0xee, 0xee, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xe2, 0xec, 0xe2, 0xe1, 0xee, 0xef, 0xef, 0xee, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xeb, 0xed, 0xeb, 0xee, 0xef, 0xef, 0xef, 0xee, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xee, 0xe4, 0xeb, 0xee, 0xef, 0xef, 0xee, 0xef, 0xef, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xef, 0xe4, 0xeb, 0xee, 0xef, 0xef, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xef, 0xe2, 0xeb, 0xee, 0xef, 0xef, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xe1, 0xee, 0xef, 0xef, 0xee, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xef, 0xeb, 0xe1, 0xee, 0xef, 0xef, 0xef, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xef, 0xef, 0xef, 0xe1, 0xe4, 0xe4, 0xe4, 0xe1, 0xeb, 0xee, 0xef, 0xef, 0xef, 0xff, 0xff,
                0xef, 0xee, 0xee, 0xef, 0xee, 0xee, 0xee, 0xee, 0xee, 0xef, 0xef, 0xef, 0xee, 0xee, 0xef, 0xff,
                0xff, 0xef, 0xef, 0xee, 0xe1, 0xe2, 0xe4, 0xe4, 0xe4, 0xeb, 0xe1, 0xee, 0xef, 0xef, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xef, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// move
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xff,
                0xff, 0xe1, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xe3, 0xe3, 0xed, 0xe3, 0xe3, 0xe3, 0xe3, 0xed, 0xe3, 0xe3, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xe3, 0xed, 0xec, 0xe3, 0xe3, 0xe3, 0xe3, 0xec, 0xed, 0xe3, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xed, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xed, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xed, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xed, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xed, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xec, 0xed, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xe3, 0xed, 0xec, 0xe3, 0xe3, 0xe3, 0xe3, 0xec, 0xed, 0xe3, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xe3, 0xe3, 0xe3, 0xed, 0xe3, 0xe3, 0xe3, 0xe3, 0xed, 0xe3, 0xe3, 0xe3, 0xe1, 0xff,
                0xff, 0xe1, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe2, 0xe1, 0xff,
                0xff, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xe1, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
// open
            new byte[]
            {
                0xff, 0xff, 0xe5, 0xe8, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe8, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xe5, 0xe7, 0xe5, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xff, 0xe5, 0xe7, 0xe6, 0xe9, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe6, 0xea, 0xe6, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe6, 0xea, 0xe6, 0xe7, 0xe5, 0xff, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe6, 0xea, 0xe6, 0xff, 0xe5, 0xe7, 0xe5, 0xe7, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe6, 0xea, 0xe6, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe8, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe6, 0xea, 0xe6, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe8, 0xe8, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xea, 0xea, 0xea, 0xea, 0xea, 0xea, 0xea, 0xea, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe7, 0xe7, 0xe7, 0xe7, 0xe7, 0xe7, 0xe7, 0xea, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe8, 0xe8, 0xe8, 0xe8, 0xe8, 0xe7, 0xe7, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe6, 0xe6, 0xe6, 0xe6, 0xe6, 0xe6, 0xe5, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe8, 0xe8, 0xe8, 0xe8, 0xe8, 0xe8, 0xe7, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xe5, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff
            },
// question mark
            new byte[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xea, 0xec, 0xec, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe7, 0xea, 0xec, 0xea, 0xe9, 0xea, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe9, 0xec, 0xe9, 0xe8, 0xe7, 0xe8, 0xea, 0xec, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xe5, 0xe8, 0xe9, 0xe8, 0xe5, 0xe5, 0xe8, 0xe9, 0xec, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xe5, 0xe8, 0xe9, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe9, 0xec, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe8, 0xec, 0xea, 0xe8, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe9, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe9, 0xea, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe7, 0xe9, 0xe7, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe8, 0xe9, 0xe8, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe9, 0xec, 0xe9, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe8, 0xe9, 0xe8, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xe5, 0xe5, 0xe5, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            },
        };

        // Feeble Files specific
        static readonly byte[] _mouseOffs =
        {
            6, 0, 15, 21, 16, 21, 14, 21, 15, 21, 16, 21, 16, 21, 16, 21, 15, 21, 15, 21, 15, 21, 14, 21, 12, 21, 12, 21,
            12, 21, 12, 21,
            6, 2, 10, 12, 9, 12, 8, 11, 7, 10, 6, 9, 4, 8, 3, 7, 1, 7, 0, 6, 3, 7, 4, 8, 6, 9, 7, 10, 8, 11, 9, 12,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 3, 0, 3, 0, 4, 1, 4, 1, 3, 2, 3, 2, 2, 1, 3, 0, 4, 0, 3, 0, 0,
            0, 0, 5, 16, 4, 19, 2, 21, 1, 21, 1, 21, 1, 21, 1, 18, 3, 9, 6, 2, 6, 0, 3, 6, 4, 12, 4, 13, 4, 13, 4, 14,
            0, 0, 6, 13, 5, 15, 4, 16, 3, 19, 2, 19, 2, 19, 2, 18, 1, 16, 4, 10, 7, 3, 7, 0, 4, 2, 4, 6, 0, 0, 0, 0,
            0, 0, 7, 0, 7, 1, 8, 1, 11, 1, 13, 1, 9, 1, 6, 1, 6, 0, 6, 0, 6, 0, 7, 0, 11, 0, 13, 0, 9, 0, 7, 0,
            0, 0, 7, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

            // SAM icons
            0, 0, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3,
            0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            0, 0, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 1, 1, 2, 2, 3, 3, 5, 5, 7, 6, 9, 8, 11, 10, 14, 13, 16, 16, 0, 0,
            0, 0, 4, 3, 5, 2, 4, 2, 4, 3, 5, 3, 5, 2, 4, 2, 4, 3, 5, 3, 5, 2, 4, 3, 4, 3, 5, 3, 5, 2, 4, 2,

// Asteroid Map icons
            0, 0, 3, 0, 4, 1, 5, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3, 0, 4, 1, 5, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 8, 0, 7, 0, 8, 0, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
// Other icons
            0, 0, 9, 9, 9, 10, 8, 11, 7, 11, 7, 11, 8, 11, 9, 10, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 10, 7, 10, 6, 10, 5, 10, 4, 10, 3, 10, 4, 10, 5, 10, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 7, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 7, 3, 4, 3, 2, 4, 0, 5, 0, 7, 0, 7, 0, 5, 2, 4, 4, 3, 7, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 12, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
// Vent icons
            0, 0, 8, 3, 7, 3, 6, 3, 5, 3, 4, 3, 3, 3, 2, 3, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 2, 3, 3, 3, 4, 3, 8, 3, 10, 3, 12, 3, 14, 3, 17, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 3, 14, 4, 12, 5, 10, 6, 9, 7, 8, 7, 7, 8, 6, 9, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 2, 3, 3, 2, 3, 4, 3, 4, 3, 5, 3, 4, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 7, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 9, 9, 9, 10, 8, 11, 7, 11, 7, 11, 8, 11, 9, 10, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 10, 7, 10, 6, 10, 5, 10, 4, 10, 3, 10, 4, 10, 5, 10, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        };
    }
}