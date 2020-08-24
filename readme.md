# Applications

## Console v1

A legacy method of searching the exact process which locks the specified file.<br/>
Use *ExclusionFolders.txt* and *ExclusionProcesses.txt* to speed-up the process.

## Console v2

Allows to find the exact process pretty quick via Restart Manager API.

### Usage

Run the application with a specified file to unlock it.<br/>
`FileLockedBy.exe myDocument.docx`<br/>

### Explorer integration

Register the application<br/>
![alt](img/register.png)<br/>
Navigate to a locked file which needs to be deleted<br/>
![alt](img/locked.png)<br/>
Do mouse right-click on the file<br/>
![alt](img/menu.png)<br/>
Click *Unlocker* in the menu<br/>
![alt](img/unlocked.png)<br/>
Click *Unlocker* once again to be sure the file is unlocked<br/>
![alt](img/notfound.png)<br/>
Delete the file<br/>
![alt](img/word.png)<br/>
*PROFIT!*

*Microsoft.Win32.Registry* is used due to .Net core limitations.<br/>