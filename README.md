# boARd
##### Augmented reality Android application based around local multiplayer board games

***
## Configuring Environment

#### Android SDK
Download: http://developer.android.com/sdk/index.html
* Download Android Studio to ensure full Android support. When seleclting what to install from the SDK Manager, make sure to include all build, platform, and debugging tools along with the Support Library, the Google USB Driver, and Android NDK
* Set your build target to your device's appriate target (for development purposes). The actual version target for the app will be set in Unity

#### Unity
Download: https://unity3d.com/get-unity/download/archive
* To ensure compaitability with Qualcomm's Vuforia AR API, we must use the 32-bit version of Unity 5.3.1. Got to the link above and select Unity Editor 32-bit for the 5.3.1 build (for Windows) or Unity Editor (for Mac)
* Follow the standard Unity installation process
* Go back to the page mentioned above and download the Unity Installer. You don't actually use this to install Unity, rather to install the Android module needed to build a project for Android. Install only that package
* Once Unity is installed, go to Edit > Preferences > External Tools and set the paths for the SDK and JDK folders
* Instead of making a project, clone the repo into a folder where Unity will look for projects and import the Repo as a project. This way, any changes you make to the project will be made to the same contents as the repo, which will in turn reflect the changes in your commits.

#### Unity Remote
Download: https://play.google.com/store/apps/details?id=com.unity3d.genericremote&hl=en
* Unity Remote allows you to test your app without having to push it to your device by setting it so when you press the play button in Unity, the scene starts playing on both Unity AND the device. In the case of this app the AR functionality does not occur, but all other functionality (menus, interacting with the game pieces, etc.) can be tested this way
* Download and install the app, then make sure USB Debugging is turned on
* You need to setup the app in a specific order for it to pick up on Unity. To do this, close Unity if it's currently open then do the following:
* First, connect your phone to your computer. Make sure the transfer mode is either MTP or PTP (not just charging)
* Next, open the app to the initial waiting screen.
* Finally, you can now launch Unity on your computer. To test if its working, try pressing the play button and see if the scene is mirrored onto your device. If not, you'll need to play around with it, there's lots of posts online about it
* 
***

## Resources

Below are some yotube channels/tutorial that may be useful reference material:
* Unity's own tutorials: https://unity3d.com/learn/tutorials
* Android and Vuforia tutorials: https://www.youtube.com/channel/UCBVWZH7ZrnegbWiK9pY5V-A/videos
* Unity (with a focus on Android) tutorials: https://www.youtube.com/channel/UC1KVAbDOeaViHLiFv0Y_61g/videos