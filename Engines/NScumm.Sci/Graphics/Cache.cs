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
using NScumm.Sci.Engine;

namespace NScumm.Sci.Graphics
{
    internal class FontCache : HashMap<int, GfxFont> { }
    internal class ViewCache : HashMap<int, GfxView> { }

    /// <summary>
    /// Cache class, handles caching of views/fonts
    /// </summary>
    internal class GfxCache
    {
        private const int MAX_CACHED_FONTS = 20;

        private GfxPalette _palette;
        private GfxScreen _screen;
        private ResourceManager _resMan;
        private FontCache _cachedFonts;
        private ViewCache _cachedViews;

        public GfxCache(ResourceManager resMan, GfxScreen screen, GfxPalette palette)
        {
            _resMan = resMan;
            _screen = screen;
            _palette = palette;

            _cachedFonts = new FontCache();
            _cachedViews = new ViewCache();
        }

        public GfxFont GetFont(int fontId)
        {
            if (_cachedFonts.Count >= MAX_CACHED_FONTS)
                PurgeFontCache();

            if (!_cachedFonts.ContainsKey(fontId))
            {
                // Create special SJIS font in japanese games, when font 900 is selected
                if ((fontId == 900) && (SciEngine.Instance.Language == Core.Common.Language.JA_JPN))
                    _cachedFonts[fontId] = new GfxFontSjis(_screen, fontId);
                else
                    _cachedFonts[fontId] = new GfxFontFromResource(_resMan, _screen, fontId);
            }

            return _cachedFonts[fontId];
        }

        private void PurgeFontCache()
        {
            throw new NotImplementedException();
        }
    }
}