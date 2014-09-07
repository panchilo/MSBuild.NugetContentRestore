MSBuild.NugetContentRestore README
==================================

Congratulations! MSBuild.NugetContentRestore is now installed to your project.
A reference to NugetContentRestoreTask has been added to your project.
The only remaining step is to use it. Here is an example of how (and when) I use it. 
Edit your project file so it looks something like this:

	<Target Name="BeforeBuild">
	  <NugetContentRestoreTask SolutionDir="$(SolutionDir)" ProjectDir="$(ProjectDir)" />
	</Target>

All NuGet Packages Content Folders will be copied to your project folder right before is built.
