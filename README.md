# 1. Crestron Network Monitor

This project allows IT & AV System professionals to remotely Shutdown, Restart, Sleep, a PC over a network, using simple UDP frames that can be generated from controls systems such as Crestron, AMX, Medialon, Widget Designer, or even from a terminal/powershell script, or a custom application that has access to low level networking functionality.

Additionally, the application listens for a Ping message, and responds with Pong, so that the User Interface of your control system can verify that the PC is up and running at the application level (as opposed to a low level ICMP ping that just tells you that the PC network stack is up, but doesn't tell you if the computer is locked up). *Important Note* This is not an ICMP Ping, this is a command and response from the application.

For information on how to control this application & PC's with Crestron, please refer to this [blog post](http://3-byte.com/blog/2010/11/18/network-shutdown)

It's very similar for AMX. 

# 2. App Settings

There is a simple appsettings.json file included with the project. It's recommended to leave this alone, but you can adjust the UDP listener port, which is defaulted to 16009. The application will log which port it's using on startup, and there is error-checking in the application logic, so that if a port number is malformed or invalid, the port will default back to 16009, and the log viewers will tell you. 

# 3. Frame Syntax

The application is expecting a carriage return on every frame. The syntax is either a "!0D" or a "\r" depending on your control system. It will also return the PONG command with a carriage return. Note, you can send ASCII frames in lower, pascal, or camel case, as the application confirms them with a .ToUpper method. 

# 4. Frames

**Commands**
1. SHUTDOWN!0D
2. RESTART!0D
3. SLEEP!0D
4. PING!0D

**Responses**
1. PONG!0D (only to a PING!0D)

Note that all of these commands with the exception of the Ping are forced commands on the PC receiving them, and any data will be lost. This application is intended for museum and corporate environments where there are many PC's connected to display devices such as projectors and touchscreens and it would be tedious to manually shut them down daily, and wasteful to keep them running all night when the system is not in use.

# 5. Security

There is no security implemented on the application level. It's assumed that the network is secure. If there is a desire for some security mechanism, please open a ticket and it will be considered. 

# 5. Network Settings

The application will listen on any available network adapter, including multiple IP addresses bound to the same NIC. The port to which it responds (it only every responds to the PING command, with PONG) initialized with zero until a frame is sent, and the sender will either pass an explicit or random port to respond, the application will take that port number, and send the Pong message over that port. This is usually handled by the implementation of the network device in control systems, noting to worry about. 

# 6. Medialon Manager Usage

For a Medialon MLLC (which is somewhat redundant since Medialon provides a PC watchdog), use this MLLC driver, which has the commands and a monitoring feature to automatically send the ping command. 

The application starts up minimized, but has two log views so you can easily see communications with the controller sending message, and startup information. 

# 7. Windows Event Viewer
THe application can also log to the Windows Event Viewer, this is especially helpful to persist logs over long periods of time, and when working with enterprise management platforms, like Azure dashboards, ActiveDirectory, or aggregators like loggy, this can be super helpful rather than collecting logs from local drives on dozens or hundreds of machines. Some good info [here](https://www.loggly.com/ultimate-guide/centralizing-windows-logs/)

It is essential that this powershell script be run (and not changed. In the future I may make the logging context adjustable in the appsettings.json file, but for now the Windows Event Logger context is hard-coded).

Once run, refresh the event viewer by selecting "Action" in the menu, and then "Refresh". You should see the first entry like this:

![](CrestronNetworkMonitorWPFUI/Screenshots/EnableEventViewer.png)







