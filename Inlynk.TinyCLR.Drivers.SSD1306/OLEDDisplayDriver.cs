using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drawing;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Inlynk.TinyCLR.Drivers.SSD1306
{
	public class OLEDDisplayDriver : BufferDrawTargetVerticalByteStrip1Bpp
	{
		private readonly byte[] _data;
		private readonly byte[] _command = new byte[2];
		private readonly Timer _refreshBurnoutTimer;
		private readonly I2cDevice i2c;
		public IntPtr Hdc { get; }
		public DateTime _lastUpdated { get; private set; }
		/// <summary>
		/// Constructor for SSD1306 based display. Supports 128x64 and 128x32 displays;
		/// </summary>
		/// <param name="controller">I2cController</param>
		/// <param name="width">Width of the display</param>
		/// <param name="height">Height of device</param>
		/// <param name="timeOut">Minutes to toggle screen to prevent burnout, minimum 5.</param>
		public OLEDDisplayDriver(I2cController controller, int width = 128, int height = 32, int timeOut = 10) : base(width, height)
		{
			_data = new byte[((width * height) / 8) + 1];
			_data[0] = 0x40;
			Hdc = GraphicsManager.RegisterDrawTarget(this);
			i2c = controller.GetDevice(new I2cConnectionSettings(0x3C)
			{
				AddressFormat = I2cAddressFormat.SevenBit,
				BusSpeed = I2cBusSpeed.FastMode,
			});
			Initialize();
			_lastUpdated = DateTime.Now;

			//This timer toggles the screen after a period of time to prevent pixel burnout
			_refreshBurnoutTimer = new Timer((c) =>
			{
				if (timeOut < 5) {
					timeOut = 10;
				}
				if (DateTime.Now > _lastUpdated.AddMinutes(timeOut))
				{
					SendCommand(0xAE);
					Thread.Sleep(200);
					SendCommand(0xAF);
					_lastUpdated = DateTime.Now;
				}

			}, null, 60 * 1000, 5000);

		}

		private void Initialize()
		{
			Thread.Sleep(1500); //Allow display to power up

			//turn off oled panel
			SendCommand(0xAE);
			//set display clock divide ratio/oscillator frequency
			SendCommand(0xD5);
			SendCommand(0x80);
			SendCommand(0xA6);
			SendCommand((byte)(Height - 1));
			SendCommand(0xD3);
			SendCommand(0x00);
			SendCommand(0x40 | 0x00);
			SendCommand(0x8D);
			SendCommand(0x14);
			SendCommand(0x20);
			SendCommand(0x00);
			SendCommand(0xA0 | 0x1);
			SendCommand(0xC8);
			SendCommand(0xDA);
			SendCommand(0x02);
			SendCommand(0x81);
			SendCommand(0x8F);
			SendCommand(0xD9);
			SendCommand(0xF1);
			SendCommand(0xDB);
			SendCommand(0x40);
			SendCommand(0xA4);
			SendCommand(0xA6);
			SendCommand(0x2E);
			SendCommand(0xAF);

			SendCommand(0x20);
			SendCommand(0x00);
			SendCommand(0x21);
			SendCommand(0);
			SendCommand(128 - 1);
			SendCommand(0x22);
			SendCommand(0);
			SendCommand(7);


			Flush();

		}

		private void SendCommand(byte cmd)
		{
			_command[1] = cmd;
			i2c.Write(_command);
		}

		public override void Dispose()
		{
			base.Dispose();
			i2c.Dispose();
			_refreshBurnoutTimer?.Dispose();
		}

		/// <summary>
		/// Writes the data to the OLED display
		/// </summary>
		public override void Flush()
		{
			base.Flush();
			//Clears the data buffer used to write to the display 
			Array.Clear(_data, 1, _data.Length - 1);

			//Writes the cleared buffer to the display
			i2c.Write(_data);

			//Copy the underlying buffer used by Graphics frame 
			Array.Copy(buffer, 0, _data, 1, _data.Length - 1);

			//Writes the buffer to the display
			i2c.Write(_data);

			//Clearing both buffers
			Array.Clear(_data, 1, _data.Length - 1);
			Array.Clear(buffer, 0, buffer.Length);
			//_lastUpdated = DateTime.Now;
		}

		public void Clear()
		{
			Array.Clear(buffer, 0, buffer.Length);
			Flush();
		}
	}
}
