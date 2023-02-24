# Build Instructions for Instance Hosts

# Linux
 Applies to `aarch64` and `amd64` servers only.
## Hardware Requirements:
- 64-bit x86 or ARM CPU
- 1 GB RAM 

MySQL is very intensive, you should be able to run this on even a potato though!
## Software Requirements:
- .NET 7 SDK
- ASP.NET 7 Runtime
- MySQL or MariaDB
- A will to play LBP after 2021

### Arch
```
# pacman -Sy dotnet-sdk aspnet-runtime mysql
```
Please note that this will install MariaDB
### Debian/Ubuntu
TBW, .NET 7 is provided by Microsoft rather than the distro maintainers.
```
# apt install mysql
```
### Gentoo
```
# emerge --ask --verbose dotnet-sdk-bin mysql
```
## Building
Clone `Project Lighthouse`
```
$ git clone https://github.com/LBPUnion/ProjectLighthouse.git
```
Build for `Production`
```
$ dotnet build -c Release
```

# Windows
TBW