﻿//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2015 
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


using NScumm.Sci.Graphics;
using System;

namespace NScumm.Sci.Engine
{
    partial class Kernel
    {
        private static Register kInitBresen(EngineState s, int argc, StackPtr? argv)
        {
            SegManager segMan = s._segMan;
            Register mover = argv.Value[0];
            Register client = SciEngine.ReadSelector(segMan, mover, SciEngine.Selector(o => o.client));
            short stepFactor = (argc >= 2) ? argv.Value[1].ToInt16() : (short)1;
            short mover_x = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.x));
            short mover_y = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.y));
            short client_xStep = (short)(SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.xStep)) * stepFactor);
            short client_yStep = (short)(SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.yStep)) * stepFactor);

            short client_step;
            if (client_xStep < client_yStep)
                client_step = (short)(client_yStep * 2);
            else
                client_step = (short)(client_xStep * 2);

            short deltaX = (short)(mover_x - SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.x)));
            short deltaY = (short)(mover_y - SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.y)));
            short mover_dx = 0;
            short mover_dy = 0;
            short mover_i1 = 0;
            short mover_i2 = 0;
            short mover_di = 0;
            short mover_incr = 0;
            short mover_xAxis = 0;

            while (true)
            {
                mover_dx = client_xStep;
                mover_dy = client_yStep;
                mover_incr = 1;

                if (Math.Abs(deltaX) >= Math.Abs(deltaY))
                {
                    mover_xAxis = 1;
                    if (deltaX < 0)
                        mover_dx = (short)-mover_dx;
                    mover_dy = deltaX != 0 ? (short)(mover_dx * deltaY / deltaX) : (short)0;
                    mover_i1 = (short)(((mover_dx * deltaY) - (mover_dy * deltaX)) * 2);
                    if (deltaY < 0)
                    {
                        mover_incr = -1;
                        mover_i1 = (short)-mover_i1;
                    }
                    mover_i2 = (short)(mover_i1 - (deltaX * 2));
                    mover_di = (short)(mover_i1 - deltaX);
                    if (deltaX < 0)
                    {
                        mover_i1 = (short)-mover_i1;
                        mover_i2 = (short)-mover_i2;
                        mover_di = (short)-mover_di;
                    }
                }
                else {
                    mover_xAxis = 0;
                    if (deltaY < 0)
                        mover_dy = (short)-mover_dy;
                    mover_dx = deltaY != 0 ? (short)(mover_dy * deltaX / deltaY) : (short)0;
                    mover_i1 = (short)(((mover_dy * deltaX) - (mover_dx * deltaY)) * 2);
                    if (deltaX < 0)
                    {
                        mover_incr = -1;
                        mover_i1 = (short)-mover_i1;
                    }
                    mover_i2 = (short)(mover_i1 - (deltaY * 2));
                    mover_di = (short)(mover_i1 - deltaY);
                    if (deltaY < 0)
                    {
                        mover_i1 = (short)-mover_i1;
                        mover_i2 = (short)-mover_i2;
                        mover_di = (short)-mover_di;
                    }
                    break;
                }
                if (client_xStep <= client_yStep)
                    break;
                if (client_xStep == 0)
                    break;
                if (client_yStep >= Math.Abs(mover_dy + mover_incr))
                    break;

                client_step--;
                if (client_step == 0)
                    throw new InvalidOperationException("kInitBresen failed");
                client_xStep--;
            }

            // set mover
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.dx), (ushort)mover_dx);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.dy), (ushort)mover_dy);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i1), (ushort)mover_i1);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i2), (ushort)mover_i2);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_di), (ushort)mover_di);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_incr), (ushort)mover_incr);
            SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_xAxis), (ushort)mover_xAxis);
            return s.r_acc;
        }

        private static Register kDoBresen(EngineState s, int argc, StackPtr? argv)
        {
            SegManager segMan = s._segMan;
            Register mover = argv.Value[0];
            Register client = SciEngine.ReadSelector(segMan, mover, SciEngine.Selector(o => o.client));
            bool completed = false;
            bool handleMoveCount = SciEngine.Instance.Features.HandleMoveCount;

            if (ResourceManager.GetSciVersion() >= SciVersion.V1_EGA_ONLY)
            {
                var client_signal = (ViewSignals)SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.signal));
                SciEngine.WriteSelectorValue(segMan, client, SciEngine.Selector(o => o.signal), (ushort)(client_signal & ~ViewSignals.HitObstacle));
            }

            short mover_moveCnt = 1;
            short client_moveSpeed = 0;
            if (handleMoveCount)
            {
                mover_moveCnt = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_movCnt));
                client_moveSpeed = (short)SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.moveSpeed));
                mover_moveCnt++;
            }

            if (client_moveSpeed < mover_moveCnt)
            {
                mover_moveCnt = 0;
                short client_x = (short)SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.x));
                short client_y = (short)SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.y));
                short mover_x = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.x));
                short mover_y = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.y));
                short mover_xAxis = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_xAxis));
                short mover_dx = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.dx));
                short mover_dy = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.dy));
                short mover_incr = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_incr));
                short mover_i1 = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i1));
                short mover_i2 = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i2));
                short mover_di = (short)SciEngine.ReadSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_di));
                short mover_org_i1 = mover_i1;
                short mover_org_i2 = mover_i2;
                short mover_org_di = mover_di;

                if ((ResourceManager.GetSciVersion() >= SciVersion.V1_EGA_ONLY))
                {
                    // save current position into mover
                    SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.xLast), (ushort)client_x);
                    SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.yLast), (ushort)client_y);
                }

                // Store backups of all client selector variables. We will restore them
                // in case of a collision.
                SciObject clientObject = segMan.GetObject(client);
                int clientVarNum = clientObject.VarCount;
                Register[] clientBackup = new Register[clientVarNum];
                for (var i = 0; i < clientVarNum; ++i)
                    clientBackup[i] = clientObject.GetVariable(i);

                if (mover_xAxis!=0)
                {
                    if (Math.Abs(mover_x - client_x) < Math.Abs(mover_dx))
                        completed = true;
                }
                else {
                    if (Math.Abs(mover_y - client_y) < Math.Abs(mover_dy))
                        completed = true;
                }
                if (completed)
                {
                    client_x = mover_x;
                    client_y = mover_y;
                }
                else {
                    client_x += mover_dx;
                    client_y += mover_dy;
                    if (mover_di < 0)
                    {
                        mover_di += mover_i1;
                    }
                    else {
                        mover_di += mover_i2;
                        if (mover_xAxis == 0)
                        {
                            client_x += mover_incr;
                        }
                        else {
                            client_y += mover_incr;
                        }
                    }
                }
                SciEngine.WriteSelectorValue(segMan, client, SciEngine.Selector(o => o.x), (ushort)client_x);
                SciEngine.WriteSelectorValue(segMan, client, SciEngine.Selector(o => o.y), (ushort)client_y);

                // Now call client::canBeHere/client::cantBehere to check for collisions
                bool collision = false;
                Register cantBeHere = Register.NULL_REG;

                if (SciEngine.Selector(o => o.cantBeHere) != -1)
                {
                    // adding this here for hoyle 3 to get happy. CantBeHere is a dummy in hoyle 3 and acc is != 0 so we would
                    //  get a collision otherwise
                    s.r_acc = Register.NULL_REG;
                    SciEngine.InvokeSelector(s, client, SciEngine.Selector(o => o.cantBeHere), argc, argv);
                    if (!s.r_acc.IsNull)
                        collision = true;
                    cantBeHere = s.r_acc;
                }
                else {
                    SciEngine.InvokeSelector(s, client, SciEngine.Selector(o => o.canBeHere), argc, argv);
                    if (s.r_acc.IsNull)
                        collision = true;
                }

                if (collision)
                {
                    // We restore the backup of the client variables
                    for (var i = 0; i < clientVarNum; ++i)
                        clientObject.SetVariableRef(i, clientBackup[i]);

                    mover_i1 = mover_org_i1;
                    mover_i2 = mover_org_i2;
                    mover_di = mover_org_di;

                    ViewSignals client_signal = (ViewSignals)SciEngine.ReadSelectorValue(segMan, client, SciEngine.Selector(o => o.signal));
                    SciEngine.WriteSelectorValue(segMan, client, SciEngine.Selector(o => o.signal), (ushort)(client_signal | ViewSignals.HitObstacle));
                }

                SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i1), (ushort)mover_i1);
                SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_i2), (ushort)mover_i2);
                SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_di), (ushort)mover_di);

                if (ResourceManager.GetSciVersion() >= SciVersion.V1_EGA_ONLY)
                {
                    // In sci1egaonly this block of code was outside of the main if,
                    // but client_x/client_y aren't set there, so it was an
                    // uninitialized read in SSCI. (This issue was fixed in sci1early.)
                    if (handleMoveCount)
                        SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_movCnt), (ushort)mover_moveCnt);
                    // We need to compare directly in here, complete may have happened during
                    //  the current move
                    if ((client_x == mover_x) && (client_y == mover_y))
                        SciEngine.InvokeSelector(s, mover, SciEngine.Selector(o => o.moveDone), argc, argv);
                    return s.r_acc;
                }
            }

            if (handleMoveCount)
                SciEngine.WriteSelectorValue(segMan, mover, SciEngine.Selector(o => o.b_movCnt), (ushort)mover_moveCnt);

            return s.r_acc;
        }
    }
}