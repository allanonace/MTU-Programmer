using System;
using System.Collections.Generic;
using System.Text;

namespace Lexi.Interfaces
{
    /// <summary>
    /// Reads a number of characters from the ISerial input buffer and writes them into an array of characters at a given offset.
    /// </summary>
    /// <example>
    /// Example of USB/FTDI Serial using SerialPort Class Internally
    /// <code>
    /// public class USBSerial : ISerial
    /// {
    ///    SerialPort base_serial;
    /// 
    ///    public USBSerial(string portName)
    ///    {
    ///        base_serial = new SerialPort(portName, 1200, Parity.None, 8, StopBits.Two);
    ///        base_serial.Open();
    ///    }
    /// 
    ///    public int Read(byte[] buffer, int offset, int count)
    ///    {
    ///        return base_serial.Read(buffer, offset, count);
    ///    }
    /// 
    ///    public void Write(byte[] buffer, int offset, int count)
    ///    {
    ///        base_serial.Write(buffer, offset, count);
    ///    }
    /// 
    ///    public void Close()
    ///    {
    ///        base_serial.Close();
    ///    }
    /// 
    ///    public Boolean IsOpen()
    ///    {
    ///        return base_serial.IsOpen;
    ///    }
    /// 
    ///    public void Open()
    ///    {
    ///        base_serial.Open();
    ///    }
    /// 
    ///    public int BytesToRead()
    ///    {
    ///        return base_serial.BytesToRead;
    ///    }
    /// 
    ///    public Boolean isEcho()
    ///    {
    ///        return true;
    ///    }
    /// }
    /// </code>
    /// </example>
    public interface ISerial
    {
        /// <summary>
        /// Reads a number of characters from the ISerial input buffer and writes them into an array of characters at a given offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in buffer at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if count is greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Writes a specified number of characters to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <remarks></remarks>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Closes the port connection, sets the <c>IsOpen</c> property to false, and disposes of the internal Stream object.
        /// </summary>
        /// <remarks></remarks>
        void Close();

        /// <summary>
        /// Gets a value indicating the open or closed status of the ISerial object.
        /// </summary>
        /// <returns>Boolean value indicating the open or closed status of the ISerial object</returns>
        /// <remarks>The IsOpen property tracks whether the port is open for use by the caller, not whether the port is open by any application on the machine.</remarks>
        Boolean IsOpen();

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <remarks></remarks>
        void Open();

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        /// <returns>Number of bytes of data in the receive buffer</returns>
        /// <remarks>
        /// The receive buffer includes the serial driver's receive buffer as well as internal buffering in the <c>ISerial</c> object itself.
        /// </remarks>
        int BytesToRead();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        Boolean isEcho();
    }
}
