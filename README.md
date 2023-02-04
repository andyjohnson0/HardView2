# HardView2

HardView2 is a simple, low-distraction image viewer. This repo contains Windows and Xamarin/Android implementations.
I use the Windows version to sort pictures before adding them to Lightroom. I built the Android version primarily as
a learning excersise to re-familiarise myself with Xamarin and the Android platform.

The name comes from an old Delph-based image viewer called Hardview (for "hardware viewer", I believe), by Tony Mills.
I used it for many years before a Windows upgrade finally stopped it working.


## Getting Started

### Windows

1. Install .net framework 4.7.2 or later.

2. Download the pre-built binary `hv.exe` from the [releases](https://github.com/andyjohnson0/HardView2/releases)
page, or clone the repo and build it yourself.

3. Add `hv.exe` to your path

4. Run `hv`. By default it will display the contents of your My Pictures folder.
It supports jpeg and png format pictures only, and image file extensions must be `.jpg` or `.jpeg` or `.png`.

### Android

1. You'll need a device or emulator running Android 11 or later. 

2. Download `uk.andyjohnson.HardView2.apk` from the [releases](https://github.com/andyjohnson0/HardView2/releases)
page (starting at v0.7) and side-load it, or clone the repo and build it yourself.



## Using HardView2

### Windows

The program runs full-screen and is mostly keyboard driven:

* Scroll through the pictures using the **left** and **right** arrow buttons. 
Press the **down** arrow button to select a sub-directory. 
Press the **up** arrow button to move to the parent directory.

- Press **Return** to move to a random picture.
**Home** and **End** move to the first and last pictures respectively.

- **Space** moves the current picture to a sub-directory named `temp`. This is useful for sorting pictures into
keep vs not keep as part of a simple photographic workflow (e.g before adding to Lightroom or similar).

- **F** shows a directory browse dialog to move to a new directory.

- **Delete** moves the current photo to the Windows recycle bin.

- **I** toggles displaying picture information.

- **S** toggles scaling pictures to fit the screen.

- **PgDn** zooms in and **PgUp** zooms out.

- **Shift** plus **Up**/**Down**/**Left**/**Right** pans the image (useful when zooming).

- **R** resets scaling and zooming.

- **M** displays a menu allowing the application to be registered with Windows explorer.

- **Escape** exits the program.

- You can also use the mouse wheel to zoom in/out, and drag to drag the image.

The program supports jpeg and png image files only.

### Android

The app runs full-screen and is touch-driven.

Initially a menu of buttons is displayed. From left to right their functions are:

- Select directory/folder
- Browse images in current directory
- Show/hide image name
- Show image properties
- About the app

After selcting a directory, the first image is displayed. Touch gestues can then be used.
The display is divided into a 3x3 grid, with short or long taps in each area having a 
different meaning:

- Left middle/bottom:
  - Short tap: previous image
  - Long tap: first image
- Right middle/bottom:
  - Short tap: next image
  - Long tap: last image
- Middle/bottom:
  - Short tap: random image
- Top:
  - Short tap: show menu
- Centre:
  - Long tap: reset zoom and scroll

Use two-finger pinch to zoom the image, and single-finger drag to scroll.

The app supports jpeg and png image files only.


## Author

Andrew Johnson | https://github.com/andyjohnson0 | http://andyjohnson.uk


## Licence

Except for third-party elements that are licened separately, this project is licensed under
the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

The "stack of photos" icon used in this software is copyright [Icons8 LLC](https://icons8.com)
and is used under the terms of their [free licence](https://icons8.com/license).

Android icons are copyright Google LLC and are used under their
[open source licence](https://developers.google.com/fonts/faq).
