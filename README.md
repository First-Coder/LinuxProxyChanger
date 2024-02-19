<p align="center">
  <img src="https://github.com/First-Coder/LinuxProxyChanger/assets/13957661/9117f5f6-ca11-4bd7-92f1-b4043d1a0140">
</p>

---

[![.NET](https://github.com/First-Coder/LinuxProxyChanger/actions/workflows/dotnet.yml/badge.svg)](https://github.com/First-Coder/LinuxProxyChanger/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/First-Coder/LinuxProxyChanger/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/First-Coder/LinuxProxyChanger/actions/workflows/codeql-analysis.yml)

LinuxProxyChanger is a dotnet core application that allows you to auto adjust the proxy on any linux platform. The application reacts to network changes and uses a hotst to check which network it is on.

## Installation

Install the following packages in the terminal

```bash
$ wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
$ sudo apt update
$ sudo apt install apt-transport-https
$ sudo apt install dotnet-runtime-3.1
$ sudo apt install screen
```

Move the Proxy_Changer directory to /opt/Proxy_Changer/.

Move the file LinuxProxyChanger.service to the Systemd directory.

activate the autostart with the following command

```bash
systemctl enable LinuxProxyChanger
```

## Usage

Start programme manually
```bash
systemctl start LinuxProxyChanger
```

Stop programme manually
```bash
systemctl stop LinuxProxyChanger
```

Check programme status
```bash
systemctl status LinuxProxyChanger
```

You can call up the console of the program with the following command

```bash
screen -r LinuxProxyChanger
```
## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[GPL-3.0](https://choosealicense.com/licenses/gpl-3.0/)

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate?hosted_button_id=8PBF4BN7R46TE)
