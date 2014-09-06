tfs-monitor
===========

For monitoring TFS - builds monitor, work items, etc. Useful for the big screen in the hallway or developers' desktops

Usage
--------
When cloning the repository, you may notice that the TfsMonitor.Web project is missing the transform files Web.Debug.config and Web.Release.config that are referenced in the .csproj file. They are not required - but you'll need to fill in the `projectCollectionUrl` value in the Web.config if it's not replaced by a value in the transforms. 
