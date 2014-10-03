//
//  ScummEngine3.cs
//
//  Author:
//       Scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
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

using NScumm.Core.Graphics;
using NScumm.Core.Input;
using NScumm.Core.Audio;
using NScumm.Core.IO;

namespace NScumm.Core
{
    public class ScummEngine3: ScummEngine
    {
        public ScummEngine3(GameInfo game, IGraphicsManager graphicsManager, IInputManager inputManager, IAudioDriver audioDriver)
            : base(game, graphicsManager, inputManager, audioDriver)
        {
            Variables[VariableCurrentLights.Value] = (int)(LightModes.ActorUseBasePalette | LightModes.ActorUseColors | LightModes.RoomLightsOn);
        }
    }
    
}
