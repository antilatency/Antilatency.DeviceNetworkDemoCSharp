using System;
using System.Threading;
using Antilatency.DeviceNetwork;

namespace DeviceNetworkDemoCSharp {
    class Program {
        static void Main(string[] args) {
            // Load Antilatency Device Network library
            using var deviceNetworkLibrary = Antilatency.DeviceNetwork.Library.load();
            
            // Create device network filter and then create network using that filter.
            var networkFilter = GetAllUsbDevicesFilter(deviceNetworkLibrary);
            using var network = deviceNetworkLibrary.createNetwork(networkFilter);

            // Each time device network is changed due to connection or disconnection of a device that matches the device filter of network,
            // or start or stop of a task on any network's device, the network update id will be incremented by 1. 
            uint prevUpdateId = 0;

            while (true) {
                // Check if network has been changed.
                var currentUpdateId = network.getUpdateId();
                if (currentUpdateId == prevUpdateId) {
                    Thread.Yield();
                    continue;
                }
                
                prevUpdateId = currentUpdateId;
                
                Console.WriteLine($"--- Device network changed, update id: {currentUpdateId} ---");

                // Get the array of currently connected nodes.
                var nodes = network.getNodes();

                // Print some information for each node. Reading the property of newly connected node starts the property task on that node,
                // so you will see additional increments of update id: first at node connection, second at property task start and third when property task finished.
                foreach (var node in nodes) {
                    // Get some property values for node.
                    var hardwareName = network.nodeGetStringProperty(node, Antilatency.DeviceNetwork.Interop.Constants.HardwareNameKey);
                    var hardwareVersion = network.nodeGetStringProperty(node, Antilatency.DeviceNetwork.Interop.Constants.HardwareVersionKey);
                    var hardwareSerialNo = network.nodeGetStringProperty(node, Antilatency.DeviceNetwork.Interop.Constants.HardwareSerialNumberKey);
                    var firmwareName = network.nodeGetStringProperty(node, Antilatency.DeviceNetwork.Interop.Constants.FirmwareNameKey);
                    var firmwareVersion = network.nodeGetStringProperty(node, Antilatency.DeviceNetwork.Interop.Constants.FirmwareVersionKey);
                    
                    // Get node's parent node. When some device (Alt, Tag, Bracer, etc.) connected to the socket, this socket becomes its parent.
                    // Node, that is directly connected to the PC, smartphone, etc. via USB cable, will have null node handle.
                    var parent = network.nodeGetParent(node);
                    var parentStr = parent == Antilatency.DeviceNetwork.NodeHandle.Null ? "Root" : parent.value.ToString();
                    
                    // Get node's status.
                    var status = network.nodeGetStatus(node);
                    
                    Console.WriteLine($"Node: {node.value}");
                    
                    Console.WriteLine($"\tStatus: {status.ToString()}");
                    Console.WriteLine($"\tParent node: {parentStr}");
                    
                    Console.WriteLine($"\tProperties:");
                    Console.WriteLine($"\t\tHardware name: {hardwareName}");
                    Console.WriteLine($"\t\tHardware version: {hardwareVersion}");
                    Console.WriteLine($"\t\tSerial number: {hardwareSerialNo}");
                    Console.WriteLine($"\t\tFirmware name: {firmwareName}");
                    Console.WriteLine($"\t\tFirmware version: {firmwareVersion}");
                    
                    
                    Console.WriteLine("");
                }
            }
        }

        #region DeviceFilter samples
        /*
         * Get device filter for all Antilatency USB devices.
         */
        private static Antilatency.DeviceNetwork.IDeviceFilter GetAllUsbDevicesFilter(Antilatency.DeviceNetwork.ILibrary deviceNetworkLibrary) {
            var result = deviceNetworkLibrary.createFilter();
            result.addUsbDevice(Antilatency.DeviceNetwork.Constants.AllUsbDevices);
            return result;
        }

        /*
         * Get device filter for all IP devices.
         */
        private static Antilatency.DeviceNetwork.IDeviceFilter GetAllIpDevicesFilter(Antilatency.DeviceNetwork.ILibrary deviceNetworkLibrary) {
            var result = deviceNetworkLibrary.createFilter();
            result.addIpDevice(Antilatency.DeviceNetwork.Constants.AllIpDevicesIp, Antilatency.DeviceNetwork.Constants.AllIpDevicesMask);
            return result;
        }

        /*
         * Get device filter for Antilatency USB Sockets (HMD Radio Socket and Wired USB Socket).
         */
        private static Antilatency.DeviceNetwork.IDeviceFilter GetAntilatencyUsbSocketDevicesFilter(Antilatency.DeviceNetwork.ILibrary deviceNetworkLibrary) {
            var result = deviceNetworkLibrary.createFilter();

            var usbDeviceFilter = new Antilatency.DeviceNetwork.UsbDeviceFilter() {
                vid = UsbVendorId.Antilatency, pid = 0x0000, pidMask = 0xFFFF
            };
            result.addUsbDevice(usbDeviceFilter);
            
            return result;
        }
        #endregion
    }
}