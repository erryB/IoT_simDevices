# Azure Device Provisioning Service #

Azure [Device Provisioning Service](https://docs.microsoft.com/en-us/azure/iot-dps/about-iot-dps) is a helper service for IoT Hub that allows zero-touch and automatic provisioning of your device to tha appropriate ioT Hub, according to the policy you select.

To run this sample you need to:

- create a Device Provisioning Service in your Azure subscription
- add one or more IoT Hub to your DPS 
- have a valid X.509 certificate for your device. You can create one running the GenerateTestCertificate.ps1 at [this link](https://github.com/Azure/azure-iot-sdk-csharp/tree/master/provisioning/device/samples/ProvisioningDeviceClientX509)
- replace the certificate in the AlfredCert folder with the one you just created
- update the App.config of Alfred device with the IDScope of your DPS and the password of your own certificate

If you need more information about Azure Device Provisioning sample you can try the [quickstart tutorials](https://docs.microsoft.com/en-us/azure/iot-dps/quick-setup-auto-provision-cli) and take a look to the [official SDK](https://github.com/Azure/azure-iot-sdk-csharp/tree/master/provisioning).

Enjoy :)