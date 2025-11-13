# GUI Stats and Related Scripts
This is my collection of small, general, or useful scripts that can be used for various reasons.
The Unity Package included in this directory of this repository contains all the scripts mentioned below.
These should support Unity Text, UI, and non-UI TextMeshPro components to display info on.
They should also allow for less frequent updates for performance, reducing redundant checks as needed. 

---

## Date/Time Display
A modified/fork of Jetdog's DateTime_UI script. 

It allows you to display the time in any format supported and in any time zone. 

<img width="520" height="347" alt="Screenshot 2025-09-06 083719" src="https://github.com/user-attachments/assets/21ca8f14-1c00-48f9-a0aa-76adf2340e5e" />

## FPS Counter Display
Uses `Time.unscaledDeltaTime` to measure frame timing without being affected by time scaling.

It should be as accurate as the FPS counter in the quick menu in VRChat. 

<img width="413" height="149" alt="Screenshot 2025-09-06 083930" src="https://github.com/user-attachments/assets/1ae4c5e6-6d48-44a2-8183-fd3cb6987475" />

## Player Counter Display
Shows the number of currently connected players in the current instance.

<img width="264" height="165" alt="Screenshot 2025-11-13 072236" src="https://github.com/user-attachments/assets/acd38314-8845-43a3-8f29-0eec70e09b00" />


## Time Since Display
A customizable display that lets you know how long it has been since a certain time. 

It can also be used to count down to something, like a date or time, toggling the "Countdown" mode to true in the inspector.

You can hide individual parts of the display, like the dates (Years, Months, Days) and time (Hours, Minutes, Seconds).

You can also enable prefixes and customize them freely in the inspector. 

<img width="420" height="289" alt="Screenshot 2025-09-06 084902" src="https://github.com/user-attachments/assets/6c4b4c3f-d17e-40d8-966e-fb4c44a332d6" />

## Remote Text Loader
Loads text from sites like Pastebin to display to players.

This can be useful to display updates, news, or changelogs.

<img width="470" height="312" alt="Screenshot 2025-09-06 102528" src="https://github.com/user-attachments/assets/5430ea77-9106-4f60-b4d8-4b3603172f7d" />
