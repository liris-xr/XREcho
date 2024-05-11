> :warning: **XREcho is no longer maintained, please consider using [PLUME](https://github.com/liris-xr/PLUME) instead.**

<p align="center"><img width="80%" src="xrecho_logo.png" alt="XREcho logo"></p>
<p align="center"><a href="https://www.flaticon.com/free-icons/virtual-reality" style="color: lightgray" title="virtual reality icons">HMD icon created by AB Design - Flaticon</a></p>

# XREcho
Unity plug-in used to record and replay XR interactions, hmd and controllers movement and visualizing trajectory

For any enquiry please use the issue page or send an e-mail to : sophie.villenave@enise.fr

## Installation
* Download the latest package : https://github.com/Plateforme-VR-ENISE/XREcho/releases
* Import the package within your Unity Project
* Find the prefab "XREcho", drag and drop it in your scene

Note : XREcho is plug and play with VR applications using the XR Interaction Toolkit only. Minor modifications must be made to be compatible with other VR framework.

We recommend using OpenXR as your XR backend.

## Sample Scene Instructions
* Clone or download the repository
* Open the project with Unity 2020.3+
* Connect a PC VR HMD of your choosing (tested with HTC Vive Pro Eye, HTC Vive Pro 2 and Oculus Rift S)
* WARNING : Depending on the HMD you'll need to setup Open XR Controllers configuration. Go to Edit -> Project Settings -> XR Plug-in Management -> OpenXR and add your HMD controllers. HTC Vive and Oculus Touch are added by default.
* Open the SampleScene within Unity
* Launch the application via Unity play mode

## Record Parameters and data
### General
To change the config of XREcho recorder to match your needs, several properties can be modified in XREchoConfig and the RecordingManager properties in the Unity inspector.

In the `XREchoConfig` game object, you will be able to modify general settings such as CSV formats (separator, culture, ...), auto-record.

In the `RecordingManager`, you can select the objects to be recorded or layers to be recorded. The tracking rate (in Hz) determines the frequency at which samples are recorded. Warning : at the moment, frequency is limited by the frequency of image rendering. Lastly you can enable eye-gaze recording. Note : You need a compatible XR headset.

For each tracked GameObject, you can specify individual properties. You can specify for each one if its position and rotation has to be tracked, and a specific tracking rate that would be different from the global configuration.

### Eye-Tracking
#### OpenXR (WIP)
* Gaze origin
* Gaze direction

#### HTC Vive Pro Eye
* Gaze origin
* Gaze direction
* Occulometry data

SRAnipal runtime is needed. Download it here : https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/download/latest/ 

#### Varjo XR3
* Gaze origin
* Gaze direction
* Occulometry data

A custom VarjoXR3 runtime is included in XREcho to be compatible with OpenXR.

## Setting Replay Parameters
### Default Models
If you want to have the same replay models across your scenes, we provide default models for hmd, controllers, eye-gaze and missing gameobjects.

## Setting Visualization Parameters
### Heatmap
* By default, a plane is setup as the projection plane for the heatmap
* You can change its size to better fit your scene

## In-App use
### Record
* Session and Project names can be modified
* Hitting the red button will start the recording. Recorded time and data size are displayed. During the recording, XREcho's interface is disabled.
* Hitting the black button while recording will stop the recording. XREcho's interface is then re-enabled.

### Replay
* Select a project and a session
* Available records corresponding the project, session and scene name will be available for selection. Selecting a record loads it into memory.
* When the selected record is loaded, the media bar allows for play/pause and fast forward.
* When replaying, the point of view can be switched from top view to first person view. Note : If your headset is connected, you can watch the replay in through the camera.

### Visualization
#### Trajectory
* Toggle trajectory (main camera) visualization
* Adjust the trajectory width
* Adjust the trajectory complexity i.e. percentage of points used to compute the trajectory. Note : setting to 100% can lead to performance issues

#### Position Heatmap (Current Record)
* Toggle position heatmap
* Change lowest bound : time under bound will be represented as the lowest color (i.e. blue)
* Min button : sets lowest bound to the minimum value recorded (almost always 0)
* Change highest bound : time over bound will be represented as the highest color (i.e. red)
* Max button : sets lowest bound to the maximum value recorded

#### Position Heatmap (Aggregated Records)
* Toggle aggregated position heatmap
* Aggregation is made across same project records

## Reference
Sophie Villenave, Jonathan Cabezas, Patrick Baert, Florent Dupont, and Guillaume Lavoué. 2022. XREcho: A Unity plug-in to record and visualize user behavior during XR sessions. In Proceedings of ACM Multimedia Systems Conference (MMSys ’22). ACM, New York, NY, USA, 6 pages.
