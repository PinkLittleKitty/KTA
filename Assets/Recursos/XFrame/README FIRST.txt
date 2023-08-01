 ***********************************
*      X-FRAME FPS ACCELERATOR     *
* (C) Copyright 2016-2018 Kronnect * 
*            README FILE           *
 ***********************************


How to use this asset
---------------------

Thanks for purchasing X-Frame FPS Accelerator.

To use the asset in your project, select your camera and add the component "X-Frame FPS Accelerator" script to it.
Use the custom inspector to customize the behaviour. Many properties in the inspector shows a tooltip with some additional details.


Hints
-----

- Test results on real mobile device.
- If you experiment problems with the asset, try a different Render Method (option in X-Frame inspector).


Support
-------

* Email support: contact@kronnect.me
* Website-Forum Support: http://kronnect.me
* Twitter: @KronnectGames


Other Cool Assets!
------------------

Check our other assets on the Asset Store publisher page:
https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:15018



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of X-Frame FPS Accelerator will be eventually available on the Asset Store.



Version history
---------------

Version 3.1 Current Release
- Added Enable Click Events to support scene gameobjects that require OnMouseDown/OnMouseUp events
- [Fix] Fixed HDR issue with Second Camera Blit mode

Version 3.0.1 2018-05-25
- [Fix] Fixed unnecessary memory allocations when X-Frame method is set to disabled

Version 3.0 2018-05-15
- Support for Lightweight Rendering Pipeline in Unity 2018.1
- Option to show FPS on screen

Version 2.3 2018-05-03
- API: Added ScreenToCameraRay function: takes into account current downsampling factor
- [Fix] Changes in X-Frame inspector were not marking scene as dirty (pending save)

Version 2.2.10 2017.12-18
- UI Canvas: Added support for UI events on canvases in World Space
- UI Canvas: Added support for OnPointerClick proxy

Version 2.2.9 2017.11.24
- Added support for Canvas UI events on canvases with Screen Space - Camera option enabled
- Removed Simple rendering mode (Unity 2017.2 and up)
- [Fix] Fixed Canvas UI scaling issue when canvas is configured as Screen Space - Camera

Version 2.2.8 2017.08.15
- [Fix] Fixed Second Camera Blit issues with Unity 2017.1

Version 2.2.7 2017.06.26
- Added user-defined camera clear flags parameter for X-Frame camera
- [Fix] Improved billboard creation so any residual billboard is reused

Version 2.2.6 2017.06.12
- [Fix] Fixed Canvas UI wrong scale with Simple compositing method

Version 2.2.5 2017.05.08
- [Fix] Fixed graphic glitch on Unity 5.6

Version 2.2.4 2017.04.15
- [Fix] Fixed regression bug which was limiting max FPS

Version 2.2.3 2017.03.17
- Improved Second Camera Blit method
- Added Prewarm option to pre-generate the render buffers at the start
- [Fix] Fixed crash with Simple Mode and Quad/Vertical downsamplers

Version 2.2.2 2017.02.26
- [Fix] Fixed stuttering on some Android devices when enabling Antialias setting

Version 2.2.1 2017.01.15
- [Fix] Fixed inspector issue when disabling non-compliant image effects
- [Fix] Fixed issue when niceFPS triggers

Version 2.2 2016.08.18
- General shader optimizations
- New downsampling parameter
- Effect can now be seen in Scene View in Unity 5.4
- Added niceFPS, adaptation speed up and adaptation speed down parameters.

Version 2.1 2016.08.05
- Two new render methods added for best compatibility (Billboard World Space and Billboard Overlay).

Version 2.0 2016.08.01
- Simplified workflow. Two render methods allowed: simple or second camera with optional sharpen pass.
- [Fix] Fixed lag spikes due to invalid antialias setting.

Version 2.0 2016.07.15:
- New filtering option (point / bilinear) for upscaling frames
- Quality setting was reset when entering into playmode
- [Fix] Fixed issue when switching cameras

Version 1.0 2016.06.14 - First release

