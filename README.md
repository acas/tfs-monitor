tfs-monitor
===========

For monitoring TFS - builds monitor, work items, etc. Useful for the big screen in the hallway or developers' desktops. A work in progress, the TFS Monitor currently only monitors builds. It is rough around the edges, but should work. Mostly.

Usage
--------
When cloning the repository, you will need to add a file next to the `Web.config` called `Web.Private.config`. The file should have the location of your TFS project collection: 

```
	<appSettings>
	  <add key="projectCollectionUrl" value="https://mytfssite.com/MyProjectCollection" />
	</appSettings>
```


Install
---------
To install TFS Monitor in IIS:
*Make sure the identity IIS runs under (the Application Pool Identity) has access to TFS. 
*Enable 32-bit Applications (Application Pool)
*Possibly some other configuration stuff that just happened to work in our environment....
*You're done!

When you clone the repository, you'll notice that there's a `.pubxml` file included in the project that isn't included in source control. You can add this back in to easily deploy the application to your server. 