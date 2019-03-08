using System;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Settings.Abstractions;
using Lexi.Interfaces;

namespace ble_library
{
    public class BleSerial : ISerial
    {
        private BlePort ble_port_serial;

        /// <summary>
        /// Initialize Bluetooth LE Serial port
        /// </summary>
        /// <param name="adapter">The Bluetooth Low Energy Adapter from the OS</param>
        public BleSerial(IBluetoothLowEnergyAdapter adapter)
        {     
            ble_port_serial = new BlePort(adapter);
        }

        private void ExceptionCheck(byte[] buffer, int offset, int count){
            if (buffer == null)
            {
                throw new ArgumentException("Parameter cannot be null", nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentException("Parameter cannot be less than Zero", nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentException("Parameter cannot be less than Zero", nameof(count));
            }

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Incorrect buffer size", nameof(buffer));
            }
        }

        /// <summary>
        /// Reads a number of characters from the ISerial input buffer and writes them into an array of characters at a given offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            ExceptionCheck(buffer, offset, count);

            int readedElements = 0;

            try{
                for (int i = 0; i < count; i++)
                {
                    if (ble_port_serial.BytesToRead() == 0)
                    {
                        return readedElements;
                    }
                    buffer[i+offset] = ble_port_serial.GetBufferElement();
                    readedElements++;
                }
            }
            catch (Exception e)
            {
                throw e;
            }  
         
            return readedElements;
        }

        /// <summary>
        /// Writes a specified number of characters to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <remarks></remarks>
        public void Write(byte[] buffer, int offset, int count)
        {
            int bytesToWrite = count;
            int bytesWritten = 0;
            ExceptionCheck(buffer, offset, count);
            do
            {
                int currentWriteCount = 16;
                if (bytesToWrite <= 16)
                {
                    currentWriteCount = bytesToWrite;
                }
                ble_port_serial.ClearBuffer();   // TO-DO
                ble_port_serial.Write_Characteristic(buffer, bytesWritten + offset, currentWriteCount);
                // TODO: check ack before next iteration

                bytesToWrite = bytesToWrite - currentWriteCount;
                bytesWritten = bytesWritten + currentWriteCount;
            }
            while (bytesToWrite > 0);
        }

        /// <summary>
        /// Closes the port connection, sets the <c>IsOpen</c> property to false, and disposes of the internal Stream object.
        /// </summary>
        /// <remarks></remarks>
        public void Close()
        {
            Task.Factory.StartNew(ble_port_serial.DisconnectDevice);
        }

        /// <summary>
        /// Gets a value indicating the open or closed status of the ISerial object.
        /// </summary>
        /// <returns>Boolean value indicating the open or closed status of the ISerial object</returns>
        /// <remarks>The IsOpen property tracks whether the port is open for use by the caller, not whether the port is open by any application on the machine.</remarks>
        public Boolean IsOpen()
        {
            return (ble_port_serial.GetConnectionStatus() == BlePort.CONNECTED);
        }

        /// <summary>
        /// Gets a value indicating the open or closed status of the ISerial object.
        /// </summary>
        /// <returns>Int value indicating the open or closed status of the ISerial object</returns>
        /// <remarks>The IsOpen property tracks whether the port is open for use by the caller, not whether the port is open by any application on the machine.</remarks>
        public int GetConnectionStatus()
        {
            return ble_port_serial.GetConnectionStatus();
        }

        /// <summary>
        /// Gets a value indicating the error on the connectio
        /// </summary>
        /// <returns>int value indicating the error on the connectio</returns>
        public int GetConnectionError()
        {
            return ble_port_serial.GetConnectionError();
        }

        /// <summary>
        /// Starts the device scanning
        /// </summary>
        /// <remarks></remarks>
        public async Task Scan(){
            
            await ble_port_serial.StartScan();
        }

        public Boolean IsScanning()
        {
            return ble_port_serial.IsScanning();
        }

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <param name="blePeripheral">The object that contains the device information.</param>
        /// <remarks></remarks>
        public void Open(IBlePeripheral blePeripheral, bool isBounded = false)
        {
            if(!IsOpen())
            {
                Task.Factory.StartNew(() => ble_port_serial.ConnectoToDevice(blePeripheral,isBounded));
         
            }else{
                // TO-DO: mantenemos la misma conexion o cerramos y volvemos a abrir otra
            }
        }

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <remarks></remarks>
        public void Open()
        {
         
        }

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        /// <returns>Number of bytes of data in the receive buffer</returns>
        /// <remarks>
        /// The receive buffer includes the serial driver's receive buffer as well as internal buffering in the <c>ISerial</c> object itself.
        /// </remarks>
        public int BytesToRead()
        {
            return ble_port_serial.BytesToRead();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public Boolean isEcho()
        {
            return true;
        }

        /// <summary>
        /// Gets the BLE device list
        /// </summary>
        /// <returns>The BLE device list</returns>
        /// <remarks></remarks>
        public List <IBlePeripheral> GetBlePeripheralList() {
            return ble_port_serial.GetBlePeripherals();
        }

        /// <summary>
        /// Gets the BLE device Battery Level
        /// </summary>
        /// <returns>The BLE device Battery Level</returns>
        /// <remarks></remarks>
        public byte[] GetBatteryLevel(){
            return ble_port_serial.GetBatteryLevel();
        }
        /// <summary>
        /// Gets the TimeOut in seconds
        /// </summary>
        /// <returns>Gets the TimeOut in seconds</returns>
        /// <remarks></remarks>
        public void SetTimeOutSeconds(int sec)
        {
            ble_port_serial.TimeOutSeconds = sec;
        }
    }
}