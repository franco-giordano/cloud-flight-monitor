# cloud-flight-monitor

Simple tray icon app displaying current battery level for HyperX Cloud Flight Wireless headsets. Built with [MightyHID](https://github.com/MightyDevices/MightyHID) and WPF .NET 3.1

## Usage

Just download the zip file from [Releases](https://github.com/franco-giordano/cloud-flight-monitor/releases), extract and execute. No installation needed. You can customize the default tray icons simply by changing the files under icos/.

## Development

Developed with Visual Studio 2019 CE. Make sure to setup MightyHID (under lib/) and System.Windows.Forms as Project References, as well as install latest System.Drawing.Common from Nuget.

## License

MIT

## Other Resources

Heavily inspired from [crazyklatsch/Arctis7_BatteryReader](https://github.com/crazyklatsch/Arctis7_BatteryReader). Cloud Flight HID values taken from [srn/hyperx-cloud-flight-wireless](https://github.com/srn/hyperx-cloud-flight-wireless/).
