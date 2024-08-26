---
order: 2
icon: container
---
# Installing and Setting Up SQL Server 2022 in Docker

+++ macOS

Docker released beta support today for Apple’s Rosetta 2 x86 emulation layer, which means you can run SQL Server on Apple M1 or Apple M2 silicon using this option.

1. Download and install the latest Docker for Apple Silicon
2. Once Docker Desktop is running, open the Dashboard and go into Settings
3. Find the “Features in development” option, and select the “Use Rosetta for x86/amd64 emulation on Apple Silicon” checkbox
4. Restart the Docker engine
5. In the following Quickstart, please us the following command "sudo docker pull woodruffsolutions/sql-2019-chinook"
6. Follow the instructions on <a href="https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker" target="_blank">Quickstart: Run SQL Server Linux container images with Docker</a> to install SQL Server
7. You can ignore the warning that the “requested image’s platform (linux/amd64) does not match the detected host platform”

+++ Windows

## WSL 2 backend

- WSL version 1.1.3.0 or above.
- Windows 11 64-bit: Home or Pro version 21H2 or higher, or Enterprise or Education version 21H2 or higher.
- Windows 10 64-bit: Home or Pro 21H2 (build 19044) or higher, or Enterprise or Education 21H2 (build 19044) or higher.
- Enable the WSL 2 feature on Windows. For detailed instructions, refer to the [Microsoft documentation](https://docs.microsoft.com/en-us/windows/wsl/install-win10).
- The following hardware prerequisites are required to successfully run WSL 2 on Windows 10 or Windows 11:
  - 64-bit processor with [Second Level Address Translation (SLAT)](https://en.wikipedia.org/wiki/Second_Level_Address_Translation)
  - 4GB system RAM
  - BIOS-level hardware virtualization support must be enabled in the BIOS settings. For more information, see [Virtualization](https://docs.docker.com/desktop/troubleshoot/topics/#virtualization).
- Download and install the [Linux kernel update package](https://docs.microsoft.com/windows/wsl/wsl2-kernel).

## Install Docker and Docker Desktop

1. Download Docker Desktop for Windows from https://www.docker.com/products/docker-desktop.
2. Open the downloaded setup and grant administrator privileges, if required.
3. Follow the setup wizard to complete the installation of Docker Desktop.
4. Restart your PC for the changes to take effect.
5. Start Docker Desktop from the Windows Start menu, then select the Docker icon from the hidden icons menu of your taskbar.

## Docker Containers

``` output
docker pull woodruffsolutions/sql-2019-chinook
```
+++

Finally, follow the instructions on <a href="https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker" target="_blank">Quickstart: Run SQL Server Linux container images with Docker</a> to install SQL Server