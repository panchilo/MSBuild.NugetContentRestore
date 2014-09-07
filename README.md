#MSBuild.NugetContentRestore
MSBuild task to restore Nuget content files to project folder

##Introduction
MSBuild.NugetContentRestore takes care of copying your Nuget content files to your project folder. This is helpful specially for HTML front-end packages like angularjs, bootstrap or any Nuget package that contains a Content folder.
During installation of the front-end package (using Install-Package), all Package Content is copied to your project folder. The problem is that, in general, you don't want to check those files into your version control system. Your would expect that "nuget restore" will also take care of copying the content files but this is not the case. This issue has been repetitively reported here:
- http://nuget.codeplex.com/workitem/2094
- http://nuget.codeplex.com/workitem/1239

And Jeff Handley explained why this is not the case here:
- http://jeffhandley.com/archive/2013/12/09/nuget-package-restore-misconceptions.aspx

Other interesting solution to workaround this issue is available here:
- https://github.com/baseclass/Contrib.Nuget

I wanted to keep things simpler and let the project take care of the hard work. 

##Download
To install MSBuild.NugetContentRestore, run the following command in the Package Manager Console:
    
	PM> Install-Package MSBuild.NugetContentRestore
	
More information about MSBuild.NugetContentRestore NuGet Package available at https://www.nuget.org/packages/MSBuild.NugetContentRestore/

##Usage
After installing MSBuild.NugetContentRestore using NuGet, your Visual Studio Project (.csproj, .vbproj) will have a reference to MSBuild.NugetContentRestore Task. The only remaining step is to use it. Here is an example of how (and when) I use it. Edit your project file so it looks something like this:

	<Target Name="BeforeBuild">
	  <NugetContentRestoreTask SolutionDir="$(SolutionDir)" ProjectDir="$(ProjectDir)" />
	</Target>

All NuGet Packages Content Folders will be copied to your project folder right before is built.

##License
The MIT License (MIT)

Copyright (c) 2014, Francisco Lopez

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.