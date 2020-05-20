# HardView2

HardView2 is a simple, low-distraction image viewer. I use it to sort pictures before adding them to Lightroom.

The name comes from an old Delph-based image viewer called hardview, by Tony Mills. I used it for many years
before a Windows upgrade finally stopped it working.


## Getting Started

1. Download the pre-built binary from the [releases](https://github.com/andyjohnson0/HardView2/releases)
page, or clone the repo and build it yourself.

2. Add `hv.exe` to your path

3. Run `hv`. By default it will display the contents of your My Pictures folder.
It supports jpeg and png format pictures only, and the file extension must be
`.jpg` or `.jpeg` or `.png`.

## Using HardView2

The program is mostly keyboard driven:

* Scroll through the pictures using the **left** and **right** arrow buttons. 
Press the **down** arrow button to select a sub-directory. 
Press the **up** arrow button to move to the parent directory.

* Press **Return** to move to a random picture.
**Home** and **End** move to the first and last pictures respectively.

* **Space** moves the current picture to a sub-directory named `temp`. This is useful for sorting pictures into
keep vs not keep as part of a simple photographic workflow (e.g before adding to Lightroom or similar).

* **F** shows a directory browse dialog to move to a new directory.

* **Delete** moves the current photo to the Windows recycle bin.

* **I** toggles displaying picture information.

* **S** toggles scaling pictures to fit the screen.

* **PgDn** zooms in.

* **PgUp** zooms out.

* **R** resets scaling and zooming.

* **Escape** exits the program.

* You can also use the mouse wheel to zoom in/out, and drag to drag the image.

## Prerequisites

.net framework 4.5 or later.

Visual Studio 2019 to build.

## Author

Andy Johnson | https://github.com/andyjohnson0 | http://andyjohnson.uk

## Licence

This project is licensed under the terms of the MIT license. See the licence file for more information.
