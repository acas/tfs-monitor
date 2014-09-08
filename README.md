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
