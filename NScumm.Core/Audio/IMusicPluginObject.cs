//
//  IMusicPluginObject.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
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

using System.Collections.Generic;

namespace NScumm.Core.Audio
{
    /// <summary>
    /// Interface for the plugin objects which handle plugins
    /// instantiation. Subclasses for this may be used for engine plugins and other
    /// types of plugins. An existing IPluginObject refers to an executable file
    /// loaded in memory and ready to run. The plugin, on the other hand, is just
    /// a handle to the file/object, whether it's loaded in memory or not.
    /// </summary>
    interface IPluginObject
    {
        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        /// <value>The name.</value>
        string Name{ get; }
    }

    interface IMusicPluginObject: IPluginObject
    {
        /// <summary>
        /// Gets a unique string identifier which will be used to save the
        /// selected MIDI driver to the config file.
        /// </summary>
        /// <value>The identifier.</value>
        string Id { get; }

        /// <summary>
        /// Gets a list of the available devices.
        /// </summary>
        /// <value>The devices.</value>
        IList<MusicDevice> GetDevices();

        /// <summary>
        /// Checks whether a device can actually be used. Currently this is only
        /// implemented for the MT-32 emulator to check whether the required rom
        /// files are present.
        /// </summary>
        /// <returns><c>true</c>, if device was checked, <c>false</c> otherwise.</returns>
        /// <param name="handle">Handle.</param>
        bool CheckDevice(DeviceHandle handle);

        /// <summary>
        /// Creates a MIDI Driver instance based on the device
        /// previously detected via MidiDriver.DetectDevice()
        /// </summary>
        /// <param name="handle">Handle.</param>
        MidiDriver CreateInstance(IMixer mixer, DeviceHandle handle);
    }
}