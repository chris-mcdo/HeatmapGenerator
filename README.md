# HeatmapGenerator
C# console application: generates heatmaps from eye-tracking data (JSON) and image files (BMP). Creates "instantaneous" or global heatmaps, and videos (with addons).
## What this does
1. Reads eye-tracking data in JSON format (see samples).
2. Generates intensity heatmap using Gaussian smoothing (kernel) function.
3. Combines background image with heatmap using alpha compositing.
4. Optionally generates video using ffmpeg and mp4fpsmod tools.

## Setup
1. Ensure the eye-tracking data and image files are in the correct format and folder organisation. Or: modify the read-in code. See sample provided
2. Install ffmpeg and fpsmp4mod and add them to your system path (optional; for video):
    * The latest ffmpeg build is [here](https://ffmpeg.zeranoe.com/builds/ "Zeranoe"). Setup instructions [here](https://video.stackexchange.com/questions/20495/how-do-i-set-up-and-use-ffmpeg-in-windows).
    * The latest mp4fpsmod build is on [this page](https://sites.google.com/site/qaacpage/cabinet).

## Running the tool
1. Select the function:
    * "generate instantaneous heatmaps" creates heatmap images (png) for individual frames. After they are generated there is the option to create a video.
    * "generate global heatmaps" creates a single heatmap image (png) for eye-tracking data over a given range.
2. Select the folder containing the image frames.
3. Enter the start and end frames to read, in ms:
    * For instantaneous heatmaps, this is the range of frames to generate (png) images for; or the timespan of the video.
    * For global heatmaps, the start time defines the specific image to draw the heatmap over. The end frame defines the period of time over which eye-tracking points are generated.
