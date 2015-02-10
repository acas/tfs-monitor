TFS Monitor
===========

TFS Monitor monitors builds (continuous integration and manual), work items and sprints. Useful both for developers' desktops and the big screen in the hallway or central location. It is rough around the edges, not everything works and the code is a mess. But we use it and it works...most of the time.

Tested on the following system, YMMV:
* TFS 2013
* Windows Server/IIS
* Chrome

TFS Monitor's chief benefits over Microsoft's Team Web Access solution are:
* TFS Monitor uses SignalR to refresh when changes are made to the underlying data. This makes it ideal for unattended use on a large, public monitor
* TFS Monitor provides views that span multiple projects. For whatever reason, TFS considers each project its own siloed universe. For teams that juggle multiple projects
 at once, TFS Monitor gives developers and others a way to quickly see the status of all their projects in one place.
* TFS Monitor provides a clean, clutter-free view of the status of your builds/sprints, making it ideal for an unattended large monitor.


Usage
--------
When cloning the repository, you will need to add a file next to the `Web.config` called `Web.Private.config`. The file should have appSettings entries for:

* `projectCollectionUrl` - The location of your TFS project collection 
* `acasLibrariesPath` - A url that can go in a script tag that references the acas-libraries javascript code. 

Optionally, you can add:
* `projectRegex` - a Regular Expression that will limit which projects are considered for work item queries and build definitions
* `buildDefinitionRegex` - a Regular Expression that will limit which build definitions are included (in addition to the limit the `projectRegex` performs)
* `bingBackgrounds` - if `true`, TFS Monitor will use the Bing background image of the day as the background of the monitor, 
making your TFS Monitor a part of your office's decor

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
* Make sure the identity IIS runs under (the Application Pool Identity) has access to TFS. 
* Enable 32-bit Applications (Application Pool)
* Possibly some other configuration stuff that just happened to work in our environment....
* You're done!

When you clone the repository, you'll notice that there's a `.pubxml` file included in the project that isn't included in source control. You can add this back in to easily deploy the application to your server. 
