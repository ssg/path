[![.NET](https://github.com/ssg/path/actions/workflows/build-test.yml/badge.svg)](https://github.com/ssg/path/actions/workflows/build-test.yml)
![NuGet](https://img.shields.io/nuget/vpre/Path)

# path
Path is a command-line tool to manage PATH environment variable on Windows.
It's a successor to my [PathCleaner](https://github.com/ssg/PathCleaner) project. 

# installation
Specifying version is required for pre-release projects on NuGet:

```
dotnet tool install path --global 
```

# ⚠️warning
Messing with your PATH can easily cause your system to break or your applications to stop running.
Use only if you know what you're doing. You've been warned.

# why
PATH management in operating systems is inconsistent among platforms (even shells), and it's unnecessarily cumbersome. 
PATH variable gets cluttered over time too with many orphaned entries unnecessarily creating miniscule but non-zero overhead. 
This tool is mostly a UX experiment on how to make PATH management from command-line better.
Although only Windows is supported currently, the ultimate goal is to make this tool the standard syntax
for interacting with paths on any OS. 

I also wanted to develop a tool with .NET 6 + System.CommandLine + Spectre.Console
to learn more about them. I also learned a lot about GitHub Actions while coding this.
Love all of them so far! 

# usage

## path add

Add a directory to user PATH:

```bat
path add C:\some\new\path
```

Want to add it to the machine-level PATH? Run this on an elevated prompt instead:

```bat
path add C:\some\new\path --global
```

## path remove

Want to remove all instances of a directory from a PATH with a single command? Here it is:

```bat
path remove C:\some\existing\path\
```

## path move

Directories in PATH are searched in the order of addition. Want to move a directory higher up in the PATH?
This command will move the directory to the beginning of the PATH:

```bat
path move C:\some\new\path top
```

Want to move a directory after another directory?

```bat
path move C:\some\new\path after C:\Windows
```

or before?

```bat
path move C:\som\new\path before C:\Windows
```

## path analyze

Over time, PATH might get cluttered with empty, orphaned folders without any executables in them. Use
"analyze" command to find them:

```bat
path analyze
```

Analyze can also automatically fix the problems for you:

```bat
path analyze --fix
```

You might be scared of what fix will do, so you can run the same fix command with the `--whatif` argument:

```bat
path analyze --fix --whatif
```

# license
MIT license. See LICENSE.md file for details.
