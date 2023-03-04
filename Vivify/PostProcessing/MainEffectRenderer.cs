﻿using System;
using IPA.Utilities;
using UnityEngine;

namespace Vivify.PostProcessing
{
    // SetTargetBuffers for some reason causes OnRenderImage to recieve a null source.
    // This class allows anyone to apply effects to any render texture
    internal class MainEffectRenderer
    {
        private readonly Action<MainEffectController, RenderTexture, RenderTexture> _imageEffectControllerCallback =
            MethodAccessor<MainEffectController, Action<MainEffectController, RenderTexture, RenderTexture>>
            .GetDelegate("ImageEffectControllerCallback");

        private readonly Action<MainEffectController> _onPreRender = MethodAccessor<MainEffectController, Action<MainEffectController>>.GetDelegate("OnPreRender");
        private readonly Action<MainEffectController> _onPostRender = MethodAccessor<MainEffectController, Action<MainEffectController>>.GetDelegate("OnPostRender");

        private readonly MainEffectController _mainEffectController;

        internal MainEffectRenderer(MainEffectController mainEffectController)
        {
            _mainEffectController = mainEffectController;
        }

        internal void Render(RenderTexture src, RenderTexture dest)
        {
            _onPreRender(_mainEffectController);
            _onPostRender(_mainEffectController);
            _imageEffectControllerCallback(_mainEffectController, src, dest);
        }
    }
}
