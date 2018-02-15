## SimulatedDevices ##

In this repo you can find some samples of devices connected to [Azure IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-what-is-azure-iot). There are many tutorials to create your own application connected to IoT Hub, here you have some code that you can download and use for quick and easy tests. You just need to create your own IoT Hub in your Azure subscription, register your device and copy the connection string in the *App.config* of the project you want to run. 

*If you need more details on the creation of IoT Hub and device management, please read the [official documentation](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-how-to)*. 

Here's what you can find in this repo:

**Batman** is a C# console application that sends random temperature and humidity to IoT Hub every second and receives cloud to device messages.

**Joker** is a C# console application that first of all reads the device twin from IoT Hub, and updates one of the properties. After that, it just sends messages with temperature and a boolean to indicate if the monitored door is open.

**Robin** is a C# console application that sends temperature to IoT Hub passing thru an IoT Edge device used as transparent gateway. You can find more details in *Robin/README.md* file.

In the **Helper** folder:
- *CreateDeviceIdentity* is a C# console application you can use to register your device to your IoT Hub
- *AddTagsAndQuery* is a C# console application that shows you how to add tags to a device twin and how to create querys to retrieve information.