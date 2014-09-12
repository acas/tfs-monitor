tfs-monitor
===========

For monitoring TFS - builds monitor, work items, etc. Useful for the big screen in the hallway or developers' desktops. A work in progress, the TFS Monitor currently only monitors builds. It is rough around the edges, but should work. Mostly.

Usage
--------
When cloning the repository, you will need to add a file next to the `Web.config` called `Web.Private.config`. The file should have appSettings entries for:

*`projectCollectionUrl` - The location of your TFS project collection 
*`acasLibrariesPath` - A url that can go in a script tag that references the acas-libraries javascript code. 

Optionally, you can add:
*`projectRegex` - a Regular Expression that will limit which projects are considered for work item queries and build definitions
*`buildDefinitionRegex` - a Regular Expression that will limit which build definitions are included (in addition to the limit the `projectRegex` performs)

For example:

```
	<appSettings>
	  <add key="projectCollectionUrl" value="https://mytfssite.com/MyProjectCollection" />	
	  <add key="acasLibrariesPath"    value="//cdn.mysite.com/acas-libraries.js" />	
          <add key="projectRegex"    value="MyProject" />	
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