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

using System;
using System.Linq;
using NScumm.Sci.Engine;
using NScumm.Core.Audio;
using NScumm.Core;

namespace NScumm.Sci.Sound
{
    enum SoundStatus
    {
        Stopped = 0,
        Initialized = 1,
        Paused = 2,
        Playing = 3
    }

    class SoundCommandParser
    {
        public const int MUSIC_VOLUME_DEFAULT = 127;
        public const int MUSIC_VOLUME_MAX = 127;
        public const int MUSIC_MASTERVOLUME_DEFAULT = 15;
        public const int MUSIC_MASTERVOLUME_MAX = 15;

        private SciVersion _soundVersion;
        private SegManager _segMan;
        private AudioPlayer _audio;
        private Kernel _kernel;
        private ResourceManager _resMan;
        private bool _useDigitalSFX;
        private SciMusic _music;

        public SoundCommandParser(IMixer mixer, ResourceManager resMan, SegManager segMan, Kernel kernel, AudioPlayer audio, SciVersion soundVersion)
        {
            _resMan = resMan;
            _segMan = segMan;
            _kernel = kernel;
            _audio = audio;
            _soundVersion = soundVersion;

            // Check if the user wants synthesized or digital sound effects in SCI1.1
            // games based on the prefer_digitalsfx config setting

            // In SCI2 and later games, this check should always be true - there was
            // always only one version of each sound effect or digital music track
            // (e.g. the menu music in GK1 - there is a sound effect with the same
            // resource number, but it's totally unrelated to the menu music).
            // The GK1 demo (very late SCI1.1) does the same thing
            // TODO: Check the QFG4 demo
            _useDigitalSFX = (ResourceManager.GetSciVersion() >= SciVersion.V2 || SciEngine.Instance.GameId == SciGameId.GK1 /*|| ConfMan.getBool("prefer_digitalsfx")*/); //TODO: ConfMan

            _music = new SciMusic(mixer, _soundVersion, _useDigitalSFX);
            _music.Init();
        }

        public Register kDoSoundMute(int argc, StackPtr? argv)
        {
            ushort previousState = _music.SoundOn ? (ushort)1 : (ushort)0;
            if (argc > 0)
            {
                // TODO: debugC(kDebugLevelSound, "kDoSound(mute): %d", argv[0].toUint16());
                _music.SoundOn = argv.Value[0].ToUInt16() != 0;
            }

            return Register.Make(0, previousState);
        }

        public void kDoSoundInit(int argc, StackPtr? argv)
        {
            // TODO: debugC(kDebugLevelSound, "kDoSound(init): %04x:%04x", PRINT_REG(argv[0]));
            ProcessInitSound(argv.Value[0]);
        }

        private int GetSoundResourceId(Register obj)
        {
            int resourceId = obj.Segment != 0 ? (int)SciEngine.ReadSelectorValue(_segMan, obj, SciEngine.Selector(s => s.number)) : -1;
            // Modify the resourceId for the Windows versions that have an alternate MIDI soundtrack, like SSCI did.
            if (SciEngine.Instance != null && SciEngine.Instance.Features.UseAltWinGMSound)
            {
                // Check if the alternate MIDI song actually exists...
                // There are cases where it just doesn't exist (e.g. SQ4, room 530 -
                // bug #3392767). In these cases, use the DOS tracks instead.
                if (resourceId != 0 && _resMan.TestResource(new ResourceId(ResourceType.Sound, (ushort)(resourceId + 1000))) != null)
                    resourceId += 1000;
            }

            return resourceId;
        }

        private void ProcessInitSound(Register obj)
        {
            int resourceId = GetSoundResourceId(obj);

            // Check if a track with the same sound object is already playing
            MusicEntry oldSound = _music.GetSlot(obj);
            if (oldSound != null)
                ProcessDisposeSound(obj);

            MusicEntry newSound = new MusicEntry();
            newSound.resourceId = (ushort)resourceId;
            newSound.soundObj = obj;
            newSound.loop = (ushort)SciEngine.ReadSelectorValue(_segMan, obj, SciEngine.Selector(s => s.loop));
            if (_soundVersion <= SciVersion.V0_LATE)
                newSound.priority = (short)SciEngine.ReadSelectorValue(_segMan, obj, SciEngine.Selector(s => s.priority));
            else
                newSound.priority = (short)(SciEngine.ReadSelectorValue(_segMan, obj, SciEngine.Selector(s => s.priority)) & 0xFF);
            if (_soundVersion >= SciVersion.V1_EARLY)
                newSound.volume = (short)ScummHelper.Clip((int)SciEngine.ReadSelectorValue(_segMan, obj, SciEngine.Selector(s => s.vol)), 0, MUSIC_VOLUME_MAX);
            newSound.reverb = -1;  // initialize to SCI invalid, it'll be set correctly in soundInitSnd() below

            // TODO: debugC(kDebugLevelSound, "kDoSound(init): %04x:%04x number %d, loop %d, prio %d, vol %d", PRINT_REG(obj),
            //resourceId, newSound.loop, newSound.priority, newSound.volume);

            InitSoundResource(newSound);

            _music.PushBackSlot(newSound);

            if (newSound.soundRes != null || newSound.pStreamAud != null)
            {
                // Notify the engine
                if (_soundVersion <= SciVersion.V0_LATE)
                    SciEngine.WriteSelectorValue(_segMan, obj, SciEngine.Selector(s => s.state), (ushort)SoundStatus.Initialized);
                else
                    SciEngine.WriteSelector(_segMan, obj, SciEngine.Selector(s => s.nodePtr), obj);
            }
        }

        private void InitSoundResource(MusicEntry newSound)
        {
            if (newSound.resourceId != 0 && _resMan.TestResource(new ResourceId(ResourceType.Sound, newSound.resourceId)) != null)
                newSound.soundRes = new SoundResource(newSound.resourceId, _resMan, _soundVersion);
            else
                newSound.soundRes = null;

            // In SCI1.1 games, sound effects are started from here. If we can find
            // a relevant audio resource, play it, otherwise switch to synthesized
            // effects. If the resource exists, play it using map 65535 (sound
            // effects map)
            bool checkAudioResource = ResourceManager.GetSciVersion() >= SciVersion.V1_1;
            // Hoyle 4 has garbled audio resources in place of the sound resources.
            if (SciEngine.Instance.GameId == SciGameId.HOYLE4)
                checkAudioResource = false;

            if (checkAudioResource && _resMan.TestResource(new ResourceId(ResourceType.Audio, newSound.resourceId)) != null)
            {
                // Found a relevant audio resource, create an audio stream if there is
                // no associated sound resource, or if both resources exist and the
                // user wants the digital version.
                if (_useDigitalSFX || newSound.soundRes == null)
                {
                    int sampleLen;
                    newSound.pStreamAud = _audio.GetAudioStream(newSound.resourceId, 65535, out sampleLen);
                    newSound.soundType = SoundType.SFX;
                }
            }

            if (newSound.pStreamAud == null && newSound.soundRes != null)
                _music.SoundInitSnd(newSound);
        }

        private void ProcessDisposeSound(Register obj)
        {
            throw new NotImplementedException();
        }
    }
}