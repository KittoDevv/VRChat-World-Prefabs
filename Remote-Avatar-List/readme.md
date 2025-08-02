# **Remote Avatar List**
This prefab allows you to remotely edit your Avatar Lists using a website like Pastebin to host the list on.

<img width="610" height="649" alt="Screenshot 2025-08-02 085924" src="https://github.com/user-attachments/assets/acaa6f31-7cf8-4b6d-b007-448829e10962" />

## **How to set this Prefab up in your world**
To set up this prefab, use Pastebin to host the list it is whitelisted by VRChat, so it works regardless if Untrusted URLs are turned on.
It is recommended to leave the paste visibility on Unlisted, but Public works just as fine. You cannot use Private for visibility.
After creating your pastes, get the "Raw" version's URL and put it into the script. The Raw link will look something like this: https://pastebin.com/raw/xxxxxxx

To properly set up the list, format the list like this:

Avatar ID, Avatar Name, Creator

You cannot leave a part or section blank or missing, or it may not work as intended in the list.
If you don't want to use avatar names, just use "Avatar" in place of the name.

When adding/removing slots, make sure they are in the same position in the array, or labels/pedestals may be incorrectly placed/swapped or in the wrong position.
